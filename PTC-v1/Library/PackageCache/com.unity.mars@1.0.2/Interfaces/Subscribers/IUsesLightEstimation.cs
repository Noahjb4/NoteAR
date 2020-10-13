using System;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Provides access to light estimation features
    /// </summary>
    public interface IUsesLightEstimation : IFunctionalitySubscriber<IProvidesLightEstimation>
    {
    }

    static class IUsesLightEstimationMethods
    {

        /// <summary>
        /// Try to get the light estimation data
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="lightEstimation">The light estimation data</param>
        /// <returns>True if the operation succeeded; false if the data is not available or the feature is not supported</returns>
        public static bool TryGetLightEstimation(this IUsesLightEstimation obj, out MRLightEstimation lightEstimation)
        {
            return obj.provider.TryGetLightEstimation(out lightEstimation);
        }

        public static void SubscribeLightEstimationUpdated(this IUsesLightEstimation obj, Action<MRLightEstimation> lightEstimationUpdated)
        {
            obj.provider.lightEstimationUpdated += lightEstimationUpdated;
        }

        public static void UnsubscribeLightEstimationUpdated(this IUsesLightEstimation obj, Action<MRLightEstimation> lightEstimationUpdated)
        {
            obj.provider.lightEstimationUpdated -= lightEstimationUpdated;
        }
    }
}
