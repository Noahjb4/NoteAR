﻿using System;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace Unity.MARS
{
    /// <summary>
    /// Module that handles placing objects in the scene onto interaction targets
    /// when dragging objects from the project window and translating objects while holding Shift
    /// </summary>
    public class ScenePlacementModule : IModuleDependency<SceneViewInteractionModule>, IModuleDependency<SimulationSceneModule>
    {
        const string k_UndoPlaceObjectMessage = "Place Object";
        const string k_AddMarsSessionMessage = "The scene you are editing does not contain a MARS Session. Would you like to add one now?";
        const string k_NoSessionTitle = "No MARS Session";
        const string k_AddSessionButton = "Add MARS Session";
        const string k_CancelButton = "Cancel";
        const string k_OkButton = "OK";
        const string k_AssetsCanOnlyBeDroppedOntoSimulatedDataMessage = "Assets can only be dropped onto simulated data (surfaces, etc.) in the Simulation View.";
        const string k_DropAssetsOnSimulatedDataTitle = "Drop assets on simulated data";

        public class PlacementOverrideData
        {
            public bool useSnapToPivotOverride { get; private set; }
            public bool snapToPivotOverride { get; private set; }
            public bool useOrientToSurfaceOverride { get; private set; }
            public bool orientToSurfaceOverride { get; private set; }
            public bool useAxisOverride { get; private set; }
            public AxisEnum axisOverride { get; private set; }
            public Vector3 axisOverrideVector { get; private set; }

            public void ResetData()
            {
                useSnapToPivotOverride = false;
                useOrientToSurfaceOverride = false;
                useAxisOverride = false;
                axisOverride = AxisEnum.None;
            }

            void SetSnapToPivot(bool use, bool value)
            {
                if (useSnapToPivotOverride || !use)
                    return;

                useSnapToPivotOverride = true;
                snapToPivotOverride = value;
            }

            void SetOrientToSurface(bool use, bool value)
            {
                if (useOrientToSurfaceOverride || !use)
                    return;

                useOrientToSurfaceOverride = true;
                orientToSurfaceOverride = value;
            }

            void SetAxisOverride(bool use, AxisEnum value, Vector3 vector)
            {
                if (useAxisOverride || !use)
                    return;

                useAxisOverride = true;
                axisOverride = value;
                axisOverrideVector = vector;
            }

            public void SetOverrideData(PlacementOverride overrides)
            {
                SetSnapToPivot(overrides.useSnapToPivotOverride, overrides.snapToPivotOverride);
                SetOrientToSurface(overrides.useOrientToSurfaceOverride, overrides.orientToSurfaceOverride);
                SetAxisOverride(overrides.useAxisOverride, overrides.axisOverride, overrides.axisOverrideVector);
            }
        }

        static readonly int k_DropPosShaderID = Shader.PropertyToID("_DropPos");
        static readonly string k_TypeName = typeof(ScenePlacementModule).FullName;

        static SceneView s_CurrentHoveredSceneView;

        /// <summary>
        /// Action when a gameobject is dropped onto another gameobject and attached to it.
        /// </summary>
        public event Action<GameObject, GameObject> ObjectDropped;

        SceneViewInteractionModule m_SceneViewInteraction;
        SimulationSceneModule m_SimulationSceneModule;

        GameObject m_NewlyAddedObject;
        GameObject m_CurrentDraggedObject;
        GameObject m_PlacementPreviewObject;
        PivotMode m_PivotModeCache;
        Vector3 m_OrientAxisDirection;
        PlacementOverrideData m_PlacementOverrideData = new PlacementOverrideData();

        public PlacementOverrideData PlacementOverrides { get; } = new PlacementOverrideData();

        public bool isDragging => m_CurrentDraggedObject != null;

        /// <summary>
        /// If enabled, objects being placed in the editor should orient to the surface
        /// </summary>
        public bool orientToSurface
        {
            get
            {
                if (PlacementOverrides.useOrientToSurfaceOverride)
                    return PlacementOverrides.orientToSurfaceOverride;

                var toSurface = EditorPrefsUtils.GetBool(k_TypeName);
                return Event.current.control ? !toSurface : toSurface;
            }
            set => EditorPrefsUtils.SetBool(k_TypeName, value);
        }

        /// <summary>
        /// If enabled, objects being placed in the editor should snap to the pivot of the target
        /// </summary>
        public bool snapToPivot
        {
            get
            {
                if (PlacementOverrides.useSnapToPivotOverride)
                    return PlacementOverrides.snapToPivotOverride;

                var toPivot = EditorPrefsUtils.GetBool(k_TypeName);
                return Event.current.alt ? !toPivot : toPivot;
            }
            set => EditorPrefsUtils.SetBool(k_TypeName, value);
        }

        /// <summary>
        /// Axis for the up direction of an object being oriented to a surface
        /// </summary>
        public AxisEnum orientAxis
        {
            get
            {
                return PlacementOverrides.useAxisOverride
                    ? PlacementOverrides.axisOverride
                    : (AxisEnum) EditorPrefsUtils.GetInt(k_TypeName);
            }
            set
            {
                if (m_CurrentDraggedObject != null)
                    return;

                SetOrientAxisDirection(value);
                EditorPrefsUtils.SetInt(k_TypeName, (int) value);
            }
        }

        /// <summary>
        /// The up axis of the object that will be aligned with the normal when orienting to surface
        /// </summary>
        Vector3 OrientAxisDirection
        {
            get
            {
                return PlacementOverrides.useAxisOverride
                    ? PlacementOverrides.axisOverrideVector
                    : m_OrientAxisDirection;
            }
        }

        public void ConnectDependency(SceneViewInteractionModule dependency)
        {
            m_SceneViewInteraction = dependency;
            m_SceneViewInteraction.HoverBegin += StartHover;
            m_SceneViewInteraction.HoverUpdate += UpdateHover;
            m_SceneViewInteraction.HoverEnd += EndHover;
        }

        public void ConnectDependency(SimulationSceneModule dependency)
        {
            m_SimulationSceneModule = dependency;
        }

        public void LoadModule()
        {
            SceneView.duringSceneGui += OnSceneGUI;

            SetOrientAxisDirection(orientAxis);
            ScenePlacementGUI.AddPlacementSettingsToToolbar();
        }

        public void UnloadModule()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            if (m_PlacementPreviewObject != null)
                UnityObjectUtils.Destroy(m_PlacementPreviewObject);

            if (m_CurrentDraggedObject != null)
                UnityObjectUtils.Destroy(m_CurrentDraggedObject);

#if UNITY_2019_3 // 19.3 specific.
            ScenePlacementGUI.RemovePlacementSettingsFromToolbar();
#endif
            if (m_SceneViewInteraction != null)
            {
                m_SceneViewInteraction.HoverBegin -= StartHover;
                m_SceneViewInteraction.HoverUpdate -= UpdateHover;
                m_SceneViewInteraction.HoverEnd -= EndHover;
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (EditorWindow.mouseOverWindow == sceneView)// Avoid running the dragging placement logic on scene views the mouse is not over
            {
                s_CurrentHoveredSceneView = sceneView;

                CheckNewlyAddedObject(sceneView);

                var currentEvent = Event.current;
                if (m_NewlyAddedObject != null)
                    m_CurrentDraggedObject = m_NewlyAddedObject;
                else if (IsMovingSelection() && m_CurrentDraggedObject == null && currentEvent.shift)
                    m_CurrentDraggedObject = Selection.activeGameObject;
            }

            InteractionTargetCheck();
            SetToolForView(sceneView);
            DisableEntityVisualCollision();
        }

        void InteractionTargetCheck()
        {
            m_SceneViewInteraction.RequestInteraction(this, m_CurrentDraggedObject != null);
            var currentEvent = Event.current;
            if ((currentEvent.type == EventType.DragPerform || currentEvent.rawType == EventType.MouseUp) &&
                m_CurrentDraggedObject != null)
            {
                var hoverTarget = m_SceneViewInteraction.CurrentHoverTarget;
                if (hoverTarget != null)
                {
                    OnDrop(hoverTarget, m_CurrentDraggedObject);
                }
                else if (m_CurrentDraggedObject.scene == m_SimulationSceneModule.ContentScene)
                {
                    if (MarsRuntimeUtils.GetMarsSessionInActiveScene() == null)
                    {
                        var addNewSession = EditorUtility.DisplayDialog(k_NoSessionTitle, k_AddMarsSessionMessage,
                            k_AddSessionButton, k_CancelButton);
                        if (addNewSession)
                            MARSSession.EnsureSessionInActiveScene();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(k_DropAssetsOnSimulatedDataTitle, k_AssetsCanOnlyBeDroppedOntoSimulatedDataMessage, k_OkButton);
                    }

                    // Destroy the object and consume the GUI drop event so the editor doesn't try to use the destroyed object
                    // Not using delay call will cause glitch where the next object dragged moves towards camera.
                    EditorApplication.delayCall += () => UnityObjectUtils.Destroy(m_CurrentDraggedObject);
                    Event.current.Use();
                }

                m_NewlyAddedObject = null;
                m_CurrentDraggedObject = null;
            }
        }

        void SetToolForView(SceneView sceneView)
        {
            var currentEvent = Event.current;
            if (m_CurrentDraggedObject != null && currentEvent.type == EventType.DragUpdated)
            {
                if (sceneView is SimulationView)
                    EditorTools.SetActiveTool<MarsCreateTool>();
                else if (EditorTools.activeToolType == typeof(MarsCreateTool))
                    EditorTools.RestorePreviousTool();
            }
        }

        void UpdatePlacementOverrides()
        {
            // Interaction target placement overrides will be given priority
            m_PlacementOverrideData.ResetData();
            var targetPlacementOverride = m_SceneViewInteraction.CurrentHoverTarget.GetComponent<PlacementOverride>();
            if (targetPlacementOverride != null)
                m_PlacementOverrideData.SetOverrideData(targetPlacementOverride);

            if (m_CurrentDraggedObject != null)
            {
                var objectPlacementOverride = m_CurrentDraggedObject.GetComponent<PlacementOverride>();
                if (objectPlacementOverride != null)
                    m_PlacementOverrideData.SetOverrideData(objectPlacementOverride);
            }
        }

        void StartHover(InteractionTarget target)
        {
            m_PivotModeCache = Tools.pivotMode;
            Tools.pivotMode = PivotMode.Pivot;
            UpdatePlacementOverrides();
        }

        void UpdateHover(InteractionTarget target, RaycastHit hit)
        {
            Shader.SetGlobalVector(k_DropPosShaderID, hit.point);

            if (m_CurrentDraggedObject != null)
            {
                if (m_PlacementPreviewObject == null)
                    CreatePlacementPreviewObject(m_CurrentDraggedObject);

                m_CurrentDraggedObject.SetActive(false);
                m_PlacementPreviewObject.SetActive(true);

                var targetRotation = Quaternion.identity;
                if (orientToSurface)
                    targetRotation = GetRotationForSurfaceAndAxis(hit.normal, OrientAxisDirection);

                m_PlacementPreviewObject.transform.rotation = targetRotation;
                m_PlacementPreviewObject.transform.position = snapToPivot ? target.transform.position : hit.point;
            }
        }

        static Quaternion GetRotationForSurfaceAndAxis(Vector3 surfaceNormal, Vector3 axis)
        {
            return Quaternion.LookRotation(surfaceNormal, Vector3.up)
                   * Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, axis));
        }

        void CreatePlacementPreviewObject(GameObject dropObject)
        {
            dropObject.SetActive(false);
            m_PlacementPreviewObject = UnityObject.Instantiate(dropObject, dropObject.transform.parent);
            if (dropObject.scene.IsValid() && dropObject.scene != m_PlacementPreviewObject.scene)
                SceneManager.MoveGameObjectToScene(m_PlacementPreviewObject, dropObject.scene);

            m_PlacementPreviewObject.SetHideFlagsRecursively(HideFlags.HideAndDontSave);
            foreach (var collider in m_PlacementPreviewObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }
        }

        void EndHover(InteractionTarget target)
        {
            Shader.SetGlobalVector(k_DropPosShaderID, Vector3.zero);

            if (m_PlacementPreviewObject != null)
                UnityObjectUtils.Destroy(m_PlacementPreviewObject);

            if(m_CurrentDraggedObject != null)
                m_CurrentDraggedObject.SetActive(true);

            PlacementOverrides.ResetData();
            Tools.pivotMode = m_PivotModeCache;
            target.SetHovered(false);
        }

        void OnDrop(InteractionTarget target, GameObject droppedObj)
        {
            droppedObj.SetActive(true);
            if (m_PlacementPreviewObject != null)
            {
                Undo.RecordObject(droppedObj.transform, k_UndoPlaceObjectMessage);
                droppedObj.transform.position = m_PlacementPreviewObject.transform.position;
                droppedObj.transform.rotation = m_PlacementPreviewObject.transform.rotation;
            }

            if (droppedObj.transform.parent != target.AttachTarget)
            {
                if (m_NewlyAddedObject == droppedObj)
                    droppedObj.transform.SetParent(target.AttachTarget);
                else
                    Undo.SetTransformParent(droppedObj.transform, target.AttachTarget, k_UndoPlaceObjectMessage);


                if (ObjectDropped != null)
                {
                    var currentHoveredSimView = s_CurrentHoveredSceneView as SimulationView;
                    if (currentHoveredSimView != null && currentHoveredSimView.EnvironmentSceneActive)
                    {
                        droppedObj.SetLayerRecursively(SimulationConstants.SimulatedEnvironmentLayerIndex);
                    }
                    else
                    {
                        ObjectDropped(droppedObj,
                            target.AttachTarget == null ? null : target.AttachTarget.gameObject);
                    }
                }

                EditorApplication.DirtyHierarchyWindowSorting();
                EditorApplication.RepaintHierarchyWindow();
                EditorGUIUtility.PingObject(droppedObj);
            }

            var targetType = "unknown";
            var targetLabel = "unknown";
            var faceInteractionTarget = target as FaceLandmarkInteractionTarget;
            if (faceInteractionTarget != null)
            {
                targetType = "face landmark";
                targetLabel = faceInteractionTarget.landmark.ToString();
            }

            EditorEvents.DropTargetUsed.Send(new DropTargetUsedArgs
            {
                type = targetType,
                label = targetLabel
            });
        }

        static bool IsMovingSelection()
        {
            var selectedTransform = Selection.activeTransform;
            if (Event.current.type == EventType.MouseDrag && selectedTransform != null && Tools.current == Tool.Move &&
                selectedTransform.hasChanged)
            {
                selectedTransform.hasChanged = false;
                return true;
            }

            return false;
        }

        void CheckNewlyAddedObject(SceneView sceneView)
        {
            if (Event.current.type != EventType.DragUpdated)
                return;

            var draggedObjects = DragAndDrop.objectReferences;
            if (draggedObjects.Length == 0)
            {
                m_NewlyAddedObject = null;
            }
            else if (m_NewlyAddedObject == null) // Adding something, but the object has not been determined
            {
                var scene = sceneView.camera.scene;
                if (!scene.IsValid())
                    scene = SceneManager.GetActiveScene();

                var gameObjs = scene.GetRootGameObjects();
                foreach (var go in gameObjs)
                {
                    if (go.hideFlags == HideFlags.HideInHierarchy && go.name == draggedObjects[0].name)
                    {
                        go.transform.SetParent(null, true);
                        m_NewlyAddedObject = go;
                        break;
                    }
                }
            }
        }

        void DisableEntityVisualCollision()
        {
            if (m_CurrentDraggedObject != null)
            {
                var entities = m_CurrentDraggedObject.GetComponentsInChildren<MARSEntity>();
                foreach (var entity in entities)
                {
                    EntityVisualsModule.instance.DisableVisualCollision(entity);
                }
            }
            else
            {
                EntityVisualsModule.instance.ResetVisualCollision();
            }
        }

        void SetOrientAxisDirection(AxisEnum axis)
        {
            switch (axis)
            {
                case AxisEnum.XUp:
                    m_OrientAxisDirection = Vector3.right;
                    break;
                case AxisEnum.XDown:
                    m_OrientAxisDirection = Vector3.left;
                    break;
                case AxisEnum.YUp:
                    m_OrientAxisDirection = Vector3.up;
                    break;
                case AxisEnum.YDown:
                    m_OrientAxisDirection = Vector3.down;
                    break;
                case AxisEnum.ZUp:
                    m_OrientAxisDirection = Vector3.forward;
                    break;
                case AxisEnum.ZDown:
                    m_OrientAxisDirection = Vector3.back;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
