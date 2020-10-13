using System;
using Unity.MARS.Simulation;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS.Providers
{
    [ProviderSelectionOptions(ProviderPriorities.SimulatedProviderPriority)]
    class SimulatedCameraViewProvider : MonoBehaviour, IProvidesCameraTexture, IProvidesCameraIntrinsics,
        IProvidesCameraProjectionMatrix, IUsesSimulationVideoFeed
    {
        Texture m_VideoTexture;

        float? m_FOV;

        IProvidesSimulationVideoFeed IFunctionalitySubscriber<IProvidesSimulationVideoFeed>.provider { get; set; }

        public CameraFacingDirection CameraFacingDirection => m_VideoTexture != null ? this.GetVideoFacingDirection() : CameraFacingDirection.World;

        public float FocalLength { get; private set; }

        public event Action<float> FieldOfViewUpdated;

        public void LoadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesCameraProjectionMatrix>(obj);
            this.TryConnectSubscriber<IProvidesCameraTexture>(obj);
            this.TryConnectSubscriber<IProvidesCameraIntrinsics>(obj);
        }

        public void UnloadProvider() { }

        public Texture GetCameraTexture() { return m_VideoTexture; }

        public void RequestCameraFacingDirection(CameraFacingDirection facingDirection)
        {
            // Not implemented - video feed in simulation is either always user-facing or always world-facing
        }

        public float? GetFieldOfView() { return m_FOV; }

        public Matrix4x4? GetProjectionMatrix() { return null; }

        void OnEnable()
        {
            var videoFeedTexture = this.GetVideoFeedTexture();
            if (videoFeedTexture != null)
                SetupFromVideoFeed(videoFeedTexture);

            this.SubscribeVideoFeedTextureCreated(SetupFromVideoFeed);
        }

        void OnDisable()
        {
            m_VideoTexture = null;
            m_FOV = null;
            this.UnsubscribeVideoFeedTextureCreated(SetupFromVideoFeed);
        }

        void SetupFromVideoFeed(Texture videoFeedTexture)
        {
            m_VideoTexture = videoFeedTexture;
            FocalLength = this.GetVideoFocalLength();
            var halfHeight = videoFeedTexture.height * 0.5f;
            var halfFOV = Mathf.Atan(halfHeight / FocalLength) * Mathf.Rad2Deg;
            m_FOV = halfFOV * 2f;
            FieldOfViewUpdated?.Invoke(m_FOV.Value);
        }
    }
}
