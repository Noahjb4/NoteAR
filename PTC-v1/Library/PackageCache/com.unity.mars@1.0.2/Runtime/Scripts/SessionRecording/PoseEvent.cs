using UnityEngine;

namespace Unity.MARS.Recording
{
    /// <summary>
    /// Event for camera tracking recordings
    /// </summary>
    public struct PoseEvent
    {
        public float time;
        public Pose pose;
    }
}
