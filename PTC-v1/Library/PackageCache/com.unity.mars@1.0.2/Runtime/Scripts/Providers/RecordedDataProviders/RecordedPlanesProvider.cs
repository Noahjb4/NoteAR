﻿using System;
using System.Collections.Generic;
using Unity.MARS.Data;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.Playables;

namespace Unity.MARS.Recording.Providers
{
    [ProviderSelectionOptions(ProviderPriorities.RecordedProviderPriority, disallowAutoCreation:true)]
    public class RecordedPlanesProvider : RecordedTrackablesProvider, IProvidesPlaneFinding, IUsesMARSTrackableData<MRPlane>,
        IProvidesTraits<Pose>, IProvidesTraits<Vector2>, IProvidesTraits<int>, IProvidesTraits<bool>, INotificationReceiver
    {
        static readonly TraitDefinition[] k_ProvidedTraits =
        {
            TraitDefinitions.Plane,
            TraitDefinitions.Pose,
            TraitDefinitions.Bounds2D,
            TraitDefinitions.Alignment
        };

        readonly Dictionary<MarsTrackableId, MRPlane> m_Planes = new Dictionary<MarsTrackableId, MRPlane>();
        readonly List<PlaneEventMarker> m_BufferedPlaneEvents = new List<PlaneEventMarker>();

        bool m_Paused;

        public event Action<MRPlane> planeAdded;
        public event Action<MRPlane> planeUpdated;
        public event Action<MRPlane> planeRemoved;

        public TraitDefinition[] GetProvidedTraits() { return k_ProvidedTraits; }

        public void LoadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesPlaneFinding>(obj);
        }

        public void UnloadProvider() { }

        public override void ClearData()
        {
            foreach (var kvp in m_Planes)
            {
                RemovePlaneData(kvp.Value);
            }

            m_Planes.Clear();
            m_BufferedPlaneEvents.Clear();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ClearData();
            m_Paused = false;
        }

        public void GetPlanes(List<MRPlane> planes)
        {
            foreach (var kvp in m_Planes)
            {
                planes.Add(kvp.Value);
            }
        }

        public void StopDetectingPlanes() { m_Paused = true; }

        public void StartDetectingPlanes()
        {
            m_Paused = false;
            foreach (var planeEvent in m_BufferedPlaneEvents)
            {
                ProcessPlaneEvent(planeEvent);
            }

            m_BufferedPlaneEvents.Clear();
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            var planeEvent = notification as PlaneEventMarker;
            if (planeEvent == null)
                return;

            if (m_Paused)
                m_BufferedPlaneEvents.Add(planeEvent);
            else
                ProcessPlaneEvent(planeEvent);
        }

        void ProcessPlaneEvent(PlaneEventMarker planeEvent)
        {
            switch (planeEvent.EventType)
            {
                case TrackableEventType.Added:
                    AddPlane(planeEvent.Plane);
                    break;
                case TrackableEventType.Updated:
                    UpdatePlane(planeEvent.Plane);
                    break;
                case TrackableEventType.Removed:
                    RemovePlane(planeEvent.Plane);
                    break;
            }
        }

        void AddPlane(MRPlane plane)
        {
            var playbackID = GetPlaybackID(plane.id);
            plane.id = playbackID;
            m_Planes[playbackID] = plane;
            var id = this.AddOrUpdateData(plane);
            this.AddOrUpdateTrait(id, TraitNames.Plane, true);
            this.AddOrUpdateTrait(id, TraitNames.Pose, plane.pose);
            this.AddOrUpdateTrait(id, TraitNames.Bounds2D, plane.extents);
            this.AddOrUpdateTrait(id, TraitNames.Alignment, (int)plane.alignment);
            if (planeAdded != null)
                planeAdded(plane);
        }

        void UpdatePlane(MRPlane plane)
        {
            var playbackID = GetPlaybackID(plane.id);
            plane.id = playbackID;
            m_Planes[playbackID] = plane;
            var id = this.AddOrUpdateData(plane);
            this.AddOrUpdateTrait(id, TraitNames.Pose, plane.pose);
            this.AddOrUpdateTrait(id, TraitNames.Bounds2D, plane.extents);
            if (planeUpdated != null)
                planeUpdated(plane);
        }

        void RemovePlane(MRPlane plane)
        {
            var playbackID = GetPlaybackID(plane.id);
            plane.id = playbackID;
            m_Planes.Remove(playbackID);
            RemovePlaneData(plane);
        }

        void RemovePlaneData(MRPlane plane)
        {
            var id = this.RemoveData(plane);
            this.RemoveTrait<bool>(id, TraitNames.Plane);
            this.RemoveTrait<Pose>(id, TraitNames.Pose);
            this.RemoveTrait<Vector2>(id, TraitNames.Bounds2D);
            this.RemoveTrait<int>(id, TraitNames.Alignment);
            if (planeRemoved != null)
                planeRemoved(plane);
        }
    }
}
