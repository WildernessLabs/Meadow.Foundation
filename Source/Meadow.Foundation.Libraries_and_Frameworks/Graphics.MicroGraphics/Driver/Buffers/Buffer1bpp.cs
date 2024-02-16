using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 1bpp pixel buffer with horizontal pixel packing
    /// 1 byte represents 8 pixels on the x-axis
    /// </summary>
    public class Buffer1bpp : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer - 1 bit per pixel 
        /// </summary>
        public override ColorMode ColorMode => ColorMode.Format1bpp;

        /// <summary>
        /// Creates a new Buffer1bpp object
        /// </summary>
        /// <param name="width">width of buffer in pixels</param>
        /// <param name="height">height of buffer in pixels</param>
        /// <param name="buffer">data to copy into buffer</param>
        public Buffer1bpp(int width, int height, byte[] buffer) :
            base(width, height, buffer)
        { }

        /// <summary>
        /// Creates a new Buffer1bpp object
        /// </summary>
        /// <param name="width">width of buffer in pixels</param>
        /// <param name="height">height of buffer in pixels</param>
        public Buffer1bpp(int width, int height) :
            base(width, height)
        { }

        /// <summary>
        /// Creates a new empty Buffer1bpp object
        /// </summary>
        public Buffer1bpp() : base()
        { }

        /// <summary>
        /// Creates a new Buffer1bpp object
        /// </summary>
        /// <param name="width">width of buffer in pixels</param>
        /// <param name="height">height of buffer in pixels</param>
        /// <param name="pageSize">the display page size, this will pad the total buffer size to multiples of the page size</param>
        public Buffer1bpp(int width, int height, int pageSize)
        {
            Width = width;
            Height = height;

            int bufferSize = (height * width >> 3);

            bufferSize += bufferSize % pageSize;

            Buffer = new byte[bufferSize];
        }

        /// <summary>
        /// Is the pixel on / enabled for a given location
        /// </summary>
        /// <param name="x">x location in pixels</param>
        /// <param name="y">y location in pixels</param>
        /// <returns>true if pixel is set / enabled</returns>
        public virtual bool GetPixelIsEnabled(int x, int y)
        {
            var index = (y >> 3) * Width + x;

            return (Buffer[index] & (1 << y % 8)) != 0;
        }

        /// <summary>
        /// Get the pixel color 
        /// </summary>
        /// <param name="x">x location of pixel</param>
        /// <param name="y">y location of pixel</param>
        /// <returns>The pixel color as a Color object - will be black or white only</returns>
        public override Color GetPixel(int x, int y)
        {
            return GetPixelIsEnabled(x, y) ? Color.White : Color.Black;
        }

        /// <summary>
        /// Set a pixel in the display buffer
        /// </summary>
        /// <param name="x">x position in pixels from left</param>
        /// <param name="y">y position in pixels from top</param>
        /// <param name="enabled">is pixel enabled (on)</param>
        public virtual void SetPixel(int x, int y, bool enabled)
        {
            var index = (y >> 3) * Width + x;

            var bitMask = (byte)(1 << (y % 8));

            Buffer[index] = enabled ? (byte)(Buffer[index] | bitMask) : (byte)(Buffer[index] & ~bitMask);
        }

        /// <summary>
        /// Set a pixel in the display buffer
        /// </summary>
        /// <param name="x">x position in pixels from left</param>
        /// <param name="y">y position in pixels from top</param>
        /// <param name="color">the color of the pixel - will snap to black or white (on/off)</param>
        public override void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color1bpp);
        }

        /// <summary>
        /// Fill the buffer with a color
        /// </summary>
        /// <param name="color">the fill color - will snap to black or white (on/off)</param>
        public override void Fill(Color color)
        {
            Clear(color.Color1bpp);
        }

        /// <summary>
        /// Fill the buffer with a color
        /// </summary>
        /// <param name="x">The x position in pixels</param>
        /// <param name="y">The y position in pixels</param>
        /// <param name="width">Width to fill in pixels</param>
        /// <param name="height">Height to fill in pixels</param>
        /// <param name="color">The color to fill</param>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if the fill region is outside of the buffer</exception>
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
                {   //byte aligned and at least 8 rows to go
                    if ((j + y) % 8 == 0 && j + y + 8 <= height)
                    {
                        //set an entire byte - fast
                        Buffer[((j + y) >> 3) * Width + x + i] = (byte)((isColored) ? 0xFF : 0);
                        j += 7; //the main loop will add 1 to make it 8
                    }
                    else
                    {
                        SetPixel(x + i, y + j, isColored);
                    }
                }
            }
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
        /// Invert a pixel
        /// </summary>
        /// <param name="x">x position of pixel</param>
        /// <param name="y">y position of pixel</param>
        public override void InvertPixel(int x, int y)
        {
            var index = (y >> 3) * Width + x;

            Buffer[index] = Buffer[index] ^= (byte)(1 << y % 8);
        }

        /// <summary>
        /// Write a buffer to specific location to the current buffer
        /// </summary>
        /// <param name="x">x origin</param>
        /// <param name="y">y origin</param>
        /// <param name="buffer">buffer to write</param>
        public override void WriteBuffer(int x, int y, IPixelBuffer buffer)
        {
            if (buffer is Buffer1bpp buf1bpp)
            {
                for (int i = 0; i < buffer.Width; i++)
                {
                    for (int j = 0; j < buffer.Height; j++)
                    {
                        //if we got really clever we could find other alignment points but this is a good start
                        if (y % 8 == 0 && j + 8 <= buffer.Height)
                        {
                            //copy an entire byte - fast
                            Buffer[((y + j) >> 3) * Width + x + i] = buffer.Buffer[(j >> 3) * buffer.Width + i];
                            j += 7; //the main loop will add 1 to make it 8
                        }
                        else
                        {   //else 1 bit at a time 
                            SetPixel(x + i, y + j, buf1bpp.GetPixelIsEnabled(i, j));
                        }
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