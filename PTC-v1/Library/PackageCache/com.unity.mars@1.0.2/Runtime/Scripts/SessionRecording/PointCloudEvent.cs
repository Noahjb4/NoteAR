using System.Collections.Generic;

namespace Unity.MARS.Recording
{
    /// <summary>
    /// Event for point cloud recordings
    /// </summary>
    public struct PointCloudEvent
    {
        public float Time;
        public List<SerializedPointCloudData> Data;
    }
}
