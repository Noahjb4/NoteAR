﻿using System;
using Unity.MARS.Data;
using Unity.MARS.Query;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS
{
    [RequireComponent(typeof(Proxy))]
    [ComponentTooltip("Sets the position of this GameObject to the position of the found real-world object.")]
    [MonoBehaviourComponentMenu(typeof(SetPoseAction), "Action/Set Pose")]
    public class SetPoseAction : TransformAction, IMatchAcquireHandler, IMatchUpdateHandler, IRequiresTraits
    {
        /// <summary>
        /// The mode that will the set pose action will use.
        /// </summary>
        public enum AlignMode
        {
            None = 0,
            Y,
            Z,
        }

        [SerializeField]
        [Tooltip("When enabled, movement of the matched data, such as surface-resizing, will be followed")]
        public bool FollowMatchUpdates = true;

        [SerializeField]
        [Tooltip("The axis that will be aligned to the world up direction. If set to None (default), the data's pose rotation will be used directly." +
            "If set to Y, the local Y axis will always be aligned with the world Up direction." +
            "If set to Z, the local Z axis will always be aligned with the world Up direction.")]
        public AlignMode AlignWithWorldUp = AlignMode.None;

        static readonly TraitRequirement[] k_RequiredTraits = { TraitDefinitions.Pose };

        public void OnMatchAcquire(QueryResult queryResult)
        {
            UpdatePosition(queryResult);
        }

        public void OnMatchUpdate(QueryResult queryResult)
        {
            if (FollowMatchUpdates)
            {
                UpdatePosition(queryResult);
            }
        }

        void UpdatePosition(QueryResult queryResult)
        {
            if ((!this) || (!enabled))
                return;

            if (queryResult.TryGetTrait(TraitNames.Pose, out Pose newPose))
            {
                if (AlignWithWorldUp != AlignMode.None)
                {
                    var localForward = newPose.up;
                    localForward.y = 0f; // Make sure new forward is orthogonal to up
                    if (localForward == Vector3.zero) // Vector3 == has a built-in approximate check
                        localForward = newPose.forward;

                    switch (AlignWithWorldUp)
                    {
                        case AlignMode.Y: // If data's Y is not aligned to world up, then make local Y world up use projected data Y as local forward
                            newPose.rotation = Quaternion.LookRotation(localForward, Vector3.up);
                            break;
                        case AlignMode.Z: // If data's Z is not aligned to world up, then make local Z world up and use data Y or Z as local up
                            newPose.rotation = Quaternion.LookRotation(Vector3.up, localForward);
                            break;
                    }
                }

                transform.SetWorldPose(newPose);
            }
        }

        public TraitRequirement[] GetRequiredTraits()
        {
            return k_RequiredTraits;
        }
    }
}
