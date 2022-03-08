
namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents an abstract 2D buffer of graphics data
    /// </summary>
    public interface IDisplayBuffer
    {
        /// <summary>
        /// Width in pixels
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Height in pixels
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Total bytes in buffer
        /// </summary>
        int ByteCount { get; }

        /// <summary>
        /// Color mode of buffer
        /// </summary>
        ColorType displayColorMode { get; }

        /// <summary>
        /// The bytes of the buffer data as a byte array
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// Set the Color of a given pixel
        /// </summary>
        /// <param name="x">x location of pixel - 0,0 at yop left</param>
        /// <param name="y">y location of pixel - 0,0 at yop left</param>
        /// <param name="color">color of pixel</param>
        void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Get color of pixel - may be scaled based on buffer color depth
        /// </summary>
        /// <param name="x">x location of pixel - 0,0 at yop left</param>
        /// <param name="y">y location of pixel - 0,0 at yop left</param>
        /// <returns></returns>
        Color GetPixel(int x, int y);

        /// <summary>
        /// Copy a buffer into this buffer
        /// </summary>
        /// <param name="x">target x position to draw buffer</param>
        /// <param name="y">target y position to draw buffer </param>
        /// <param name="buffer">buffer to draw to target</param>
        /// <returns></returns>
        bool WriteBuffer(int x, int y, IDisplayBuffer buffer);

        /// <summary>
        /// Clear the buffer (write 0s to byte array)
        /// </summary>
        void Clear();

        /// <summary>
        /// Fill the buffer with a color
        /// </summary>
        /// <param name="color">color to fill buffer</param>
        void Fill(Color color);

        /// <summary>
        /// Fill a region of the buffer with a color
        /// </summary>
        /// <param name="color">color to fill buffer</param>
        /// <param name="x">x start</param>
        /// <param name="y">y start</param>
        /// <param name="width">width to fill</param>
        /// <param name="height">height to fill</param>
        void Fill(Color color, int x, int y, int width, int height);
    }
}