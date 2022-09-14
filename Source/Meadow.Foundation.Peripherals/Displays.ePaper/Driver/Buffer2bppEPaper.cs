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

        /// <summary>
        /// Color mode of the buffer - 2 bit per pixel 
        /// </summary>
        public ColorType ColorMode => ColorType.Format2bpp;

        /// <summary>
        /// Bitdepth of display as an integer
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

        public void Fill(int originX, int originY, int width, int height, Color color)
        {
            for(int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SetPixel(x + originX, y + originY, color);
                }
            }
        }

        public Color GetPixel(int x, int y)
        {
            throw new System.NotImplementedException();
        }

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

            if(state == PixelState.ColorOn)
            {
                colorBuffer.SetPixel(x, y, false);
                blackBuffer.SetPixel(x, y, true);
            }
            else if(state == PixelState.On)
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

        public void WriteBuffer(int originX, int originY, IPixelBuffer buffer)
        {
            blackBuffer.WriteBuffer(originX, originY, buffer);
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