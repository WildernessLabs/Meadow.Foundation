namespace Meadow.Foundation.Sensors.Camera
{
    /// <summary>
    /// Interface for camera sensors
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// Capture a new image
        /// </summary>
        /// <returns>true if successful</returns>
        public bool TakePicture();

        /// <summary>
        /// Check if there is picture data on the camera
        /// </summary>
        /// <returns>true is data is avaliable</returns>
        //public bool IsPictureDataAvaliable();
    }
}