﻿using Unity.MARS.Query;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Base for action classes that change an object's transform
    /// Serves just to keep users from putting more than one of these on at at ime
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class TransformAction : MonoBehaviour, IMatchLossHandler
    {
#pragma warning disable 649
        [SerializeField]
        [Tooltip("When enabled, the object's pose and scale will be reset if the data for it is lost")]
        bool m_ResetOnLoss;
#pragma warning restore 649

        Pose m_OriginalPose;
        Vector3 m_OriginalScale;

        protected virtual void Awake()
        {
            m_OriginalPose = transform.GetWorldPose();
            m_OriginalScale = transform.localScale;
        }

        protected virtual void OnDisable()
        {
            if (!m_ResetOnLoss)
                return;

            transform.SetWorldPose(m_OriginalPose);
            transform.localScale = m_OriginalScale;
        }

        public virtual void OnMatchLoss(QueryResult queryResult)
        {
            if (!m_ResetOnLoss)
                return;

            transform.SetWorldPose(m_OriginalPose);
            transform.localScale = m_OriginalScale;
        }
    }
}
