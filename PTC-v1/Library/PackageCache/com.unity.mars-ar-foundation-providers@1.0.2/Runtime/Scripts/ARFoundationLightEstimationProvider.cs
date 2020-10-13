using System;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

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

    public class ARFoundationLightEstimationProvider : IProvidesLightEstimation
    {
        ARCameraManager m_ARCameraManager;
        ARCameraManager m_NewARCameraManager;
        MRLightEstimation m_LightEstimation;

        public event Action<MRLightEstimation> lightEstimationUpdated;

        public void LoadProvider()
        {
            ARFoundationSessionProvider.RequireARSession();

            var camera = MarsRuntimeUtils.GetActiveCamera(true);
            if (camera)
            {
                m_ARCameraManager = camera.gameObject.GetComponent<ARCameraManager>();
                if (!m_ARCameraManager)
                {
                    m_ARCameraManager = camera.gameObject.AddComponent<ARCameraManager>();
                    m_NewARCameraManager = m_ARCameraManager;
                    m_ARCameraManager.hideFlags = HideFlags.DontSave;
                }

#if ARFOUNDATION_4_OR_NEWER
                m_ARCameraManager.requestedLightEstimation = LightEstimation.AmbientColor
                    | LightEstimation.AmbientIntensity
                    | LightEstimation.AmbientSphericalHarmonics
                    | LightEstimation.MainLightIntensity
                    | LightEstimation.MainLightDirection;
#else
                m_ARCameraManager.lightEstimationMode = LightEstimationMode.AmbientIntensity;
#endif
            }
            else
            {
                throw new InvalidOperationException("No camera found");
            }

            m_ARCameraManager.frameReceived += ARCameraFrameEvent;
        }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesLightEstimation>(obj);
        }

        public void UnloadProvider()
        {
            m_ARCameraManager.frameReceived -= ARCameraFrameEvent;
            if (m_NewARCameraManager)
                UnityObjectUtils.Destroy(m_NewARCameraManager);
            ARFoundationSessionProvider.TearDownARSession();
        }

        void ARCameraFrameEvent(ARCameraFrameEventArgs args)
        {
            var lightEstimation = args.lightEstimation;
            m_LightEstimation.m_AmbientBrightness = lightEstimation.averageBrightness;
            m_LightEstimation.m_AmbientColorTemperature = lightEstimation.averageColorTemperature;
            m_LightEstimation.m_ColorCorrection = lightEstimation.colorCorrection;

#if ARFOUNDATION_3_0_1_OR_NEWER
            m_LightEstimation.m_AmbientIntensityInLumens = lightEstimation.averageIntensityInLumens;
#endif

#if ARFOUNDATION_4_OR_NEWER
            m_LightEstimation.m_MainLightBrightness = lightEstimation.averageMainLightBrightness;
            m_LightEstimation.m_MainLightColor = lightEstimation.mainLightColor;
            m_LightEstimation.m_MainLightDirection = lightEstimation.mainLightDirection;
            m_LightEstimation.m_MainLightIntensityLumens = lightEstimation.mainLightIntensityLumens;
            m_LightEstimation.m_SphericalHarmonics = lightEstimation.ambientSphericalHarmonics;
#endif

            lightEstimationUpdated?.Invoke(m_LightEstimation);
        }

        public bool TryGetLightEstimation(out MRLightEstimation lightEstimation)
        {
            lightEstimation = m_LightEstimation;
            return true;
        }

        public void ClearTrackables() { throw new NotImplementedException(); }
        public void AddExistingTrackables() { throw new NotImplementedException(); }
    }
}
