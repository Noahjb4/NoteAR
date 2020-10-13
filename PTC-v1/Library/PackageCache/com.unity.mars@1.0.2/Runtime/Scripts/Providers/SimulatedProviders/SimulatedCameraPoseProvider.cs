using System;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS.Providers
{
    [ProviderSelectionOptions(ProviderPriorities.SimulatedProviderPriority)]
    public class SimulatedCameraPoseProvider : MonoBehaviour, IProvidesCameraPose, IUsesDeviceSimulationSettings
    {
        [SerializeField]
        bool m_UseMovementBoundsInGame = true;

        CameraFPSModeHandler m_FPSModeHandler;

        public event Action<Pose> poseUpdated;
#pragma warning disable 67
        public event Action<MRCameraTrackingState> trackingStateChanged;
#pragma warning restore 67

        IProvidesDeviceSimulationSettings IFunctionalitySubscriber<IProvidesDeviceSimulationSettings>.provider { get; set; }

        public void LoadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesCameraPose>(obj);
        }

        public void UnloadProvider() { }

        public Pose GetCameraPose() { return transform.GetLocalPose(); }

        void OnEnable()
        {
            if (Application.isPlaying)
            {
                m_FPSModeHandler = new CameraFPSModeHandler();
                this.SubscribeEnvironmentChanged(OnEnvironmentChanged);
                return;
            }

            transform.SetWorldPose(this.GetDeviceStartingPose());
        }

        void OnDisable()
        {
            if (Application.isPlaying)
                this.UnsubscribeEnvironmentChanged(OnEnvironmentChanged);
        }

        void OnEnvironmentChanged()
        {
            if (Application.isPlaying)
            {
                transform.SetWorldPose(this.GetDeviceStartingPose());
                poseUpdated?.Invoke(GetCameraPose());
            }
        }

        void Update()
        {
            if (!this.GetIsMovementEnabled())
                return;

            var playing = Application.isPlaying;
            var fpsModeContext = playing ? m_FPSModeHandler : CameraFPSModeHandler.activeHandler;

            if (fpsModeContext == null)
                return;

            if (playing)
            {
                fpsModeContext.UseMovementBounds = m_UseMovementBoundsInGame;
                fpsModeContext.MovementBounds = this.GetEnvironmentBounds();
                fpsModeContext.HandleGameInput();
            }

            if (!fpsModeContext.MoveActive)
                return;

            var pose = fpsModeContext.CalculateMovement(transform.GetWorldPose(), playing);
            transform.SetWorldPose(pose);
            if (poseUpdated != null)
                poseUpdated(GetCameraPose());
        }
    }
}
