using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 2bpp buffer
    /// This is specifically built for 3 color eInk displays and wraps two 1bpp buffers
    /// </summary>
    public class Buffer2bppEPaper : IPixelBuffer
    {
        enum PixelState
        {
            On,
            Off,
            ColorOn,
        }

        /// <summary>
        /// Width of buffer in pixels
        /// </summary>
        public int Width => blackBuffer.Width;

        /// <summary>
        /// Height of buffer in pixels
        /// </summary>
        public int Height => blackBuffer.Height;

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
        public int ByteCount => (Width * Height * BitDepth) / 4;

        /// <summary>
        /// No direct access to a unified buffer
        /// Access BufferBlack and BufferColor instead
        /// </summary>
        public byte[] Buffer => throw new System.NotImplementedException();

        /// <summary>
        /// The buffer for black pixels
        /// </summary>
        public byte[] BlackBuffer => blackBuffer.Buffer;

        /// <summary>
        /// The buffer for color pixels
        /// </summary>
        public byte[] ColorBuffer => colorBuffer.Buffer;

        readonly Buffer1bppV blackBuffer;
        readonly Buffer1bppV colorBuffer;

        /// <summary>
        /// Create a new Buffer2bppEPaper object
        /// </summary>
        /// <param name="width">the buffer width in pixels</param>
        /// <param name="height">the buffer height in pixels</param>
        public Buffer2bppEPaper(int width, int height)
        {
            blackBuffer = new Buffer1bppV(width, height);
            colorBuffer = new Buffer1bppV(width, height);

            blackBuffer.InitializeBuffer();
            colorBuffer.InitializeBuffer();
        }

        /// <summary>
        /// Clear the buffer
        /// </summary>
        public void Clear()
        {
            blackBuffer.Clear(true);
            colorBuffer.Clear(true);
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

            var state = GetStateFromColor(color);

            /*
            if (state == PixelState.ColorOn)
            {
                colorBuffer.Fill(Color.White);
                blackBuffer.Fill(Color.Black);
            }
            else if (state == PixelState.On)
            {
                colorBuffer.SetPixel(x, y, false);
                blackBuffer.SetPixel(x, y, true);
            }
            else
            {
                colorBuffer.SetPixel(x, y, false);
                blackBuffer.SetPixel(x, y, false);
            }*/
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
        /// Set a color pixel on or off
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <param name="isOn">true for on, false for off</param>
        public void SetColorPixel(int x, int y, bool isOn)
        {
            if (isOn)
            {
                colorBuffer.SetPixel(x, y, false);
                blackBuffer.SetPixel(x, y, true);
            }
            else
            {
                colorBuffer.SetPixel(x, y, true);
                blackBuffer.SetPixel(x, y, true);
            }
        }

        /// <summary>
        /// Set a black pixel on or off
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <param name="isOn">true for on, false for off</param>
        public void SetBlackPixel(int x, int y, bool isOn)
        {
            if (isOn)
            {
                colorBuffer.SetPixel(x, y, true);
                blackBuffer.SetPixel(x, y, false);
            }
            else
            {
                colorBuffer.SetPixel(x, y, true);
                blackBuffer.SetPixel(x, y, true);
            }
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

            if (state == PixelState.ColorOn)
            {
                colorBuffer.SetPixel(x, y, false);
                blackBuffer.SetPixel(x, y, true);
            }
            else if (state == PixelState.On)
            {
                colorBuffer.SetPixel(x, y, true);
                blackBuffer.SetPixel(x, y, false);
            }
            else
            {
                colorBuffer.SetPixel(x, y, true);
                blackBuffer.SetPixel(x, y, true);
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
            blackBuffer.WriteBuffer(x, y, buffer);
        }

        PixelState GetStateFromColor(Color color)
        {
            if (color == Color.Black)
                return PixelState.On;
            if (color == Color.White)
                return PixelState.Off;
            return PixelState.ColorOn;
        }
    }
}