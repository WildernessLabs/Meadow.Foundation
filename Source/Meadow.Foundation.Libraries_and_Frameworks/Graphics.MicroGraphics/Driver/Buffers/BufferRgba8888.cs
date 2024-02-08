using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 32bpp color pixel buffer
    /// </summary>
    public class BufferRgba8888 : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer
        /// </summary>
        public override ColorMode ColorMode => ColorMode.Format32bppRgba8888;

        /// <summary>
        /// Create a new BufferRgba8888 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <param name="buffer">The backing buffer</param>
        public BufferRgba8888(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        /// <summary>
        /// Create a new BufferRgba8888 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        public BufferRgba8888(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new BufferRgba8888 object
        /// </summary>
        public BufferRgba8888() : base() { }

        /// <summary>
        /// Get the pixel color
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color as an 8888 32bpp value</returns>
        public int GetPixelInt(int x, int y)
        {
            //get current color
            var index = ((y * Width) + x) * 4;

            return (Buffer[index] << 24 | Buffer[++index] << 16 | Buffer[++index] << 8 | Buffer[++index]);
        }
        /// <summary>
        /// Get the pixel color
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color</returns>
        public override Color GetPixel(int x, int y)
        {
            var index = ((y * Width) + x) * 4;

            byte r = Buffer[index];
            byte g = Buffer[index + 1];
            byte b = Buffer[index + 2];
            byte a = Buffer[index + 3];

            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Set the pixel color
        /// </summary>
        /// <param name="x">X pixel position</param>
        /// <param name="y">Y pixel position</param>
        /// <param name="color">The pixel color</param>
        public override void SetPixel(int x, int y, Color color)
        {
            var index = ((y * Width) + x) * 4;

            Buffer[index] = color.R;
            Buffer[index + 1] = color.G;
            Buffer[index + 2] = color.B;
            Buffer[index + 3] = color.A;
        }

        /// <summary>
        /// Fill with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        public override void Fill(Color color)
        {
            // split the color in to two byte values
            Buffer[0] = color.R;
            Buffer[1] = color.G;
            Buffer[2] = color.B;
            Buffer[3] = color.A;

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 4; copyLength < arrayMidPoint; copyLength <<= 1)
            {
                Array.Copy(Buffer, 0, Buffer, copyLength, copyLength);
            }

            Array.Copy(Buffer, 0, Buffer, copyLength, Buffer.Length - copyLength);
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

            byte[] value = { color.R, color.G, color.B, color.A };
            int index = (y * Width + x) * 4 - 1;

            //fill the first line
            for (int i = 0; i < width; i++)
            {
                Buffer[++index] = value[0];
                Buffer[++index] = value[1];
                Buffer[++index] = value[2];
                Buffer[++index] = value[3];
            }

            //array copy the rest
            for (int j = 0; j < height - 1; j++)
            {
                Array.Copy(Buffer,
                    (y + j) * Width * 4 + x * 4,
                    Buffer,
                    (y + j + 1) * Width * 4 + x * 4,
                    width * 4);
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

            //split into R,G,B & invert
            byte r = (byte)~color.R;
            byte g = (byte)~color.G;
            byte b = (byte)~color.B;

            SetPixel(x, y, new Color(r, g, b, color.A));
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

                int sourceIndex, destinationIndex;
                int length = buffer.Width * 4;

                for (int i = 0; i < buffer.Height; i++)
                {
                    sourceIndex = length * i;
                    destinationIndex = Width * (y + i) * 4 + x * 4;

                    Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length); ;
                }
            }
            else
            {   // fall back to a slow write
                base.WriteBuffer(x, y, buffer);
            }
        }
    }
}