namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// IPixelBuffer provides a standard interface for
    /// representing the display state of a device capable of
    /// displaying pixels. It specifies methods for performing
    /// common primitive operations on a buffer of pixel data.
    /// 
    /// Conceptually, implementing classes should:
    /// 
    /// 1. Specify a bit depth for pixels
    /// 2. Specify a color mode
    /// 3. Preserve the display state as a byte[] in the PixelBuffer
    /// 4. Optimize primitive drawing methods for the bit depth of pixels
    /// 5. Be abstracted/decoupled from a specific device driver
    /// </summary>
    public interface IPixelBuffer
    {
        /// <summary>
        /// The width of the pixel buffer
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the pixel buffer
        /// </summary>
        int Height { get; }

        /// <summary>
        /// The ColorMode of the pixel buffer
        /// </summary>
        ColorMode ColorMode { get; }

        /// <summary>
        /// The BitDepth of the chosen ColorMode
        /// </summary>
        int BitDepth { get; }

        /// <summary>
        /// The number of bytes in this pixel buffer
        /// </summary>
        int ByteCount { get; }

        /// <summary>
        /// The byte array that holds all pixel data
        /// </summary>
        byte[] Buffer { get; }

        /// <summary>
        /// Set the color of the pixel at the provided coordinates
        /// </summary>
        /// <param name="x">X coordinate of the pixel: 0,0 at top left</param>
        /// <param name="y">Y coordinate of the pixel: 0,0 at top left</param>
        /// <param name="color">The pixel color</param>
        void SetPixel(int x, int y, Color color);

        /// <summary>
        /// Get the color of a pixel - may be scaled based on buffer color depth
        /// </summary>
        /// <param name="x">X coordinate of the pixel: 0,0 at top left</param>
        /// <param name="y">Y coordinate of the pixel: 0,0 at top left</param>
        Color GetPixel(int x, int y);

        /// <summary>
        /// Invert the color of a pixel at the provided location
        /// </summary>
        /// <param name="x">The X coord to invert</param>
        /// <param name="y">The Y coord to invert</param>
        void InvertPixel(int x, int y);

        /// <summary>
        /// Writes another pixel buffer into this buffer.
        /// </summary>
        /// <param name="originX">The X origin to start writing</param>
        /// <param name="originY">The Y origin to start writing</param>
        /// <param name="buffer">The buffer to write into this buffer</param>
        /// <returns></returns>
        void WriteBuffer(int originX, int originY, IPixelBuffer buffer);

        /// <summary>
        /// Fills the buffer with the provided color
        /// </summary>
        /// <param name="color">The color to fill</param>
        void Fill(Color color);

        /// <summary>
        /// Fills part of the buffer with the provided color
        /// </summary>
        /// <param name="originX">The X coord to start filling</param>
        /// <param name="originY">The Y coord to start filling</param>
        /// <param name="width">The width to fill</param>
        /// <param name="height">The height to fill</param>
        /// <param name="color">The color to fill</param>
        void Fill(int originX, int originY, int width, int height, Color color);

        /// <summary>
        /// Clears the buffer (writes 0s to the byte array)
        /// </summary>
        void Clear();

        /// <summary>
        /// Clears a region of the buffer (writes 0s to the byte array)
        /// </summary>
        /// <param name="originX">The X coord to start</param>
        /// <param name="originY">The Y coord to start</param>
        /// <param name="width">The width of the region to clear</param>
        /// <param name="height">The height of the region to clear</param>
        void Clear(int originX, int originY, int width, int height)
        {
            Fill(originX, originY, width, height, Color.Black);
        }
    }
}