using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS
{
#if UNITY_EDITOR
    [ProviderSelectionOptions(ProviderPriorities.SimulatedProviderPriority, disallowAutoCreation:true)]
    public class SimulatedDiscoverySessionProvider : MonoBehaviour, IProvidesSessionControl
    {
#pragma warning disable 649
        [SerializeField]
        SimulatedDiscoveryPlanesProvider m_PlanesProvider;

        [SerializeField]
        SimulatedDiscoveryPointCloudProvider m_PointCloudProvider;
#pragma warning restore 649

        public bool Destroyed { get; private set; }
        public bool Paused { get; private set; }

        public void LoadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesSessionControl>(obj);
        }

        public void UnloadProvider() { }

        public bool SessionExists() { return !Destroyed; }

        public bool SessionRunning() { return !Destroyed && !Paused; }

        public bool SessionReady() { return true; }

        public void CreateSession() { Destroyed = false; }

        public void DestroySession() { Destroyed = true; }

        public void ResetSession()
        {
            if (m_PlanesProvider != null)
                m_PlanesProvider.ResetPlanes();

            if (m_PointCloudProvider != null)
                m_PointCloudProvider.ClearPoints();
        }

        public void PauseSession()
        {
            Paused = true;
        }

        public void ResumeSession()
        {
            Paused = false;
        }
    }
#else
    public class SimulatedDiscoverySessionProvider : MonoBehaviour
    {
        [SerializeField]
        SimulatedDiscoveryPlanesProvider m_PlanesProvider;

        [SerializeField]
        SimulatedDiscoveryPointCloudProvider m_PointCloudProvider;
    }
#endif
}
