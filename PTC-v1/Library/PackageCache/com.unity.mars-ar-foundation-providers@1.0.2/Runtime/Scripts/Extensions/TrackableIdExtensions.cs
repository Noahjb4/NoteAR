using UnityEngine.XR.ARSubsystems;

namespace Unity.MARS.Providers
{
    public static class TrackableIdExtensions
    {
        public static MarsTrackableId ToMarsId(this TrackableId id)
        {
            return new MarsTrackableId(id.subId1, id.subId2);
        }
    }
}
