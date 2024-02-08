using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 16bpp color pixel buffer
    /// </summary>
    public class BufferRgb565 : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer
        /// </summary>
        public override ColorMode ColorMode => ColorMode.Format16bppRgb565;

        /// <summary>
        /// Create a new BufferRgb565 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <param name="buffer">The backing buffer</param>
        public BufferRgb565(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        /// <summary>
        /// Create a new BufferRgb565 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        public BufferRgb565(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new BufferRgb565 object
        /// </summary>
        public BufferRgb565() : base() { }

        /// <summary>
        /// Get the pixel color
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color as a 565 16bpp value</returns>
        public ushort GetPixel16bpp(int x, int y)
        {
            //get current color
            var index = ((y * Width) + x) * sizeof(ushort);

            return (ushort)(Buffer[index] << 8 | Buffer[++index]);
        }

        /// <summary>
        /// Get the pixel color
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color</returns>
        public override Color GetPixel(int x, int y)
        {
            ushort color = GetPixel16bpp(x, y);

            byte r = (byte)(((color >> 11) & 0x1F) * 255 / 31);
            byte g = (byte)(((color >> 5) & 0x3F) * 255 / 63);
            byte b = (byte)(((color) & 0x1F) * 255 / 31);

            return new Color(r, g, b);
        }

        /// <summary>
        /// Set the pixel color
        /// </summary>
        /// <param name="x">X pixel position</param>
        /// <param name="y">Y pixel position</param>
        /// <param name="color">The pixel color packed as a 565 16bpp ushort</param>
        public void SetPixel(int x, int y, ushort color)
        {
            var index = ((y * Width) + x) * 2;

            Buffer[index] = (byte)(color >> 8);
            Buffer[++index] = (byte)color;
        }

        /// <summary>
        /// Set the pixel color
        /// </summary>
        /// <param name="x">X pixel position</param>
        /// <param name="y">Y pixel position</param>
        /// <param name="color">The pixel color</param>
        public override void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color16bppRgb565);
        }

        /// <summary>
        /// Fill buffer with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        public override void Fill(Color color)
        {
            Clear(color.Color16bppRgb565);
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

            byte[] value = { (byte)(color.Color16bppRgb565 >> 8), (byte)color.Color16bppRgb565 };
            int index = (y * Width + x) * 2 - 1;

            //fill the first line
            for (int i = 0; i < width; i++)
            {
                Buffer[++index] = value[0];
                Buffer[++index] = value[1];
            }

            //array copy the rest
            for (int j = 0; j < height - 1; j++)
            {
                Array.Copy(Buffer,
                    (y + j) * Width * 2 + x * 2,
                    Buffer,
                    (y + j + 1) * Width * 2 + x * 2,
                    width * 2);
            }
        }

        /// <summary>
        /// Clear the buffer to a 565 16bpp color value
        /// </summary>
        /// <param name="color">The color as a ushort</param>
        public void Clear(ushort color)
        {
            // split the color in to two byte values
            Buffer[0] = (byte)(color >> 8);
            Buffer[1] = (byte)color;

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 2; copyLength < arrayMidPoint; copyLength <<= 1)
            {
                Array.Copy(Buffer, 0, Buffer, copyLength, copyLength);
            }
            //copy whatever is remaining
            Array.Copy(Buffer, 0, Buffer, copyLength, Buffer.Length - copyLength);
        }

        /// <summary>
        /// Invert the pixel
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public override void InvertPixel(int x, int y)
        {
            //get current color
            ushort color = GetPixel16bpp(x, y);

            //split into R,G,B & invert
            byte r = (byte)(0x1F - ((color >> 11) & 0x1F));
            byte g = (byte)(0x3F - ((color >> 5) & 0x3F));
            byte b = (byte)(0x1F - (color) & 0x1F);

            //get new color
            color = (ushort)(r << 11 | g << 5 | b);

            SetPixel(x, y, color);
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
                int length = buffer.Width * 2;

                for (int i = 0; i < buffer.Height; i++)
                {
                    sourceIndex = length * i;
                    destinationIndex = Width * (y + i) * 2 + x * 2;

                    Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length);
                }
            }
            else
            {   // fall back to a slow write
                base.WriteBuffer(x, y, buffer);
            }
        }
    }
}