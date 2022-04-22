using System.IO;
using System.Threading.Tasks;

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
        public bool CapturePhoto();

        /// <summary>
        /// Check if there is picture data on the camera
        /// </summary>
        /// <returns>true is data is avaliable</returns>
        public bool IsPhotoAvaliable();

        /// <summary>
        /// Get the picture data from the camera
        /// </summary>
        /// <returns>the picture data as a byte array</returns>
        public Task<byte[]> GetPhotoData();

        /// <summary>
        /// Get the picture data from the camera
        /// </summary>
        /// <returns>the picture data as a memory stream</returns>
        public Task<MemoryStream> GetPhotoStream();
    }
}