using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 2bpp vertically packed pixel buffer (for ePaper displays)
    /// </summary>
    public class BufferGray2V : BufferGray2
    {
        /// <summary>
        /// Creates a new BufferGray2V object
        /// </summary>
        /// <param name="width">width of buffer in pixels</param>
        /// <param name="height">height of buffer in pixels</param>
        /// <param name="buffer">data to copy into buffer</param>
        public BufferGray2V(int width, int height, byte[] buffer) :
            base(width, height, buffer)
        { }

        /// <summary>
        /// Create a new BufferGray2V object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        public BufferGray2V(int width, int height) :
            base(width, height)
        { }

        /// <summary>
        /// Creates a new empty Buffer1bpp object
        /// </summary>
        public BufferGray2V() : base()
        { }

        /// <summary>
        /// Sets a pixel in the vertically packed buffer
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel</param>
        /// <param name="y">The y-coordinate of the pixel</param>
        /// <param name="gray">The 2-bit grayscale value (0-3)</param>
        public override void SetPixel(int x, int y, byte gray)
        {
            int index = (x + y * Width) >> 2; // Divide by 4 (4 pixels per byte)
            int shift = (x % 4) * 2;    // Each pixel has 2 bits, so (x % 4) * 2 gives bit position

            byte mask = (byte)(0b11 << shift); // Create a mask to clear existing pixel bits
            Buffer[index] = (byte)((Buffer[index] & ~mask) | ((gray & 0b11) << shift)); // Clear and set new color
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
            if (x < 0 || y < 0 || x + width > Width ||
                y + height > Height)
            {
                throw new ArgumentOutOfRangeException(nameof(Fill), "Fill area is out of bounds");
            }

            byte grayValue = (byte)(color.Color2bppGray & 0x03); // Extract 2-bit grayscale value

            for (int i = x; i < x + width; i++)
            {
                for (int j = y; j < y + height; j++)
                {
                    SetPixel(i, j, grayValue);
                }
            }
        }

        /// <summary>
        /// Invert a pixel
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public override void InvertPixel(int x, int y)
        {
            int index = (x + y * Width) >> 2;  // Each byte holds 4 pixels, so divide by 4
            int shift = (x % 4) * 2;           // Each pixel uses 2 bits, so multiply remainder by 2

            byte mask = (byte)(0b11 << shift); // Create a mask for the 2-bit pixel

            Buffer[index] ^= mask; // XOR to toggle the pixel's 2-bit value
        }

        /// <summary>
        /// Write a buffer to specific location to the current buffer
        /// </summary>
        /// <param name="x">x origin</param>
        /// <param name="y">y origin</param>
        /// <param name="buffer">buffer to write</param>
        public override void WriteBuffer(int x, int y, IPixelBuffer buffer)
        {
            base.WriteBuffer(x, y, buffer);
        }
    }
}