using UnityEngine;

namespace Unity.XRTools.ModuleLoader.Example
{
    // ReSharper disable once UnusedMember.Global
    class LoggingProvider : IProvidesLogging
    {
        public void LoadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesLogging>(obj);
        }

        public void UnloadProvider() { }

        public void Log(object obj)
        {
            Debug.Log(obj);
        }
    }
}
