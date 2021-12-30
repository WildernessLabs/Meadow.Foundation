using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    public class BufferGray4 : BufferBase
    {
        public override int ByteCount => Width * Height / 2;

        public override ColorType displayColorMode => ColorType.Format4bppGray;

        public BufferGray4(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public BufferGray4(int width, int height) : base(width, height) { }

        public byte GetPixel4bpp(int x, int y)
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

        public override Color GetPixel(int x, int y)
        {   //comes back as a 4bit value
            var gray = GetPixel4bpp(x, y);

            return new Color(gray << 4, gray << 4, gray << 4);
        }

        public void SetPixel(int x, int y, byte gray)
        {
            int index = y * Width / 2 + x / 2; 

            if ((x % 2) == 0)
            {   //even pixel - shift to the significant nibble
                Buffer[index] = (byte)((Buffer[index] & 0x0f) | (gray << 4));
            }
            else
            {   //odd pixel
                Buffer[index] = (byte)((Buffer[index] & 0xf0) | (gray));
            }
        }

        public override void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color4bppGray);
        }

        public override void Fill(Color color)
        {
            // split the color in to two byte values
            Buffer[0] = (byte)(color.Color4bppGray | color.Color4bppGray << 4);

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 1; copyLength < arrayMidPoint; copyLength <<= 1)
            {
                Array.Copy(Buffer, 0, Buffer, copyLength, copyLength);
            }

            Array.Copy(Buffer, 0, Buffer, copyLength, Buffer.Length - copyLength);
        }

        public override void Fill(Color color, int x, int y, int width, int height)
        {
            if (x < 0 || x + width > Width ||
                y < 0 || y + height > Height)
            {
                throw new ArgumentOutOfRangeException();
            }

            //TODO optimize
            var bColor = color.Color4bppGray;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    SetPixel(x + i, y + j, bColor);
                }
            }
        }

        public new void WriteBuffer(int x, int y, IDisplayBuffer buffer)
        {
            if (base.WriteBuffer(x, y, buffer))
            {   //call the base for validation
                //and to handle the slow path when buffers don't match
                return;
            }

            //we have a happy path
            if (x%2 == 0 && buffer.Width%2 == 0)
            {
                int sourceIndex, destinationIndex;
                int length = buffer.Width / 2;

                for (int i = 0; i < buffer.Height; i++)
                {
                    sourceIndex = length * i;
                    destinationIndex = (Width * (y + i) + x) >> 2; //divide by 2

                    Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length); ;
                }
            }
            else //buffers don't align, brute-force
            {
                for (int i = 0; i < buffer.Width; i++)
                {
                    for (int j = 0; j < buffer.Height; j++)
                    {
                        SetPixel(x + i, y + j, (buffer as BufferGray4).GetPixel4bpp(i, j));
                    }
                }
            }
        }
    }
}