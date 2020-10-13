using UnityEngine;

namespace Unity.MARS.Recording
{
    /// <summary>
    /// Event for face tracking recordings
    /// </summary>
    public struct FaceEvent
    {
        public double Time;
        public SerializedFaceData FaceData;
        public TrackableEventType EventType;
    }
}
