using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    public class Buffer1bpp : BufferBase
    {
        public override int ByteCount => Width * Height / 8;

        public override ColorType displayColorMode => ColorType.Format1bpp;

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

        public override void Fill(Color color, int x, int y, int width, int height)
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

        public new void WriteBuffer(int x, int y, IDisplayBuffer buffer)
        {
            if (base.WriteBuffer(x, y, buffer))
            {   //call the base for validation
                //and to handle the slow path when buffers don't match
                return;
            }

            for (int i = 0; i < buffer.Width; i++)
            {
                for (int j = 0; j < buffer.Height; j++)
                {
                    //if we got really clever we could find other alignment points but this is a good start
                    if(y%8 == 0 && j + 8 <= buffer.Height)
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

        public Buffer1bpp Rotate(RotationType rotation)
        {
            Buffer1bpp newBuffer;

            switch(rotation)
            {
                case RotationType._90Degrees:
                    newBuffer = new Buffer1bpp(Height, Width);
                    for(int i = 0; i < Width; i++)
                    {
                        for(int j = 0; j < Height; j++)
                        {   
                            newBuffer.SetPixel(Height - j - 1, i, GetPixel(i, j));
                        }
                    }
                    break;
                case RotationType._270Degrees:
                    newBuffer = new Buffer1bpp(Height, Width);
                    for (int i = 0; i < Width; i++)
                    {
                        for (int j = 0; j < Height; j++)
                        {   
                            newBuffer.SetPixel(j, Width - i - 1, GetPixel(i, j));
                        }
                    }
                    break;
                case RotationType._180Degrees:
                    newBuffer = new Buffer1bpp(Width, Height);
                    for (int i = 0; i < Width; i++)
                    {
                        for (int j = 0; j < Height; j++)
                        {   
                            newBuffer.SetPixel(Width - i - 1, Height - j - 1, GetPixel(i, j));
                        }
                    }
                    break;
                case RotationType.Default:
                default:
                    newBuffer = new Buffer1bpp(Width, Height);
                    Array.Copy(Buffer, newBuffer.Buffer, ByteCount);
                    break;

            }

            return newBuffer;






            if(rotation == RotationType.Default ||
               rotation == RotationType._180Degrees)
            {
                
            }
            else //90 & 270
            {
                newBuffer = new Buffer1bpp(Height, Width);
            }

        }
    }
}