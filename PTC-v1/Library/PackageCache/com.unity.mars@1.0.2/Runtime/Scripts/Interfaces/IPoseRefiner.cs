using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Declares that a MonoBehaviour can refine a pose
    /// </summary>
    public interface IPoseRefiner
    {
        Pose RefinePose(Pose pose, bool leaveProxyInNewPose);
    }
}
