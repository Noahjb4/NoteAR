using System;
using Unity.XRTools.ModuleLoader;

namespace Unity.MARS
{
    /// <summary>
    /// Provides access to camera intrinsics
    /// </summary>
    public interface IUsesCameraIntrinsics : IFunctionalitySubscriber<IProvidesCameraIntrinsics>
    {
    }

    public static class IUsesCameraIntrinsicsMethods
    {
        /// <summary>
        /// Get the focal length of the camera, in pixels
        /// </summary>
        /// <returns>The focal length of the camera, in pixels</returns>
        public static float GetFocalLength(this IUsesCameraIntrinsics obj)
        {
            return obj.provider.FocalLength;
        }

        /// <summary>
        /// Get the field of view of the physical camera
        /// </summary>
        /// <returns>The camera field of view</returns>
        public static float? GetFieldOfView(this IUsesCameraIntrinsics obj)
        {
            return obj.provider.GetFieldOfView();
        }

        /// <summary>
        /// Subscribe to the FieldOfViewUpdated event, which is called when the camera field of view has changed
        /// </summary>
        /// <param name="fieldOfViewUpdated">The delegate to subscribe</param>
        public static void SubscribeFieldOfViewUpdated(this IUsesCameraIntrinsics obj, Action<float> fieldOfViewUpdated)
        {
            obj.provider.FieldOfViewUpdated += fieldOfViewUpdated;
        }

        /// <summary>
        /// Unsubscribe from the FieldOfViewUpdated event, which is called when the camera field of view has changed
        /// </summary>
        /// <param name="fieldOfViewUpdated">The delegate to unsubscribe</param>
        public static void UnsubscribeFieldOfViewUpdated(this IUsesCameraIntrinsics obj, Action<float> fieldOfViewUpdated)
        {
            obj.provider.FieldOfViewUpdated -= fieldOfViewUpdated;
        }
    }
}
