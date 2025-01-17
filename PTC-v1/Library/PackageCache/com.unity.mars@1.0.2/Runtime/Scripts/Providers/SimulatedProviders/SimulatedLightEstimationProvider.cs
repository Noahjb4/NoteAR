using System;
using Unity.MARS.Data;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS.Providers
{
#if UNITY_EDITOR
    [ProviderSelectionOptions(ProviderPriorities.SimulatedProviderPriority)]
    public class SimulatedLightEstimationProvider : MonoBehaviour, IProvidesLightEstimation
    {
        public event Action<MRLightEstimation> lightEstimationUpdated;

        SynthesizedLightEstimation m_SynthLightEstimation;
        MRLightEstimation m_LightEstimation;

        public void LoadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesLightEstimation>(obj);
        }

        public void UnloadProvider() { }

        public void RegisterSynthesizedLightEstimation(SynthesizedLightEstimation synthLightEstimation)
        {
            this.m_SynthLightEstimation = synthLightEstimation;
        }

        public void UnregisterSythensizedLightEstimation(SynthesizedLightEstimation synthLightEstimation)
        {
            if (this.m_SynthLightEstimation == synthLightEstimation)
                this.m_SynthLightEstimation = null;
        }

        public bool TryGetLightEstimation(out MRLightEstimation lightEstimation)
        {
            lightEstimation = default(MRLightEstimation);
            return false;
        }

        void Update()
        {
            if (m_SynthLightEstimation)
            {
                var lightEstimation = m_SynthLightEstimation.GetData();
                if (!m_LightEstimation.AreLightsEqual(lightEstimation))
                {
                    m_LightEstimation = lightEstimation;
                    lightEstimationUpdated?.Invoke(m_LightEstimation);
                }
            }
        }

    }
#else
public class SimulatedLightEstimationProvider : MonoBehaviour { }
#endif
}
