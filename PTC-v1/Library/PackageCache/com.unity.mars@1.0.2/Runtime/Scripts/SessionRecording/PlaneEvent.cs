namespace Unity.MARS.Recording
{
    /// <summary>
    /// Event for plane recordings
    /// </summary>
    public struct PlaneEvent
    {
        public float time;
        public MRPlane plane;
        public TrackableEventType eventType;
    }
}
