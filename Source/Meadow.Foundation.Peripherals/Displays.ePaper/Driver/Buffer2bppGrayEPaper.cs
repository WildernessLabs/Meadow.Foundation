using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 2bpp greysacle buffer
    /// This is specifically built for 4 color grayscale eInk displays and wraps two 1bpp buffers
    /// </summary>
    public class Buffer2bppGreyEPaper : IPixelBuffer
    {
        enum PixelState : byte
        {
            Black = 0,
            DarkGray = 1,
            LightGray = 2,
            White = 3,
        }

        /// <summary>
        /// Width of buffer in pixels
        /// </summary>
        public int Width => lightBuffer.Width;

        /// <summary>
        /// Height of buffer in pixels
        /// </summary>
        public int Height => lightBuffer.Height;

        /// <inheritdoc/>
        public ColorMode ColorMode => ColorMode.Format2bpp;

        /// <summary>
        /// Bit depth of display as an integer
        /// </summary>
        public int BitDepth => 2;

        /// <summary>
        /// Number of bytes in buffer
        /// The totals the byte count from both internal 1bpp buffers
        /// </summary>
        public int ByteCount => (Width * Height * BitDepth) / 8;

        /// <summary>
        /// No direct access to a unified buffer
        /// Access BufferBlack and BufferColor instead
        /// </summary>
        public byte[] Buffer => throw new System.NotImplementedException();

        /// <summary>
        /// The buffer for light and dark grey pixels
        /// </summary>
        public byte[] LightBuffer => lightBuffer.Buffer;

        /// <summary>
        /// The buffer to darken pixels to light gray and black
        /// </summary>
        public byte[] DarkBuffer => darkBuffer.Buffer;

        readonly Buffer1bppV lightBuffer;
        readonly Buffer1bppV darkBuffer;

        /// <summary>
        /// Create a new Buffer2bppGreyEPaper object
        /// </summary>
        /// <param name="width">the buffer width in pixels</param>
        /// <param name="height">the buffer height in pixels</param>
        public Buffer2bppGreyEPaper(int width, int height)
        {
            lightBuffer = new Buffer1bppV(width, height);
            darkBuffer = new Buffer1bppV(width, height);

            lightBuffer.InitializeBuffer();
            darkBuffer.InitializeBuffer();
        }

        /// <summary>
        /// Clear the buffer
        /// </summary>
        public void Clear()
        {
            lightBuffer.Clear(false);
            darkBuffer.Clear(false);
        }

        /// <summary>
        /// Fill with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        public void Fill(Color color)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    SetPixel(x, y, color);
                }
            }
        }

        /// <summary>
        /// Fill with a color
        /// </summary>
        /// <param name="originX">X start position in pixels</param>
        /// <param name="originY">Y start position in pixels</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="color">The fill color</param>
        public void Fill(int originX, int originY, int width, int height, Color color)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SetPixel(x + originX, y + originY, color);
                }
            }
        }

        /// <summary>
        /// Get the pixel color
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color</returns>
        public Color GetPixel(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Invert the pixel
        /// Not currently supported
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public void InvertPixel(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Set a pixel to a color
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <param name="color">The color - will normalize to black, white or color</param>
        public void SetPixel(int x, int y, Color color)
        {
            var state = GetStateFromColor(color);

            switch (state)
            {
                case PixelState.Black:
                    lightBuffer.SetPixel(x, y, true);
                    darkBuffer.SetPixel(x, y, true);
                    break;
                case PixelState.DarkGray:
                    lightBuffer.SetPixel(x, y, false);
                    darkBuffer.SetPixel(x, y, true);
                    break;
                case PixelState.LightGray:
                    lightBuffer.SetPixel(x, y, true);
                    darkBuffer.SetPixel(x, y, false);
                    break;
                case PixelState.White:
                    lightBuffer.SetPixel(x, y, false);
                    darkBuffer.SetPixel(x, y, false);
                    break;
            }
        }

        /// <summary>
        /// Write a buffer to the buffer
        /// </summary>
        /// <param name="x">The x position in pixels to write the buffer</param>
        /// <param name="y">The y position in pixels to write the buffer</param>
        /// <param name="buffer">The buffer to write</param>
        public void WriteBuffer(int x, int y, IPixelBuffer buffer)
        {
            lightBuffer.WriteBuffer(x, y, buffer);
        }

        PixelState GetStateFromColor(Color color) => (PixelState)color.Color2bppGray;
    }
}