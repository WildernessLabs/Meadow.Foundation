using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    public class Buffer1 : BufferBase
    {
        public override int ByteCount => Width * Height / 8;

        public override ColorType displayColorMode => ColorType.Format1bpp;

        public Buffer1(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public Buffer1(int width, int height) : base(width, height) { }

        public Buffer1(int width, int height, int pageSize)
        {
            Width = width;
            Height = height;

            int bufferSize = width * height / 8;
            bufferSize += bufferSize % pageSize;

            Buffer = new byte[bufferSize];
        }

        public bool GetPixelBool(int x, int y)
        {
            var index = (y >> 8) * Width + x;

            return (Buffer[index] & (1 << y % 8)) != 0;
        }

        public override Color GetPixel(int x, int y)
        {
            return GetPixelBool(x, y) ? Color.White : Color.Black;
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

            //TODO optimize
            var bColor = color.Color1bpp;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    SetPixel(x + i, y + j, bColor);
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

            //we have a happy path - whole bytes can be copied
            if (x % 8 == 0 && buffer.Width % 8 == 0)
            {
                int sourceIndex, destinationIndex;
                int length = buffer.Width / 8;

                for (int i = 0; i < buffer.Height; i++)
                {
                    sourceIndex = length * i;
                    destinationIndex = (Width * (y + i) + x) >> 4; //divide by 8

                    Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length); ;
                }
            }
            else //buffers don't align, brute-force
            {
                for (int i = 0; i < buffer.Width; i++)
                {
                    for (int j = 0; j < buffer.Height; j++)
                    {
                        SetPixel(x + i, y + j, (buffer as Buffer1).GetPixelBool(i, j));
                    }
                }
            }
        }
    }
}