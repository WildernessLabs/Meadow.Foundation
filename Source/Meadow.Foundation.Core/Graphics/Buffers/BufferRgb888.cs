using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 24bpp color pixel buffer
    /// </summary>
    public class BufferRgb888 : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer
        /// </summary>
        public override ColorMode ColorMode => ColorMode.Format24bppRgb888;

        /// <summary>
        /// Create a new BufferRgb888 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <param name="buffer">The backing buffer</param>
        public BufferRgb888(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        /// <summary>
        /// Create a new BufferRgb888 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        public BufferRgb888(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new BufferRgb888 object
        /// </summary>
        public BufferRgb888() : base() { }

        /// <summary>
        /// Get the pixel color
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color as a 888 24bpp int</returns>
        public int GetPixelInt(int x, int y)
        {
            //get current color
            var index = ((y * Width) + x) * 3;

            return (Buffer[index] << 16 | Buffer[++index] << 8 | Buffer[++index]);
        }

        /// <summary>
        /// Get the pixel color
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color</returns>
        public override Color GetPixel(int x, int y)
        {
            var index = ((y * Width) + x) * 3;

            byte r = Buffer[index];
            byte g = Buffer[index + 1];
            byte b = Buffer[index + 2];

            return new Color(r, g, b);
        }

        /// <summary>
        /// Set the pixel color
        /// </summary>
        /// <param name="x">X pixel position</param>
        /// <param name="y">Y pixel position</param>
        /// <param name="color">The pixel color</param>
        public override void SetPixel(int x, int y, Color color)
        {
            var index = ((y * Width) + x) * 3;

            Buffer[index] = color.R;
            Buffer[index + 1] = color.G;
            Buffer[index + 2] = color.B;
        }

        /// <summary>
        /// Fill buffer with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        public override void Fill(Color color)
        {
            Buffer[0] = color.R;
            Buffer[1] = color.G;
            Buffer[2] = color.B;

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 3; copyLength < arrayMidPoint; copyLength <<= 1)
            {
                Buffer.AsSpan(0, copyLength).CopyTo(Buffer.AsSpan(copyLength, copyLength));
            }

            Buffer.AsSpan(0, Buffer.Length - copyLength).CopyTo(Buffer.AsSpan(copyLength, Buffer.Length - copyLength));
        }

        /// <summary>
        /// Fill with a color
        /// </summary>
        /// <param name="x">X start position in pixels</param>
        /// <param name="y">Y start position in pixels</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="color">The fill color</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if fill area is beyond the buffer bounds</exception>
        public override void Fill(int x, int y, int width, int height, Color color)
        {
            if (x < 0 || x + width > Width ||
                   y < 0 || y + height > Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] value = { color.R, color.G, color.B };
            int index = (y * Width + x) * 3 - 1;

            //fill the first line
            for (int i = 0; i < width; i++)
            {
                Buffer[++index] = value[0];
                Buffer[++index] = value[1];
                Buffer[++index] = value[2];
            }

            int rowBytes = width * 3;
            int firstRow = (y * Width + x) * 3;
            var src = Buffer.AsSpan(firstRow, rowBytes);
            for (int j = 1; j < height; j++)
            {
                src.CopyTo(Buffer.AsSpan((y + j) * Width * 3 + x * 3, rowBytes));
            }
        }

        /// <summary>
        /// Invert the pixel
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public override void InvertPixel(int x, int y)
        {
            var color = GetPixel(x, y);

            byte r = (byte)~color.R;
            byte g = (byte)~color.G;
            byte b = (byte)~color.B;

            SetPixel(x, y, new Color(r, g, b));
        }

        /// <summary>
        /// Write a buffer to specific location to the current buffer
        /// </summary>
        /// <param name="x">x origin</param>
        /// <param name="y">y origin</param>
        /// <param name="buffer">buffer to write</param>
        public override void WriteBuffer(int x, int y, IPixelBuffer buffer)
        {
            if (buffer.ColorMode == ColorMode)
            {
                int length = buffer.Width * 3;
                var source = buffer.Buffer;

                for (int i = 0; i < buffer.Height; i++)
                {
                    source.AsSpan(length * i, length).CopyTo(Buffer.AsSpan(Width * (y + i) * 3 + x * 3, length));
                }
            }
            else
            {   // fall back to a slow write
                base.WriteBuffer(x, y, buffer);
            }
        }
    }
}