using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents an 18bpp color pixel buffer
    /// </summary>
    public class BufferRgb666 : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer
        /// </summary>
        public override ColorMode ColorMode => ColorMode.Format18bppRgb666;

        /// <summary>
        /// Number of bytes in buffer
        /// </summary>
        public override int ByteCount => Width * Height * 3;

        /// <summary>
        /// Create a new BufferRgb666 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <param name="buffer">The backing buffer</param>
        public BufferRgb666(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        /// <summary>
        /// Create a new BufferRgb666 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        public BufferRgb666(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new BufferRgb666 object
        /// </summary>
        public BufferRgb666() : base() { }

        /// <summary>
        /// Get the pixel color
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color</returns>
        public override Color GetPixel(int x, int y)
        {
            int index = ((y * Width) + x) * 3;
            byte r = (byte)(Buffer[index] & 0xFC);    // RRRRRR00
            byte g = (byte)(Buffer[index + 1] & 0xFC);// GGGGGG00
            byte b = (byte)(Buffer[index + 2] & 0xFC);// BBBBBB00

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
            int index = ((y * Width) + x) * 3;
            Buffer[index] = (byte)(color.R & 0xFC);    // RRRRRR00
            Buffer[index + 1] = (byte)(color.G & 0xFC);// GGGGGG00
            Buffer[index + 2] = (byte)(color.B & 0xFC);// BBBBBB00
        }

        /// <summary>
        /// Fill buffer with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        public override void Fill(Color color)
        {
            Console.WriteLine("Fill");

            byte[] value = { (byte)(color.R & 0xFC), (byte)(color.G & 0xFC), (byte)(color.B & 0xFC) };
            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            // Fill the initial part of the buffer
            for (int i = 0; i < 3; i++)
            {
                Buffer[i] = value[i];
            }

            // Use Array.Copy to fill the buffer in larger chunks
            for (copyLength = 3; copyLength < arrayMidPoint; copyLength <<= 1)
            {
                Array.Copy(Buffer, 0, Buffer, copyLength, copyLength);
            }

            // Copy whatever is remaining
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
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if fill area is beyond the buffer bounds</exception>
        public override void Fill(int x, int y, int width, int height, Color color)
        {
            if (x < 0 || x + width > Width ||
                y < 0 || y + height > Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            byte[] value = { (byte)(color.R & 0xFC), (byte)(color.G & 0xFC), (byte)(color.B & 0xFC) };
            int index = (y * Width + x) * 3 - 1;

            // Fill the first line
            for (int i = 0; i < width; i++)
            {
                Buffer[++index] = value[0];
                Buffer[++index] = value[1];
                Buffer[++index] = value[2];
            }

            //array copy the rest
            for (int j = 0; j < height - 1; j++)
            {
                Array.Copy(Buffer,
                    (y + j) * Width * 3 + x * 3,
                    Buffer,
                    (y + j + 1) * Width * 3 + x * 3,
                    width * 3);
            }
        }

        /// <summary>
        /// Invert the pixel
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public override void InvertPixel(int x, int y)
        {
            int index = ((y * Width) + x) * 3;
            Buffer[index] = (byte)(~Buffer[index] & 0xFC);    // Invert R
            Buffer[index + 1] = (byte)(~Buffer[index + 1] & 0xFC);// Invert G
            Buffer[index + 2] = (byte)(~Buffer[index + 2] & 0xFC);// Invert B
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
                int length = buffer.Width * 3;

                for (int i = 0; i < buffer.Height; i++)
                {
                    sourceIndex = length * i;
                    destinationIndex = (Width * (y + i) + x) * 3;

                    Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length);
                }
            }
            else
            {
                // Fall back to a slow write
                base.WriteBuffer(x, y, buffer);
            }
        }
    }
}