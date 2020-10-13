namespace Unity.MARS
{
    /// <summary>
    /// Represents the camera used when supplying the video feed.
    /// </summary>
    public enum CameraFacingDirection
    {
        /// <summary>
        /// No video feed should be provided.
        /// </summary>
        None,

        /// <summary>
        /// Provide the video feed from the world-facing camera. On a phone, this is usually the rear camera.
        /// </summary>
        World,

        /// <summary>
        /// Provide the video feed from the user-facing camera. On a phone, this is usually the front (i.e., "selfie") camera.
        /// </summary>
        User
    }
}
