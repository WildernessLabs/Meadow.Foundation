using Meadow.Peripherals.Displays;
using System;
using System.Linq;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 2bpp pixel buffer with indexed colors
    /// Each pixel is represented by 2 bits, allowing for 4 distinct colors
    /// </summary>
    public class BufferIndexed2 : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer
        /// </summary>
        public override ColorMode ColorMode => ColorMode.Format2bppIndexed;

        /// <summary>
        /// The indexed colors as a 4 element array of Color values
        /// </summary>
        public readonly Color[] IndexedColors = new Color[4];

        /// <summary>
        /// Create a new BufferIndexed2 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <param name="buffer">The backing buffer</param>
        public BufferIndexed2(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        /// <summary>
        /// Create a new BufferIndexed2 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        public BufferIndexed2(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new BufferIndexed2 object
        /// </summary>
        public BufferIndexed2() : base() { }

        /// <summary>
        /// Fill buffer with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        public override void Fill(Color color)
        {
            byte colorValue = (byte)GetIndexForColor(color);
            Buffer[0] = (byte)(colorValue << 2 | colorValue << 4 | colorValue << 6);

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 1; copyLength < arrayMidPoint; copyLength <<= 1)
            {
                Array.Copy(Buffer, 0, Buffer, copyLength, copyLength);
            }

            Array.Copy(Buffer, 0, Buffer, copyLength, Buffer.Length - copyLength);
        }

        /// <summary>
        /// Fill a rectangular area with a color
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

            //TODO optimize
            var index = GetIndexForColor(color);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    SetPixel(x + i, y + j, index);
                }
            }
        }

        /// <summary>
        /// Get the pixel color at a given coordinate
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The pixel color</returns>
        public override Color GetPixel(int x, int y)
        {
            var index = GetColorIndexForPixel(x, y);
            return IndexedColors[index];
        }

        /// <summary>
        /// Set the pixel color
        /// </summary>
        /// <param name="x">X pixel position</param>
        /// <param name="y">Y pixel position</param>
        /// <param name="color">The pixel color</param>
        public override void SetPixel(int x, int y, Color color)
        {
            var index = GetIndexForColor(color);
            SetPixel(x, y, index);
        }

        /// <summary>
        /// Set the pixel using a color index
        /// </summary>
        /// <param name="x">X pixel position</param>
        /// <param name="y">Y pixel position</param>
        /// <param name="colorIndex">The color index (0-3)</param>
        public void SetPixel(int x, int y, int colorIndex)
        {
            int byteIndex = (y * Width + x) >> 2; // divide by 4 to find the byte
            int pixelOffset = (x & 0x03) << 1;    // (x % 4)*2 bits offset

            // Clear current 2 bits
            Buffer[byteIndex] &= (byte)~(0x03 << pixelOffset);
            // Set new bits
            Buffer[byteIndex] |= (byte)((colorIndex & 0x03) << pixelOffset);
        }

        /// <summary>
        /// Invert the pixel color
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public override void InvertPixel(int x, int y)
        {
            throw new NotImplementedException("InvertPixel not supported for indexed buffers");
        }

        /// <summary>
        /// Write a buffer into this buffer at a specified location
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
                // We can do a direct block copy row by row
                int sourceIndex, destinationIndex;
                int length = buffer.Width / 2;

                for (int i = 0; i < buffer.Height; i++)
                {
                    sourceIndex = length * i;
                    destinationIndex = (Width * (y + i) + x) >> 2;

                    Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length);
                }
            }
            else
            {   // fall back to a slow write
                base.WriteBuffer(x, y, buffer);
            }
        }

        /// <summary>
        /// Get the pixel's color index (0-3) at a given coordinate
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The 2-bit color index</returns>
        public byte GetColorIndexForPixel(int x, int y)
        {
            int byteIndex = (y * Width + x) >> 2;
            int pixelOffset = (x & 0x03) << 1;

            byte value = (byte)((Buffer[byteIndex] >> pixelOffset) & 0x03);
            return value;
        }

        int GetIndexForColor(Color color)
        {
            if (IndexedColors == null || IndexedColors.All(x => x == null))
            {
                throw new NullReferenceException("No indexed colors assigned");
            }

            int closestIndex = -1;
            double shortestDistance = double.MaxValue;

            for (int i = 0; i < IndexedColors.Length; i++)
            {
                if (IndexedColors[i] != null)
                {
                    double distance = GetColorDistance(color, IndexedColors[i]);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        closestIndex = i;
                        if (distance == 0) { break; } // perfect match
                    }
                }
            }
            return closestIndex;
        }
    }
}
