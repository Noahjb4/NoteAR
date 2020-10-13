using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.MARS
{
    /// <summary>
    /// Provides access to a camera texture
    /// </summary>
    public interface IUsesCameraTexture : IFunctionalitySubscriber<IProvidesCameraTexture>
    {
    }

    public static class IUsesCameraTextureMethods
    {
        /// <summary>
        /// Get the current facing direction of the camera used to supply the texture
        /// </summary>
        public static CameraFacingDirection GetCameraFacingDirection(this IUsesCameraTexture obj)
        {
            return obj.provider.CameraFacingDirection;
        }

        /// <summary>
        /// Get the current camera texture
        /// </summary>
        /// <returns>The current camera texture</returns>
        public static Texture GetCameraTexture(this IUsesCameraTexture obj)
        {
            return obj.provider.GetCameraTexture();
        }

        /// <summary>
        /// Request that the provider use the camera with the given facing direction, if possible
        /// </summary>
        /// <param name="facingDirection">The requested camera facing direction</param>
        public static void RequestCameraFacingDirection(this IUsesCameraTexture obj, CameraFacingDirection facingDirection)
        {
            obj.provider.RequestCameraFacingDirection(facingDirection);
        }
    }
}
