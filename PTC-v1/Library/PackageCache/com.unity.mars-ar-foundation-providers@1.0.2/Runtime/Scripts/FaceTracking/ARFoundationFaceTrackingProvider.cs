﻿using System;
using System.Collections.Generic;
using Unity.MARS.Data;
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
    class ARFoundationFaceTrackingProvider : IProvidesFaceTracking, IUsesMARSTrackableData<IMRFace>, IProvidesFacialExpressions,
        IProvidesTraits<bool>, IProvidesTraits<Pose>, ITrackableProvider
    {
        static readonly TraitDefinition[] k_ProvidedTraits =
        {
            TraitDefinitions.Face,
            TraitDefinitions.Pose
        };

        ARFaceManager m_ARFaceManager;
        ARFaceManager m_NewARFaceManager;

        readonly Dictionary<TrackableId, ARFoundationFace> m_TrackedFaces = new Dictionary<TrackableId, ARFoundationFace>();

        public event Action<IMRFace> FaceAdded;

        public event Action<IMRFace> FaceUpdated;

        public event Action<IMRFace> FaceRemoved;

        // ReSharper disable once UnusedMember.Local
        static TraitDefinition[] GetStaticProvidedTraits() { return k_ProvidedTraits; }

        public TraitDefinition[] GetProvidedTraits() { return k_ProvidedTraits; }

        void ARFaceManagerOnFacesChanged(ARFacesChangedEventArgs changedEvent)
        {
            foreach (var arFace in changedEvent.removed)
            {
                var trackableId = arFace.trackableId;
                m_TrackedFaces.TryGetValue(trackableId, out var arFoundationFace);
                arFace.ToARFoundationFace(m_ARFaceManager.subsystem, ref arFoundationFace);
                m_TrackedFaces.Remove(trackableId);
                RemoveFaceData(arFoundationFace);
            }

            foreach (var arFace in changedEvent.updated)
            {
                UpdateFaceData(GetOrAddFace(arFace));
            }

            foreach (var arFace in changedEvent.added)
            {
                AddFaceData(GetOrAddFace(arFace));
            }
        }

        ARFoundationFace GetOrAddFace(ARFace arFace)
        {
            var trackableId = arFace.trackableId;
            m_TrackedFaces.TryGetValue(trackableId, out var arFoundationFace);
            arFace.ToARFoundationFace(m_ARFaceManager.subsystem, ref arFoundationFace);
            m_TrackedFaces[trackableId] = arFoundationFace;
            return arFoundationFace;
        }

        void AddFaceData(ARFoundationFace arFoundationFace)
        {
            var id = this.AddOrUpdateData(arFoundationFace);
            this.AddOrUpdateTrait(id, TraitNames.Face, true);
            this.AddOrUpdateTrait(id, TraitNames.Pose, arFoundationFace.pose);

            if (FaceAdded != null)
                FaceAdded(arFoundationFace);

        }

        void UpdateFaceData(ARFoundationFace arFoundationFace)
        {
            var id = this.AddOrUpdateData(arFoundationFace);
            this.AddOrUpdateTrait(id, TraitNames.Pose, arFoundationFace.pose);
            if (FaceUpdated != null)
                FaceUpdated(arFoundationFace);
        }

        void RemoveFaceData(ARFoundationFace arFoundationFace)
        {
            var id = this.RemoveData(arFoundationFace);
            this.RemoveTrait<bool>(id, TraitNames.Face);
            this.RemoveTrait<Pose>(id, TraitNames.Pose);

            var mesh = arFoundationFace.Mesh;
            if (mesh)
                UnityObject.Destroy(mesh);

            if (FaceRemoved != null)
                FaceRemoved(arFoundationFace);
        }

        public void AddExistingTrackables()
        {
#if !UNITY_EDITOR
            if (m_ARFaceManager == null)
                return;

            foreach (var arFace in m_ARFaceManager.trackables)
            {
                AddFaceData(GetOrAddFace(arFace));
            }
#endif
        }

        public void ClearTrackables()
        {
            if (m_ARFaceManager == null)
                return;

            foreach (var kvp in m_TrackedFaces)
            {
                RemoveFaceData(kvp.Value);
            }

            m_TrackedFaces.Clear();
        }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesFaceTracking>(obj);
            this.TryConnectSubscriber<IProvidesFacialExpressions>(obj);
        }

        public void LoadProvider()
        {
            ARFoundationSessionProvider.RequireARSession();

            var currentSession = ARFoundationSessionProvider.currentSession;
            if (currentSession)
            {
                var currentSessionGameObject = currentSession.gameObject;
                m_ARFaceManager = currentSessionGameObject.GetComponent<ARFaceManager>();
                if (!m_ARFaceManager)
                {
                    m_ARFaceManager = currentSessionGameObject.AddComponent<ARFaceManager>();
                    m_NewARFaceManager = m_ARFaceManager;
                }

                m_ARFaceManager.facesChanged += ARFaceManagerOnFacesChanged;
            }

            AddExistingTrackables();
        }

        public void UnloadProvider()
        {
            m_ARFaceManager.facesChanged -= ARFaceManagerOnFacesChanged;

            ClearTrackables();

            if (m_NewARFaceManager)
                UnityObjectUtils.Destroy(m_NewARFaceManager);

            ARFoundationSessionProvider.TearDownARSession();
        }

        public int GetMaxFaceCount()
        {
#if ARFOUNDATION_4_OR_NEWER
            return m_ARFaceManager == null ? 0 : m_ARFaceManager.currentMaximumFaceCount;
#elif ARFOUNDATION_3_0_1_OR_NEWER
            return m_ARFaceManager == null ? 0 : m_ARFaceManager.maximumFaceCount;
#else
            return 1;
#endif
        }

        public void GetFaces(List<IMRFace> faces)
        {
            if (m_ARFaceManager == null)
                return;

            foreach (var arFace in m_ARFaceManager.trackables)
            {
                faces.Add(GetOrAddFace(arFace));
            }
        }

        public void SubscribeToExpression(MRFaceExpression expression, Action<float> engaged, Action<float> disengaged)
        {
#if !UNITY_EDITOR
#if UNITY_IOS && INCLUDE_ARKIT_FACE_PLUGIN
            ARKitFaceExpressionsExtensions.SubscribeToExpression(expression, engaged, disengaged);
#elif UNITY_ANDROID
            ARCoreFaceExpressionsExtensions.SubscribeToExpression(expression, engaged, disengaged);
#endif
#endif
        }

        public void UnsubscribeToExpression(MRFaceExpression expression, Action<float> engaged, Action<float> disengaged)
        {
#if !UNITY_EDITOR
#if UNITY_IOS && INCLUDE_ARKIT_FACE_PLUGIN
            ARKitFaceExpressionsExtensions.UnsubscribeToExpression(expression, engaged, disengaged);
#elif UNITY_ANDROID
            ARCoreFaceExpressionsExtensions.UnsubscribeToExpression(expression, engaged, disengaged);
#endif
#endif
        }
    }
}
