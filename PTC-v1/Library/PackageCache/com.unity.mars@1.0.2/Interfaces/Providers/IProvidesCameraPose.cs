﻿using System;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Defines the API for a Camera Pose Provider
    /// This functionality provider is responsible for 3dof/6dof camera tracking data
    /// </summary>
    public interface IProvidesCameraPose : IFunctionalityProvider
    {
        /// <summary>
        /// Get the current camera pose
        /// </summary>
        /// <returns>The current camera pose</returns>
        Pose GetCameraPose();

        /// <summary>
        /// Called when the camera pose changes
        /// </summary>
        event Action<Pose> poseUpdated;

        /// <summary>
        /// Called when the type of tracking changes, for example from 6dof to 3dof
        /// </summary>
        event Action<MRCameraTrackingState> trackingStateChanged;
    }
}
