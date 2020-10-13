﻿using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS.Data
{
    [CreateAssetMenu(menuName = "MARS/In View ReasoningAPI")]
    public class InViewReasoningAPI : ScriptableObject, IReasoningAPI, IProvidesTraits<bool>,
        IUsesMARSTrackableData<MRPlane>, IRequiresTraits, IUsesCameraOffset
    {
        static readonly TraitDefinition[] k_ProvidedTraits = { TraitDefinitions.InView };
        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.Plane };

        [SerializeField]
        [Tooltip("Sets the number of seconds to wait between processing scene data")]
        float m_ProcessSceneInterval = 1f;

        ICollection<KeyValuePair<int, MRPlane>> m_PlaneCollection;
        readonly HashSet<int> m_PlanesInView = new HashSet<int>();
        Camera m_MainCamera;

        IProvidesCameraOffset IFunctionalitySubscriber<IProvidesCameraOffset>.provider { get; set; }

        public float processSceneInterval { get { return m_ProcessSceneInterval; } }

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<Vector3> k_WorldVertices = new List<Vector3>();

        public TraitDefinition[] GetProvidedTraits() { return k_ProvidedTraits; }

        public TraitRequirement[] GetRequiredTraits() { return k_RequiredTraits; }

        public void Setup()
        {
            m_PlaneCollection = this.GetCollection();
            m_PlanesInView.Clear();
            m_MainCamera = MarsRuntimeUtils.GetActiveCamera(true);
        }

        public void TearDown() { }

        public void ProcessScene()
        {
            var frustumPlanes = GeometryUtility.CalculateFrustumPlanes(m_MainCamera);

            foreach (var kvp in m_PlaneCollection)
            {
                var planeID = kvp.Key;
                var plane = kvp.Value;
                var planePose = plane.pose;

                k_WorldVertices.Clear();
                var vertices = plane.vertices;
                if (vertices == null)
                {
                    var center = plane.center;
                    var halfExtents = plane.extents * 0.5f;
                    var vert1 = center + new Vector3(halfExtents.x, 0f, halfExtents.y);
                    var vert2 = center + new Vector3(halfExtents.x, 0f, -halfExtents.y);
                    var vert3 = center + new Vector3(-halfExtents.x, 0f, -halfExtents.y);
                    var vert4 = center + new Vector3(-halfExtents.x, 0f, halfExtents.y);
                    k_WorldVertices.Add(this.ApplyOffsetToPosition(planePose.ApplyOffsetTo(vert1)));
                    k_WorldVertices.Add(this.ApplyOffsetToPosition(planePose.ApplyOffsetTo(vert2)));
                    k_WorldVertices.Add(this.ApplyOffsetToPosition(planePose.ApplyOffsetTo(vert3)));
                    k_WorldVertices.Add(this.ApplyOffsetToPosition(planePose.ApplyOffsetTo(vert4)));
                }
                else
                {
                    foreach (var vertex in vertices)
                    {
                        k_WorldVertices.Add(this.ApplyOffsetToPosition(planePose.ApplyOffsetTo(vertex)));
                    }
                }

                var inView = GeometryUtility.TestPlanesAABB(frustumPlanes, BoundsUtils.GetBounds(k_WorldVertices));
                if (!inView && m_PlanesInView.Contains(planeID))
                {
                    this.RemoveTrait(planeID, TraitNames.InView);
                    m_PlanesInView.Remove(planeID);
                }
                else if (inView && !m_PlanesInView.Contains(planeID))
                {
                    this.AddOrUpdateTrait(planeID, TraitNames.InView, true);
                    m_PlanesInView.Add(planeID);
                }
            }
        }

        public void UpdateData() { }
    }
}
