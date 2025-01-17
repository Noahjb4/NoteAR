using System;
using System.Collections.Generic;
using Unity.MARS.Data;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

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
    class ARFoundationMarkerProvider : IProvidesMarkerTracking, IUsesMARSTrackableData<MRMarker>,
        IProvidesTraits<bool>, IProvidesTraits<Pose>, IProvidesTraits<Vector2>, IProvidesTraits<string>
    {
        static readonly TraitDefinition[] k_ProvidedTraits =
        {
            TraitDefinitions.Marker,
            TraitDefinitions.Pose,
            TraitDefinitions.Bounds2D,
            TraitDefinitions.MarkerId
        };

        ARTrackedImageManager m_ARTrackedImageManager;
        ARTrackedImageManager m_NewARTrackedImageManager;

        public event Action<MRMarker> markerAdded;
        public event Action<MRMarker> markerUpdated;
        public event Action<MRMarker> markerRemoved;

        static TraitDefinition[] GetStaticProvidedTraits() { return k_ProvidedTraits; }
        public TraitDefinition[] GetProvidedTraits() { return k_ProvidedTraits; }
        public TraitDefinition[] GetRequiredTraits() { return null; }

        public void LoadProvider()
        {
            ARFoundationSessionProvider.RequireARSession();

            var currentSession = ARFoundationSessionProvider.currentSession;
            if (currentSession)
            {
                var currentSessionGameObject = currentSession.gameObject;
                m_ARTrackedImageManager = currentSessionGameObject.GetComponent<ARTrackedImageManager>();
                if (!m_ARTrackedImageManager)
                {
                    m_ARTrackedImageManager = currentSessionGameObject.AddComponent<ARTrackedImageManager>();
                    m_ARTrackedImageManager.maxNumberOfMovingImages = 4;
                    m_NewARTrackedImageManager = m_ARTrackedImageManager;
                }

                m_ARTrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
            }

            AddExistingMarkers();
        }

        void AddExistingMarkers()
        {
            if (m_ARTrackedImageManager == null)
                return;

            foreach (var trackedImage in m_ARTrackedImageManager.trackables)
            {
                AddTrackedImage(trackedImage.ToMRMarker());
            }
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs trackedImagesChangedEvent)
        {
            foreach (var trackedImage in trackedImagesChangedEvent.removed)
            {
                var mrMarker = trackedImage.ToMRMarker();
                RemoveTrackedImage(mrMarker);
            }

            foreach (var trackedImage in trackedImagesChangedEvent.updated)
            {
                var mrMarker = trackedImage.ToMRMarker();
                UpdateTrackedImage(mrMarker);
            }

            foreach (var trackedImage in trackedImagesChangedEvent.added)
            {
                var mrMarker = trackedImage.ToMRMarker();
                AddTrackedImage(mrMarker);
            }
        }

        void RemoveTrackedImage(MRMarker mrMarker)
        {
            var id = this.RemoveData(mrMarker);
            this.RemoveTrait<bool>(id, TraitNames.Marker);
            this.RemoveTrait<Pose>(id, TraitNames.Pose);
            this.RemoveTrait<Vector2>(id, TraitNames.Bounds2D);

            if (markerRemoved != null)
                markerRemoved(mrMarker);
        }

        void UpdateTrackedImage(MRMarker mrMarker)
        {
            var id = this.AddOrUpdateData(mrMarker);
            this.AddOrUpdateTrait(id, TraitNames.Pose, mrMarker.pose);

            if (markerUpdated != null)
                markerUpdated(mrMarker);
        }

        void AddTrackedImage(MRMarker mrMarker)
        {
            var id = this.AddOrUpdateData(mrMarker);
            this.AddOrUpdateTrait(id, TraitNames.Marker, true);
            this.AddOrUpdateTrait(id, TraitNames.Pose, mrMarker.pose);
            this.AddOrUpdateTrait(id, TraitNames.Bounds2D, mrMarker.extents);
            this.AddOrUpdateTrait(id, TraitNames.MarkerId, mrMarker.markerId.ToString());

            if (markerAdded != null)
                markerAdded(mrMarker);
        }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesMarkerTracking>(obj);
        }

        public void UnloadProvider()
        {
            if (m_ARTrackedImageManager == null)
                return;

            m_ARTrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;

            if (m_NewARTrackedImageManager)
                UnityObjectUtils.Destroy(m_NewARTrackedImageManager);
        }

        public void StopTrackingMarkers()
        {
            if (m_ARTrackedImageManager == null)
                return;

            m_ARTrackedImageManager.subsystem?.Stop();
        }

        public void StartTrackingMarkers()
        {
            if (m_ARTrackedImageManager == null)
                return;

            m_ARTrackedImageManager.subsystem?.Start();
        }

        public bool SetActiveMarkerLibrary(IMRMarkerLibrary activeLibrary)
        {
            var activeMarsLibrary = (MarsMarkerLibrary) activeLibrary;
            if (activeMarsLibrary == null)
                return false;

            if (!MarkerProviderSettings.instance.TryFind(activeMarsLibrary, out var xrReferenceLibrary))
            {
                Debug.LogWarning("Could not find XR Reference Library for " + activeMarsLibrary);
                return false;
            }

            // Check for image manager after trying to find library so that warning happens in play mode
            if (m_ARTrackedImageManager == null)
                return false;

            m_ARTrackedImageManager.referenceLibrary = xrReferenceLibrary;

            // When we start with a null reference library, our manager could be disabled
            if (xrReferenceLibrary != null && m_ARTrackedImageManager.enabled == false)
                m_ARTrackedImageManager.enabled = true;

            return true;
        }

        public void GetMarkers(List<MRMarker> markers)
        {
            if (m_ARTrackedImageManager == null)
                return;

            foreach (var trackedImage in m_ARTrackedImageManager.trackables)
            {
                markers.Add(trackedImage.ToMRMarker());
            }
        }
    }
}
