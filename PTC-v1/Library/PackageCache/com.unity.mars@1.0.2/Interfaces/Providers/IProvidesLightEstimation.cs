using System;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Defines the API for a Light Estimation Provider
    /// This functionality provider is responsible for light estimation
    /// </summary>
    public interface IProvidesLightEstimation : IFunctionalityProvider
    {
        /// <summary>
        /// Called when the light estimation changes
        /// </summary>
        event Action<MRLightEstimation> lightEstimationUpdated;

        /// <summary>
        /// Try to get the light estimation data
        /// </summary>
        /// <param name="lightEstimation">The light estimation data</param>
        /// <returns></returns>
        bool TryGetLightEstimation(out MRLightEstimation lightEstimation);
    }
}
