using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace Graphics.MicroGraphics.Buffers
{
    /// <summary>
    /// Represents a 2bpp buffer
    /// This is specifically built for 3 color eInk displays and wraps two 1bpp buffers
    /// </summary>
    public class Buffer2bpp : IPixelBuffer
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
        public int Width { get; protected set; }

        /// <summary>
        /// Height of buffer in pixels
        /// </summary>
        public int Height { get; protected set; }

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

        public byte[] BlackBuffer => blackBuffer.Buffer;

        public byte[] ColorBuffer => colorBuffer.Buffer;

        Buffer1bppV blackBuffer;
        Buffer1bppV colorBuffer;

        public void Clear()
        {
            blackBuffer.Clear();
            colorBuffer.Clear();
        }

        public void Fill(Color color)
        {
            throw new System.NotImplementedException();
        }

        public void Fill(int originX, int originY, int width, int height, Color color)
        {
            throw new System.NotImplementedException();
        }

        public Color GetPixel(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public void InvertPixel(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public void SetPixel(int x, int y, Color color)
        {
            throw new System.NotImplementedException();
        }

        public void WriteBuffer(int originX, int originY, IPixelBuffer buffer)
        {
            throw new System.NotImplementedException();
        }

        PixelState GetStateFromColor(Color color)
        {
            if (color == Color.Black)
                return PixelState.Off;
            if (color == Color.White)
                return PixelState.On;
            return PixelState.ColorOn;
        }
    }
}
