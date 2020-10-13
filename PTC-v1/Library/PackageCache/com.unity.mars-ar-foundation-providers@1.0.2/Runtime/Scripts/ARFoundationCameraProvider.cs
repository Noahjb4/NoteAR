using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityObject = UnityEngine.Object;

namespace Unity.MARS.Providers
{
    [ProviderSelectionOptions(excludedPlatforms: new []{
        RuntimePlatform.WindowsEditor,
        RuntimePlatform.OSXEditor,
        RuntimePlatform.LinuxEditor,
        RuntimePlatform.WindowsPlayer,
        RuntimePlatform.OSXPlayer,
        RuntimePlatform.LinuxPlayer
    })]
    public class ARFoundationCameraProvider : IProvidesCameraTexture, IProvidesCameraProjectionMatrix
    {
        ARCameraManager m_NewARCameraManager;
        ARCameraManager m_ARCameraManager;
        ARCameraBackground m_NewARCameraBackground;

        Texture2D m_CurrentTexture;
        Matrix4x4? m_CurrentProjectionMatrix;

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        // Reference type collections must also be cleared after use
        static readonly List<XRFaceSubsystem> k_FaceSubsystems = new List<XRFaceSubsystem>();

        public CameraFacingDirection CameraFacingDirection
        {
            get
            {
                k_FaceSubsystems.Clear();
                SubsystemManager.GetInstances(k_FaceSubsystems);
                var direction = CameraFacingDirection.World;
                foreach (var faceSubsystem in k_FaceSubsystems)
                {
                    if (faceSubsystem.running)
                    {
                        direction = CameraFacingDirection.User;
                        break;
                    }
                }

                k_FaceSubsystems.Clear();
                return direction;
            }
        }

        public void LoadProvider()
        {
            ARFoundationSessionProvider.RequireARSession();
            InitializeCameraProvider();
        }

        void InitializeCameraProvider()
        {
            var camera = MarsRuntimeUtils.GetActiveCamera(true);

            if (camera)
            {
                if (!camera.GetComponent<ARCameraBackground>())
                {
                    m_NewARCameraBackground = camera.gameObject.AddComponent<ARCameraBackground>();
                    m_NewARCameraBackground.hideFlags = HideFlags.DontSave;
                }

                m_ARCameraManager = camera.GetComponent<ARCameraManager>();

                if (!m_ARCameraManager)
                {
                    m_ARCameraManager = camera.gameObject.AddComponent<ARCameraManager>();
                    m_NewARCameraManager = m_ARCameraManager;
                    m_ARCameraManager.hideFlags = HideFlags.DontSave;
                }

                m_ARCameraManager.frameReceived += ARCameraManagerOnFrameReceived;
            }

            m_CurrentTexture = null;
            m_CurrentProjectionMatrix = null;
        }

        void ARCameraManagerOnFrameReceived(ARCameraFrameEventArgs cameraFrameEvent)
        {
            m_CurrentProjectionMatrix = cameraFrameEvent.projectionMatrix;

            m_CurrentTexture = cameraFrameEvent.textures.Count > 0 ? cameraFrameEvent.textures[0] : null;
        }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesCameraTexture>(obj);
            this.TryConnectSubscriber<IProvidesCameraProjectionMatrix>(obj);
        }

        public void UnloadProvider()
        {
            ShutdownCameraProvider();
            ARFoundationSessionProvider.TearDownARSession();
        }

        void ShutdownCameraProvider()
        {
            m_ARCameraManager.frameReceived -= ARCameraManagerOnFrameReceived;

            if (m_NewARCameraBackground)
                UnityObjectUtils.Destroy(m_NewARCameraBackground);

            if (m_NewARCameraManager)
                UnityObjectUtils.Destroy(m_NewARCameraManager);
        }

        public Matrix4x4? GetProjectionMatrix()
        {
            return m_CurrentProjectionMatrix;
        }

        public Texture GetCameraTexture()
        {
            return m_CurrentTexture;
        }

        public void RequestCameraFacingDirection(CameraFacingDirection facingDirection)
        {
            // AR Foundation version 3 has no way to explicitly request which camera to use.
            // This method should be implemented when this package uses AR Foundation version 4.
        }
    }
}
