﻿using System;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Component that contains edge data (start point and end point) for a landmark
    /// </summary>
    public class LandmarkOutputEdge : MonoBehaviour, ILandmarkOutput
    {
        const float k_TransformPositionAlongEdge = 0.5f;

        [SerializeField]
        bool m_ShowEdge = true;

        [SerializeField]
        Color m_EdgeColor = Color.cyan;

        [SerializeField]
        [HideInInspector]
        Vector3[] m_Points = new Vector3[2];

        public Vector3 startPoint
        {
            get { return m_Points[0]; }
            set { m_Points[0] = value; }
        }

        public Vector3 endPoint
        {
            get { return m_Points[1]; }
            set { m_Points[1] = value; }
        }

        public void UpdateOutput()
        {
            var midPoint = Vector3.Lerp(startPoint, endPoint, k_TransformPositionAlongEdge);
            var lookVector = endPoint - startPoint;

            var rotation = lookVector.sqrMagnitude > 0.01f ? Quaternion.LookRotation(lookVector, Vector3.up) : Quaternion.identity;
            var pose = new Pose(midPoint, rotation);
            transform.SetWorldPose(pose);
        }

        void OnDrawGizmosSelected()
        {
            if (!m_ShowEdge || startPoint == endPoint)
                return;

            Gizmos.color = m_EdgeColor;
            Gizmos.DrawLine(startPoint, endPoint);
        }
    }
}
