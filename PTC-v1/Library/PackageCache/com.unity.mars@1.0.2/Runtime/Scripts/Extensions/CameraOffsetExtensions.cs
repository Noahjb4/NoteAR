﻿using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.MARS
{
    public static class CameraOffsetExtensions
    {
        public static Vector3 PoseToCameraSpace(this IUsesCameraOffset obj, Pose pose, Vector3 point)
        {
            return obj.ApplyOffsetToPosition(pose.ApplyOffsetTo(point));
        }
    }
}
