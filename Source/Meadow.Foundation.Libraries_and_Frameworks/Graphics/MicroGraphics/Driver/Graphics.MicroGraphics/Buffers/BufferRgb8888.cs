using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    public class BufferRgb8888 : BufferBase
    {
        public override int ByteCount => Width * Height * 4;

        public override ColorType displayColorMode => ColorType.Format32bppRgba8888;

        public BufferRgb8888(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public BufferRgb8888(int width, int height) : base(width, height) { }

        public int GetPixelInt(int x, int y)
        {
            //get current color
            var index = ((y * Width) + x) * 4;

            return (int)(Buffer[index] << 24 | Buffer[++index] << 16 | Buffer[++index] << 8 | Buffer[++index]);
        }

        public override Color GetPixel(int x, int y)
        {
            var index = ((y * Width) + x) * 4;

            //split into R,G,B & invert
            byte r = Buffer[index];
            byte g = Buffer[index + 1];
            byte b = Buffer[index + 2];
            byte a = Buffer[index + 3];

            return new Color(r, g, b, a);
        }

        public override void SetPixel(int x, int y, Color color)
        {
            var index = ((y * Width) + x) * 4;

            Buffer[index] = color.R;
            Buffer[index + 1] = color.G;
            Buffer[index + 2] = color.B;
            Buffer[index + 3] = color.A;
        }

        public override void Fill(Color color)
        {
            // split the color in to two byte values
            Buffer[0] = color.R;
            Buffer[1] = color.G;
            Buffer[2] = color.B;
            Buffer[3] = color.A;

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 4; copyLength < arrayMidPoint; copyLength <<= 1)
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

            byte[] value = { color.R, color.G, color.B, color.A };
            int index = y * Width * 4 + x * 4 - 1;

            //fill the first line
            for (int i = 0; i < width; i++)
            {
                Buffer[++index] = value[0];
                Buffer[++index] = value[1];
                Buffer[++index] = value[2];
                Buffer[++index] = value[3];
            }

            //array copy the rest
            for (int j = 0; j < height - 1; j++)
            {
                Array.Copy(Buffer,
                    (y + j) * Width * 4 + x * 4,
                    Buffer,
                    (y + j + 1) * Width * 4 + x * 4,
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
            int length = buffer.Width * 4;

            for (int i = 0; i < buffer.Height; i++)
            {
                sourceIndex = length * i;
                destinationIndex = Width * (y + i) * 4 + x * 4;

                Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length); ;
            }
        }
    }
}