using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 2bpp pixel buffer
    /// </summary>
    public class BufferGray2 : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer
        /// </summary>
        public override ColorMode ColorMode => ColorMode.Format2bppGray;

        /// <summary>
        /// Create a new BufferGray2 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <param name="buffer">The backing buffer</param>
        public BufferGray2(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        /// <summary>
        /// Create a new BufferGray2 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        public BufferGray2(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new BufferGray2 object
        /// </summary>
        public BufferGray2() : base() { }

        /// <summary>
        /// Fill buffer with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        public override void Fill(Color color)
        {
            byte fillNibble = (byte)(color.Color2bppGray & 0b11); // Ensure only 2 bits are used
            Buffer[0] = (byte)(fillNibble | (fillNibble << 2) | (fillNibble << 4) | (fillNibble << 6));

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 1; copyLength < arrayMidPoint; copyLength <<= 1)
            {
                Array.Copy(Buffer, 0, Buffer, copyLength, copyLength);
            }

            Array.Copy(Buffer, 0, Buffer, copyLength, Buffer.Length - copyLength);
        }

        /// <summary>
        /// Fill a region with a color
        /// </summary>
        /// <param name="x">X start position in pixels</param>
        /// <param name="y">Y start position in pixels</param>
        /// <param name="width">Width in pixels</param>
        /// <param name="height">Height in pixels</param>
        /// <param name="color">The fill color</param>
        public override void Fill(int x, int y, int width, int height, Color color)
        {
            if (x < 0 || x + width > Width ||
                y < 0 || y + height > Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte gray = (byte)(color.Color2bppGray & 0b11);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    SetPixel(x + i, y + j, gray);
                }
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
            byte gray = GetPixel2bpp(x, y);
            return new Color(gray << 6, gray << 6, gray << 6);
        }

        /// <summary>
        /// Set the pixel color
        /// </summary>
        /// <param name="x">X pixel position</param>
        /// <param name="y">Y pixel position</param>
        /// <param name="color">The pixel color</param>
        public override void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color2bppGray);
        }

        /// <summary>
        /// Set the pixel to a shade of gray
        /// </summary>
        /// <param name="x">X pixel position</param>
        /// <param name="y">Y pixel position</param>
        /// <param name="gray">The 2 bit pixel gray value</param>
        public virtual void SetPixel(int x, int y, byte gray)
        {
            int index = (y * Width + x) / 4; // 4 pixels per byte
            int shift = (3 - (x % 4)) * 2; // Shift based on position in byte

            Buffer[index] = (byte)((Buffer[index] & ~(0b11 << shift)) | (gray << shift));
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        /// <param name="enabled">should the display pixels be enabled / on or clear / off</param>
        public void Clear(bool enabled)
        {
            // split the color in to two byte values
            Buffer[0] = (byte)(enabled ? 0xFF : 0);

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 1; copyLength < arrayMidPoint; copyLength <<= 1)
            {
                Array.Copy(Buffer, 0, Buffer, copyLength, copyLength);
            }

            Array.Copy(Buffer, 0, Buffer, copyLength, Buffer.Length - copyLength);
        }

        /// <summary>
        /// Invert the pixel
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public override void InvertPixel(int x, int y)
        {
            byte color = GetPixel2bpp(x, y);
            color = (byte)((~color) & 0b11); // Invert only 2 bits
            SetPixel(x, y, color);
        }

        /// <summary>
        /// Write a buffer to a specific location in the current buffer
        /// </summary>
        /// <param name="x">x origin</param>
        /// <param name="y">y origin</param>
        /// <param name="buffer">buffer to write</param>
        public override void WriteBuffer(int x, int y, IPixelBuffer buffer)
        {
            if (buffer.ColorMode == ColorMode &&
                x % 4 == 0 &&
                buffer.Width % 4 == 0)
            {
                // Optimized fast path
                int sourceIndex, destinationIndex;
                int length = buffer.Width / 4;

                for (int i = 0; i < buffer.Height; i++)
                {
                    sourceIndex = length * i;
                    destinationIndex = (Width * (y + i) + x) / 4;

                    Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length);
                }
            }
            else
            {   // Fall back to slow write
                base.WriteBuffer(x, y, buffer);
            }
        }

        /// <summary>
        /// Get a pixel's 2bpp grayscale value
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color as a 4bpp gray value</returns>
        public byte GetPixel2bpp(int x, int y)
        {
            int index = (y * Width + x) / 4;
            int shift = (3 - (x % 4)) * 2; // Extract 2-bit value
            return (byte)((Buffer[index] >> shift) & 0b11);
        }
    }
}
