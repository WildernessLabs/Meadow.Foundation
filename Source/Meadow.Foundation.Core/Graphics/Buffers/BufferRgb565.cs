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
        public unsafe ushort GetPixel16bpp(int x, int y)
        {
            fixed (byte* ptr = Buffer)
            {
                var pixelPtr = (ushort*)(ptr + ((y * Width + x) << 1));
                return *pixelPtr;
            }
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
        public unsafe void SetPixel(int x, int y, ushort color)
        {
            fixed (byte* ptr = Buffer)
            {
                var pixelPtr = (ushort*)(ptr + ((y * Width + x) << 1));
                *pixelPtr = (ushort)((color << 8) | (color >> 8));
            }
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

            ushort color565 = color.Color16bppRgb565;
            byte hi = (byte)(color565 >> 8);
            byte lo = (byte)color565;
            int rowStride = Width * 2;

            if (width <= 2)
            {
                for (int col = 0; col < width; col++)
                {
                    int index = (y * Width + x + col) * 2;
                    for (int row = 0; row < height; row++)
                    {
                        Buffer[index] = hi;
                        Buffer[index + 1] = lo;
                        index += rowStride;
                    }
                }
                return;
            }

            int firstRow = (y * Width + x) * 2;
            for (int i = 0; i < width; i++)
            {
                Buffer[firstRow + i * 2] = hi;
                Buffer[firstRow + i * 2 + 1] = lo;
            }

            int rowBytes = width * 2;
            var src = Buffer.AsSpan(firstRow, rowBytes);
            for (int j = 1; j < height; j++)
            {
                src.CopyTo(Buffer.AsSpan((y + j) * rowStride + x * 2, rowBytes));
            }
        }

        /// <summary>
        /// Clear the buffer to a 565 16bpp color value
        /// </summary>
        /// <param name="color">The color as a ushort</param>
        public void Clear(ushort color)
        {
            unsafe
            {
                fixed (byte* ptr = Buffer)
                {
                    var colorPtr = (ushort*)ptr;
                    var pixelCount = Buffer.Length >> 1;

                    for (int i = 0; i < pixelCount; i++)
                    {
                        colorPtr[i] = color;
                    }
                }
            }
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
            byte b = (byte)(0x1F - ((color) & 0x1F));

            //get new color
            color = (ushort)(r << 11 | g << 5 | b);

            SetPixel(x, y, color);
        }

        /// <summary>
        ///  fast and no checks
        /// </summary>
        public void WriteBufferRaw(BufferRgb565 bufferToDraw)
        {
            Array.Copy(bufferToDraw.Buffer, Buffer, bufferToDraw.Buffer.Length);
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
                int length = buffer.Width * 2;
                var source = buffer.Buffer;

                for (int i = 0; i < buffer.Height; i++)
                {
                    source.AsSpan(length * i, length).CopyTo(Buffer.AsSpan(Width * (y + i) * 2 + x * 2, length));
                }
            }
            else
            {   // fall back to a slow write
                base.WriteBuffer(x, y, buffer);
            }
        }
    }
}