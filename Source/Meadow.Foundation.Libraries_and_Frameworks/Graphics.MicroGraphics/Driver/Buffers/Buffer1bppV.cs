using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 1bpp pixel buffer with vertical pixel packing
    /// 1 byte represents 8 pixels on the y-axis
    /// </summary>
    public class Buffer1bppV : Buffer1bpp
    {
        /// <summary>
        /// Creates a new Buffer1bpp object
        /// </summary>
        /// <param name="width">width of buffer in pixels</param>
        /// <param name="height">height of buffer in pixels</param>
        /// <param name="buffer">data to copy into buffer</param>
        public Buffer1bppV(int width, int height, byte[] buffer) :
            base(width, height, buffer)
        { }

        /// <summary>
        /// Creates a new Buffer1bpp object
        /// </summary>
        /// <param name="width">width of buffer in pixels</param>
        /// <param name="height">height of buffer in pixels</param>
        public Buffer1bppV(int width, int height) :
            base(width, height)
        { }

        /// <summary>
        /// Creates a new empty Buffer1bpp object
        /// </summary>
        public Buffer1bppV() : base()
        { }

        /// <summary>
        /// Creates a new Buffer1bpp object
        /// </summary>
        /// <param name="width">width of buffer in pixels</param>
        /// <param name="height">height of buffer in pixels</param>
        /// <param name="pageSize">the display page size, this will pad the total buffer size to multiples of the page size</param>
        public Buffer1bppV(int width, int height, int pageSize)
        {
            Width = width;
            Height = height;

            int bufferSize = width * height / 8;
            bufferSize += bufferSize % pageSize;

            Buffer = new byte[bufferSize];
        }

        /// <summary>
        /// Is the pixel enabled / on for a given location
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <returns>true if pixel is set / enabled</returns>
        public override bool GetPixelIsEnabled(int x, int y)
        {
            return (Buffer[(x + y * Width) / 8] & (0x80 >> (x % 8))) != 0;
        }

        /// <summary>
        /// Set a pixel in the display buffer
        /// </summary>
        /// <param name="x">x position in pixels from left</param>
        /// <param name="y">y position in pixels from top</param>
        /// <param name="enabled">is pixel enabled (on)</param>
        public override void SetPixel(int x, int y, bool enabled)
        {
            if (enabled)
            {   //0x80 = 128 = 0b_10000000
                Buffer[(x + y * Width) / 8] |= (byte)(0x80 >> (x % 8));
            }
            else
            {
                Buffer[(x + y * Width) / 8] &= (byte)~(0x80 >> (x % 8));
            }
        }

        public override void Fill(int x, int y, int width, int height, Color color)
        {
            if (x < 0 || x + width > Width ||
                y < 0 || y + height > Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            var isColored = color.Color1bpp;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {   
                    //ToDo - optimize for full byte copies
                    SetPixel(x + i, y + j, isColored);
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
            Buffer[(x + y * Width) / 8] ^= (byte)~(0x80 >> (x % 8));
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
                for (int i = 0; i < buffer.Width; i++)
                {
                    for (int j = 0; j < buffer.Height; j++)
                    {
                        //1 bit at a time 
                        SetPixel(x + i, y + j, (buffer as Buffer1bpp).GetPixelIsEnabled(i, j));
                    }
                }
            }
            else
            {
                base.WriteBuffer(x, y, buffer);
            }
        }
    }
}