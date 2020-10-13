using UnityEngine.XR.ARFoundation;

namespace Unity.MARS.Providers
{
    public static class XRRaycastHitExtensions
    {
        public static MRHitTestResult ToMRHitTestResult(this ARRaycastHit hit)
        {
            return new MRHitTestResult
            {
                pose = hit.pose
            };
        }
    }
}
