using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    /// <summary>
    /// Represents a 1bpp pixel buffer
    /// </summary>
    public class Buffer1bpp : PixelBufferBase
    {
        /// <summary>
        /// Color mode of the buffer - 1 bit per pixel 
        /// </summary>
        public override ColorType ColorMode => ColorType.Format1bpp;

        /// <summary>
        /// Creates a new Buffer1bpp object
        /// </summary>
        /// <param name="width">width of buffer in pixels</param>
        /// <param name="height">height of buffer in pixels</param>
        /// <param name="buffer">data to copy into buffer</param>
        public Buffer1bpp(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public Buffer1bpp(int width, int height) : base(width, height) { }

        public Buffer1bpp(int width, int height, int pageSize)
        {
            Width = width;
            Height = height;

            int bufferSize = width * height / 8;
            bufferSize += bufferSize % pageSize;

            Buffer = new byte[bufferSize];
        }

        public bool GetPixelIsColored(int x, int y)
        {
            var index = (y >> 8) * Width + x;

            return (Buffer[index] & (1 << y % 8)) != 0;
        }

        public override Color GetPixel(int x, int y)
        {
            return GetPixelIsColored(x, y) ? Color.White : Color.Black;
        }

        public void SetPixel(int x, int y, bool colored)
        {
            var index = (y >> 3) * Width + x; //divide by 8

            if (colored)
            {
                Buffer[index] = (byte)(Buffer[index] | (byte)(1 << (y % 8)));
            }
            else
            {
                Buffer[index] = (byte)(Buffer[index] & ~(byte)(1 << (y % 8)));
            }
        }

        public override void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color1bpp);
        }

        public override void Fill(Color color)
        {
            Clear(color.Color1bpp);
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
                {   //byte aligned and at least 8 rows to go
                    if((j + y) % 8 == 0 && j + y + 8 <= height)
                    {
                        //set an entire byte - fast
                        Buffer[((j + y) >> 3) * Width + x + i] = (byte)(isColored ? 0xFF : 0);
                        j += 7; //the main loop will add 1 to make it 8
                    }
                    else
                    {
                        SetPixel(x + i, y + j, isColored);
                    }
                }
            }
        }

        public void Clear(bool isColored)
        {
            // split the color in to two byte values
            Buffer[0] = (byte)(isColored ? 0xFF : 0);

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
            throw new NotImplementedException();
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
                        //if we got really clever we could find other alignment points but this is a good start
                        if (y % 8 == 0 && j + 8 <= buffer.Height)
                        {
                            //copy an entire byte - fast
                            Buffer[((y + j) >> 3) * Width + x + i] = buffer.Buffer[(j >> 3) * buffer.Width + i];
                            j += 7; //the main loop will add 1 to make it 8
                        }
                        else
                        {   //else 1 bit at a time 
                            SetPixel(x + i, y + j, (buffer as Buffer1bpp).GetPixelIsColored(i, j));
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