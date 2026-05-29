using Meadow.Peripherals.Displays;
using System;
using System.Runtime.CompilerServices;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 12bpp color pixel buffer
    /// </summary>
    public class BufferRgb444 : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer
        /// </summary>
        public override ColorMode ColorMode => ColorMode.Format12bppRgb444;

        /// <summary>
        /// Create a new BufferRgb444 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <param name="buffer">The backing buffer</param>
        public BufferRgb444(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        /// <summary>
        /// Create a new BufferRgb444 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        public BufferRgb444(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new BufferRgb444 object
        /// </summary>
        public BufferRgb444() : base() { }

        /// <summary>
        /// Get the pixel color
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color as a 12bpp value</returns>
        public ushort GetPixel12bpp(int x, int y)
        {
            byte r, g, b;
            int index;
            if (x % 2 == 0)
            {
                index = (x + y * Width) * 3 / 2;

                r = (byte)(Buffer[index] >> 4);
                g = (byte)(Buffer[index] & 0x0F);
                b = (byte)(Buffer[index + 1] >> 4);
            }
            else
            {
                index = ((x - 1 + y * Width) * 3 / 2) + 1;
                r = (byte)(Buffer[index] & 0x0F);
                g = (byte)(Buffer[index + 1] >> 4);
                b = (byte)(Buffer[index + 1] & 0x0F);
            }

            return (ushort)(r << 8 | g << 4 | b);
        }

        /// <summary>
        /// Get the pixel color
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color</returns>
        public override Color GetPixel(int x, int y)
        {
            byte r, g, b;
            int index;
            if (x % 2 == 0)
            {
                index = (x + y * Width) * 3 / 2;

                r = (byte)(Buffer[index] >> 4);
                g = (byte)(Buffer[index] & 0x0F);
                b = (byte)(Buffer[index + 1] >> 4);
            }
            else
            {
                index = ((x - 1 + y * Width) * 3 / 2) + 1;
                r = (byte)(Buffer[index] & 0x0F);
                g = (byte)(Buffer[index + 1] >> 4);
                b = (byte)(Buffer[index + 1] & 0x0F);
            }

            r = (byte)(r * 255 / 15);
            g = (byte)(g * 255 / 15);
            b = (byte)(b * 255 / 15);

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
            SetPixel(x, y, color.Color12bppRgb444);
        }

        /// <summary>
        /// Set the pixel color
        /// </summary>
        /// <param name="x">X pixel position</param>
        /// <param name="y">Y pixel position</param>
        /// <param name="color">The pixel color packed as a 12 bpp ushort</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, ushort color)
        {
            int index;
            //one of 2 possible write patterns 
            if (x % 2 == 0)
            {
                //1st byte RRRRGGGG
                //2nd byte BBBB
                index = ((x + y * Width) * 3 / 2);
                Buffer[index] = (byte)(color >> 4); //think this is correct - grab the r & g values
                index++;
                Buffer[index] = (byte)((Buffer[index] & 0x0F) | (color << 4));
            }
            else
            {
                //1st byte     RRRR
                //2nd byte GGGGBBBB
                index = ((x - 1 + y * Width) * 3 / 2) + 1;
                Buffer[index] = (byte)((Buffer[index] & 0xF0) | (color >> 8));
                Buffer[++index] = (byte)color; //just the lower 8 bits
            }
        }

        /// <summary>
        /// Fill buffer with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        public override void Fill(Color color)
        {
            var color12bpp = color.Color12bppRgb444;
            Buffer[0] = (byte)(color12bpp >> 4);
            Buffer[1] = (byte)(((color12bpp & 0x0F) << 4) | (color12bpp >> 8));
            Buffer[2] = (byte)color12bpp;

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

            if (width <= 0 || height <= 0)
            {
                return;
            }

            ushort uColor = color.Color12bppRgb444;

            bool hasLead = (x & 1) == 1;
            int alignedX = hasLead ? x + 1 : x;
            int alignedWidth = hasLead ? width - 1 : width;
            if (alignedWidth < 0) alignedWidth = 0;

            int pairs = alignedWidth / 2;
            bool hasTrail = (alignedWidth & 1) == 1;
            int trailX = alignedX + pairs * 2;

            int rowStrideBytes = Width * 3 / 2;
            int interiorBytes = pairs * 3;
            int interiorStart = (alignedX + y * Width) * 3 / 2;

            if (pairs > 0)
            {
                Buffer[interiorStart] = (byte)(uColor >> 4);
                Buffer[interiorStart + 1] = (byte)(((uColor & 0x0F) << 4) | (uColor >> 8));
                Buffer[interiorStart + 2] = (byte)uColor;

                int filled = 3;
                while (filled < interiorBytes)
                {
                    int copy = filled < interiorBytes - filled ? filled : interiorBytes - filled;
                    Buffer.AsSpan(interiorStart, copy).CopyTo(Buffer.AsSpan(interiorStart + filled, copy));
                    filled += copy;
                }
            }

            if (hasLead) SetPixel(x, y, uColor);
            if (hasTrail) SetPixel(trailX, y, uColor);

            var interior = Buffer.AsSpan(interiorStart, interiorBytes);
            for (int row = 1; row < height; row++)
            {
                if (pairs > 0)
                {
                    interior.CopyTo(Buffer.AsSpan(interiorStart + row * rowStrideBytes, interiorBytes));
                }
                if (hasLead) SetPixel(x, y + row, uColor);
                if (hasTrail) SetPixel(trailX, y + row, uColor);
            }
        }

        /// <summary>
        /// Invert the pixel
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public override void InvertPixel(int x, int y)
        {
            byte r, g, b;
            int index;
            if (x % 2 == 0)
            {
                index = (x + y * Width) * 3 / 2;

                r = (byte)(Buffer[index] >> 4);
                g = (byte)(Buffer[index] & 0x0F);
                b = (byte)(Buffer[index + 1] >> 4);
            }
            else
            {
                index = ((x - 1 + y * Width) * 3 / 2) + 1;
                r = (byte)(Buffer[index] & 0x0F);
                g = (byte)(Buffer[index + 1] >> 4);
                b = (byte)(Buffer[index + 1] & 0x0F);
            }

            r = (byte)(~r & 0x0F);
            g = (byte)(~g & 0x0F);
            b = (byte)(~b & 0x0F);

            //get new color
            var color = (ushort)(r << 8 | g << 4 | b);

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
            if (buffer.ColorMode == ColorMode &&
                Width % 2 == 0 &&
                x % 2 == 0 &&
                buffer.Width % 2 == 0)
            {
                //we have a happy path
                int length = buffer.Width / 2 * 3;
                var source = buffer.Buffer;

                for (int i = 0; i < buffer.Height; i++)
                {
                    source.AsSpan(length * i, length).CopyTo(Buffer.AsSpan((Width * (y + i) + x) * 3 / 2, length));
                }
            }
            else
            {   // fall back to a slow write
                base.WriteBuffer(x, y, buffer);
            }
        }
    }
}