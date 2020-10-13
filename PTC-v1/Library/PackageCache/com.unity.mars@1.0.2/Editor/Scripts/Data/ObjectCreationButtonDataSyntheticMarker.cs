using UnityEngine;
using UnityEditor;
using Unity.XRTools.Utils;
using Unity.MARS.Data;
using Unity.XRTools.ModuleLoader;

namespace Unity.MARS
{
    class ObjectCreationButtonDataSyntheticMarker : ObjectCreationButtonData
    {
        const string k_SimulatedMarkersParentName = "Simulated Markers";

#pragma warning disable 649
        [SerializeField]
        Material m_SyntheticMarkersMaterial;

        [SerializeField]
        Mesh m_SyntheticMarkersMesh;
#pragma warning restore 649

        public override bool CreateGameObject()
        {
            MARSSession.EnsureRuntimeState();

            SimulationView activeView =  SimulationView.ActiveSimulationView as SimulationView;

            if(activeView != null)
                activeView.EnvironmentSceneActive = true;

            Tools.current = Tool.Move;

            using (var undoBlock = new UndoBlock("Synthetic Marker Creation"))
            {
                var imageMarkerParent = GetImageMarkerParent();
                if (imageMarkerParent == null)
                {
                    Debug.LogWarning("Unable to create synthetic marker.");
                    return false;
                }

                var syntheticMarker = CreateSyntheticMarker();
                var markerTransform = syntheticMarker.transform;
                markerTransform.parent = imageMarkerParent;

                // Zero out position and rotation, leave scale in case the user is working with other scale.
                markerTransform.localPosition = Vector3.zero;
                markerTransform.localRotation = Quaternion.identity;

                markerTransform.name = GameObjectUtility.GetUniqueNameForSibling(imageMarkerParent, m_ButtonName);

                undoBlock.RegisterCreatedObject(markerTransform.gameObject);

                Selection.activeTransform = markerTransform;

                var initialScale = Vector2.one * MARSWorldScaleModule.GetWorldScale();
                syntheticMarker.UpdateMarkerSize(initialScale);

                markerTransform.localScale = new Vector3(1, 0.01f, 1);
            }

            return true;
        }

        SynthesizedMarkerId CreateSyntheticMarker()
        {
            var synthesizedImageMarkerObj = new GameObject("Simulated marker");
            synthesizedImageMarkerObj.layer = SimulationConstants.SimulatedEnvironmentLayerIndex;

            synthesizedImageMarkerObj.SetActive(false);

            // Rest of components added by RequireComponent inside SynthesizedMarker
            synthesizedImageMarkerObj.AddComponent<SynthesizedMarker>();

            synthesizedImageMarkerObj.AddComponent<MeshRenderer>();
            synthesizedImageMarkerObj.AddComponent<MeshFilter>().sharedMesh = m_SyntheticMarkersMesh;
            synthesizedImageMarkerObj.AddComponent<MeshCollider>();

            var synthesizedMarkerId = synthesizedImageMarkerObj.GetComponent<SynthesizedMarkerId>();
            synthesizedMarkerId.Initialize(m_SyntheticMarkersMaterial);

            synthesizedImageMarkerObj.SetActive(true);

            // Without delay, the non-instant framing gets lost in the object creation hiccup.
            EditorApplication.delayCall += () =>
            {
                SimulationView.ActiveSimulationView.Frame(
                    BoundsUtils.GetBounds(synthesizedImageMarkerObj.transform), false);
            };

            return synthesizedMarkerId;
        }

        static Transform GetImageMarkerParent()
        {
            var environmentManager = ModuleLoaderCore.instance.GetModule<MARSEnvironmentManager>();
            var syntheticEnvironmentPrefabInstanceRoot = environmentManager.CurrentLoadedPrefabInstanceEnvironment;
            if (syntheticEnvironmentPrefabInstanceRoot == null)
                return null;

            var resultParent = syntheticEnvironmentPrefabInstanceRoot.transform.Find(k_SimulatedMarkersParentName);
            if (resultParent == null)
            {
                resultParent = new GameObject(k_SimulatedMarkersParentName).transform;

                Undo.RegisterCreatedObjectUndo(resultParent.gameObject, $"Create {resultParent.name}");

                resultParent.gameObject.layer = SimulationConstants.SimulatedEnvironmentLayerIndex;
            }

            resultParent.parent = syntheticEnvironmentPrefabInstanceRoot.transform;

            // Zero out the transform group in case the mars session is not zeroed out
            resultParent.localPosition = Vector3.zero;
            resultParent.localRotation = Quaternion.identity;
            resultParent.localScale = Vector3.one;

            return resultParent;
        }
    }
}
