using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if INCLUDE_POST_PROCESSING
using UnityEngine.Rendering.PostProcessing;
#endif

namespace Unity.MARS
{
    /// <summary>
    /// Holds environment scene settings
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlaneExtractionSettings))]
    public class MARSEnvironmentSettings : MonoBehaviour
    {
        [SerializeField]
        MARSEnvironmentInfo m_EnvironmentInfo = new MARSEnvironmentInfo();

#pragma warning disable 649
        [SerializeField]
        SimulationRenderSettings m_RenderSettings;

        [SerializeField]
#if INCLUDE_POST_PROCESSING
        PostProcessProfile m_PostProcessProfile;
#else
        [HideInInspector]
        ScriptableObject m_PostProcessProfile;
#endif
#pragma warning restore 649

        public MARSEnvironmentInfo EnvironmentInfo => m_EnvironmentInfo;

        /// <summary>
        /// Get the MARSEnvironmentSettings from the GameObject and create settings if none was found.
        /// </summary>
        /// <param name="gameObject">Root game object we are checking for settings</param>
        /// <param name="environmentSettings">Environment Settings that is associated with the game object</param>
        /// <returns>True if settings was found.</returns>
        public static bool GetOrCreateSettings(GameObject gameObject, out MARSEnvironmentSettings environmentSettings)
        {
            // Check for existing object
            environmentSettings = gameObject.GetComponentInChildren<MARSEnvironmentSettings>();

            if (environmentSettings == null)
            {
                Debug.Log("MARS Environment Settings not present in the gameObject - creating one now.");
                environmentSettings = gameObject.AddComponent<MARSEnvironmentSettings>();
                return false;
            }

            return true;
        }

        void Awake()
        {
            if (!Application.isPlaying)
                return;

            var fiModule = ModuleLoaderCore.instance.GetModule<FunctionalityInjectionModule>();

            if (fiModule == null)
                return;

            fiModule.activeIsland.InjectFunctionality(gameObject);
        }

#if UNITY_EDITOR
        void OnEnable()
        {
            CompositeRenderRuntimeUtils.AssignSimulationRenderSettings(gameObject.scene, m_RenderSettings);
            CompositeRenderRuntimeUtils.AssignImageEffectSettings(m_PostProcessProfile);
        }

        void OnDisable()
        {
            CompositeRenderRuntimeUtils.ClearSimulationRenderSettings(gameObject.scene);

            if (CompositeRenderRuntimeUtils.ImageEffectSettings == m_PostProcessProfile)
                CompositeRenderRuntimeUtils.ClearImageEffectSettings();
        }
#endif

        public void UpdatePrefabInfo()
        {
            var bounds = BoundsUtils.GetBounds(gameObject.transform);
            var startingPose = m_EnvironmentInfo.SimulationStartingPose;
            if (!bounds.Contains(startingPose.position))
            {
                Debug.LogWarningFormat(
                    "Simulation starting pose for environment '{0}' is outside the total bounds of the environment. " +
                    "The starting pose will be moved to the closest point on the bounds.", gameObject.name);
                startingPose.position = bounds.ClosestPoint(startingPose.position);
                m_EnvironmentInfo.SimulationStartingPose = startingPose;
            }

            m_EnvironmentInfo.EnvironmentBounds = bounds;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Sets the default camera settings based on the current scene view camera
        /// </summary>
        /// <param name="sceneView">Scene view that we are getting the camera from</param>
        /// <param name="isSimView">Is the scene view a simulation view</param>
        public void SetDefaultEnvironmentCamera(SceneView sceneView, bool isSimView)
        {
            MARSSession.EnsureRuntimeState();
            var simCamera = sceneView.camera;
            var cameraTransform = simCamera.transform;
            if (isSimView)
            {
                var cameraScale = MARSSession.GetWorldScale();
                var camPose = new Pose(cameraTransform.position / cameraScale, cameraTransform.rotation);

                m_EnvironmentInfo.DefaultCameraWorldPose = camPose;
                m_EnvironmentInfo.DefaultCameraPivot = sceneView.pivot / cameraScale;
                m_EnvironmentInfo.DefaultCameraSize = sceneView.size / cameraScale;
            }
            else
            {
                m_EnvironmentInfo.DefaultCameraWorldPose = cameraTransform.GetWorldPose();
                m_EnvironmentInfo.DefaultCameraPivot = sceneView.pivot;
                m_EnvironmentInfo.DefaultCameraSize = sceneView.size;
            }
        }

        /// <summary>
        /// Sets the simulated device starting pose based on the current scene view camera
        /// </summary>
        /// <param name="cameraPose">Camera pose we using</param>
        /// <param name="isSimView">Is this camera from a simulation view</param>
        public void SetSimulationStartingPose(Pose cameraPose, bool isSimView)
        {
            MARSSession.EnsureRuntimeState();
            if (isSimView)
            {
                var cameraScale = MARSSession.GetWorldScale();
                cameraPose.position /= cameraScale;
            }

            m_EnvironmentInfo.SimulationStartingPose = cameraPose;
        }
#endif
    }
}
