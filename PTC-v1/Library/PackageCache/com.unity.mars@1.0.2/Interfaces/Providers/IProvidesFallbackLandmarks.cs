using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS
{
    public interface IProvidesFallbackLandmarks : IFunctionalityProvider
    {
        /// <summary>
        /// Gets the set of fallback poses
        /// </summary>
        /// <returns>Dictionary of landmark name to local pose</returns>
        Dictionary<MRFaceLandmark, Pose> GetFallbackFaceLandmarkPoses();
    }
}
