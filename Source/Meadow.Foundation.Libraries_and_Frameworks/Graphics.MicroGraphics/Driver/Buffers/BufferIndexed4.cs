using Meadow.Peripherals.Displays;
using System;
using System.Linq;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 4bpp pixel buffer with indexed colors
    /// Each pixel is represented by 4 bits, allowing for 8 distinct colors
    /// </summary>
    public class BufferIndexed4 : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer
        /// </summary>
        public override ColorMode ColorMode => ColorMode.Format4bppIndexed;

        /// <summary>
        /// The indexed colors as an 8 element array of Color values
        /// </summary>
        public readonly Color[] IndexedColors = new Color[8];

        /// <summary>
        /// Create a new BufferIndexed4 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <param name="buffer">The backing buffer</param>
        public BufferIndexed4(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        /// <summary>
        /// Create a new BufferIndexed4 object
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        public BufferIndexed4(int width, int height) : base(width, height) { }

        /// <summary>
        /// Create a new BufferIndexed4 object
        /// </summary>
        public BufferIndexed4() : base() { }

        /// <summary>
        /// Fill buffer with a color
        /// </summary>
        /// <param name="color">The fill color</param>
        public override void Fill(Color color)
        {
            byte colorValue = (byte)GetIndexForColor(color);
            Buffer[0] = (byte)(colorValue | colorValue << 4);

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
        /// <param name="colorIndex">The color index (0-7)</param>
        public void SetPixel(int x, int y, int colorIndex)
        {
            int index = y * Width / 2 + x / 2;

            if ((x % 2) == 0)
            {   //even pixel - shift to the significant nibble
                Buffer[index] = (byte)((Buffer[index] & 0x0f) | (colorIndex << 4));
            }
            else
            {   //odd pixel
                Buffer[index] = (byte)((Buffer[index] & 0xf0) | (colorIndex));
            }
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
                x % 2 == 0 &&
                buffer.Width % 2 == 0)
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
        /// Get the pixel's color index (0-7) at a given coordinate
        /// </summary>
        /// <param name="x">The X pixel position</param>
        /// <param name="y">The Y pixel position</param>
        /// <returns>The 4-bit color index</returns>
        public byte GetColorIndexForPixel(int x, int y)
        {
            int index = y * Width / 2 + x / 2;
            byte color;

            if ((x % 2) == 0)
            {   //even pixel - shift to the significant nibble
                color = (byte)((Buffer[index] & 0x0f) >> 4);
            }
            else
            {   //odd pixel
                color = (byte)((Buffer[index] & 0xf0));
            }
            return color;
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