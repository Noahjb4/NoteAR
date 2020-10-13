using Unity.MARS.Providers;
using UnityEngine;

namespace Unity.MARS.Data
{
    public class SynthesizedLightEstimation : MonoBehaviour, ISimulatable
    {
        [SerializeField]
        Color m_Color = Color.white;

        [SerializeField]
        float m_Intensity = 1f;

        [SerializeField]
        bool m_Directional = false;

        MRLightEstimation m_LightEstimation = default(MRLightEstimation);

        void OnEnable()
        {
#if UNITY_EDITOR
            foreach (var provider in Resources.FindObjectsOfTypeAll<SimulatedLightEstimationProvider>())
                provider.RegisterSynthesizedLightEstimation(this);
#endif
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            foreach (var provider in Resources.FindObjectsOfTypeAll<SimulatedLightEstimationProvider>())
                provider.UnregisterSythensizedLightEstimation(this);
#endif
        }

        public MRLightEstimation GetData()
        {
            if (m_Directional)
            {
                m_LightEstimation.m_AmbientBrightness = null;
                m_LightEstimation.m_ColorCorrection = null;

                m_LightEstimation.m_MainLightBrightness = m_Intensity;
                m_LightEstimation.m_MainLightColor = m_Color;
                m_LightEstimation.m_MainLightDirection = transform.forward;
            }
            else
            {
                m_LightEstimation.m_AmbientBrightness = m_Intensity;
                m_LightEstimation.m_ColorCorrection = m_Color;

                m_LightEstimation.m_MainLightBrightness = null;
                m_LightEstimation.m_MainLightColor = null;
                m_LightEstimation.m_MainLightDirection = null;
            }

            return m_LightEstimation;
        }
    }
}
