using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    public class BufferGray8 : BufferBase
    {
        public override int ByteCount => Width * Height;

        public override ColorType displayColorMode => ColorType.Format8bppGray;

        public BufferGray8(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public BufferGray8(int width, int height) : base(width, height) { }

        public byte GetPixelByte(int x, int y)
        {
            return Buffer[y * Width + x];
        }

        public override Color GetPixel(int x, int y)
        {
            var gray = GetPixelByte(x, y);

            return new Color(gray, gray, gray);
        }

        public void SetPixel(int x, int y, byte gray)
        {
            Buffer[y * Width + x] = gray;
        }

        public override void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color8bppGray);
        }

        public override void Fill(Color color)
        {
            // split the color in to two byte values
            Buffer[0] = color.Color8bppGray;

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

            byte value = color.Color8bppGray;
            int index = y * Width + x - 1;

            //fill the first line
            for (int i = 0; i < width; i++)
            {
                Buffer[++index] = value;
            }

            //array copy the rest
            for (int j = 0; j < height - 1; j++)
            {
                Array.Copy(Buffer,
                    (y + j) * Width + x,
                    Buffer,
                    (y + j + 1) * Width + x,
                    width);
            }
        }

        public new void WriteBuffer(int x, int y, IDisplayBuffer buffer)
        {
            if (base.WriteBuffer(x, y, buffer))
            {   //call the base for validation
                //and to handle the slow path when buffers don't match
                return;
            }

            int sourceIndex, destinationIndex;
            int length = buffer.Width;

            for (int i = 0; i < buffer.Height; i++)
            {
                sourceIndex = length * i;
                destinationIndex = Width * (y + i) + x;

                Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length); ;
            }
        }
    }
}