﻿using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
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
    public class ARFoundationSessionProvider : MonoBehaviour, IProvidesSessionControl
    {
        static ARSession s_TemporarySession;
        static bool s_SharedSession;
        public static ARSession currentSession
        {
            get { return s_TemporarySession; }
        }

        public static void RequireARSession()
        {
            if (s_TemporarySession)
                return;

            ARSession session = FindObjectOfType<ARSession>();
            if (session)
            {
                s_SharedSession = true;
                s_TemporarySession = session;
            }
            else
            {
                s_SharedSession = false;
                CreateSessionObject();
                s_TemporarySession.hideFlags = HideFlags.DontSave;
            }
        }

        public static void TearDownARSession()
        {
            if (s_TemporarySession && !s_SharedSession)
            {
                UnityObjectUtils.Destroy(s_TemporarySession.gameObject);
            }
            s_TemporarySession = null;
        }

        public bool SessionExists()
        {
            return Resources.FindObjectsOfTypeAll<ARSession>().Length > 0;
        }

        public bool SessionReady()
        {
            return ARSession.state == ARSessionState.Ready;
        }

        public bool SessionRunning()
        {
            var arSessions = Resources.FindObjectsOfTypeAll<ARSession>();
            if (arSessions.Length > 0)
                return arSessions[0].enabled;

            return false;
        }

        public void CreateSession()
        {
            var arSessions = Resources.FindObjectsOfTypeAll<ARSession>();
            if (arSessions.Length == 0)
                CreateSessionObject();
        }

        static void CreateSessionObject()
        {
            s_TemporarySession = new GameObject("AR Session").AddComponent<ARSession>();
            s_TemporarySession.gameObject.AddComponent<ARInputManager>();
        }

        public void DestroySession()
        {
            var arSessions = Resources.FindObjectsOfTypeAll<ARSession>();
            if (arSessions.Length > 0)
                UnityObjectUtils.Destroy(arSessions[0].gameObject);
        }

        public void ResetSession()
        {
            var fiModule = ModuleLoaderCore.instance.GetModule<FunctionalityInjectionModule>();
            foreach (var island in fiModule.islands)
            {
                foreach (var kvp in island.providers)
                {
                    if (kvp.Value is ITrackableProvider planeProvider)
                        planeProvider.ClearTrackables();
                }
            }

            var arSessions = Resources.FindObjectsOfTypeAll<ARSession>();
            if (arSessions.Length > 0)
                arSessions[0].Reset();

            foreach (var island in fiModule.islands)
            {
                foreach (var kvp in island.providers)
                {
                    if (kvp.Value is ITrackableProvider trackableProvider)
                        trackableProvider.AddExistingTrackables();
                }
            }
        }

        public void PauseSession()
        {
            var arSessions = Resources.FindObjectsOfTypeAll<ARSession>();
            if (arSessions.Length > 0)
                arSessions[0].enabled = false;
        }

        public void ResumeSession()
        {
            var arSessions = Resources.FindObjectsOfTypeAll<ARSession>();
            if (arSessions.Length > 0)
                arSessions[0].enabled = true;
        }

        public void LoadProvider() {}

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesSessionControl>(obj);
        }

        public void UnloadProvider() {}
    }
}
