using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    public class BufferRgb444 : BufferBase
    {
        public override int ByteCount => Width * Height * 3 / 2;

        public override ColorType displayColorMode => ColorType.Format12bppRgb444;

        public BufferRgb444(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public BufferRgb444(int width, int height) : base(width, height) { }

        public ushort GetPixel12bpp(int x, int y)
        {
            byte r, g, b;
            int index;
            if (x % 2 == 0)
            {
                index = (x + y * Width) * 3 / 2;

                r = (byte)(Buffer[index] >> 4);
                g = (byte)(Buffer[index] & 0x0F);
                b = (byte)(Buffer[index + 1] >> 4);
            }
            else
            {
                index = ((x - 1 + y * Width) * 3 / 2) + 1;
                r = (byte)(Buffer[index] & 0x0F);
                g = (byte)(Buffer[index + 1] >> 4);
                b = (byte)(Buffer[index + 1] & 0x0F);
            }

            return (ushort)(r << 8 | g << 4 | b);
        }

        public override Color GetPixel(int x, int y)
        {
            byte r, g, b;
            int index;
            if (x % 2 == 0)
            {
                index = (x + y * Width) * 3 / 2;

                r = (byte)(Buffer[index] >> 4);
                g = (byte)(Buffer[index] & 0x0F);
                b = (byte)(Buffer[index + 1] >> 4);
            }
            else
            {
                index = ((x - 1 + y * Width) * 3 / 2) + 1;
                r = (byte)(Buffer[index] & 0x0F);
                g = (byte)(Buffer[index + 1] >> 4);
                b = (byte)(Buffer[index + 1] & 0x0F);
            }

            r = (byte)(r * 255 / 15);
            g = (byte)(g * 255 / 15);
            b = (byte)(b * 255 / 15);

            return new Color(r, g, b);
        }

        public override void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color12bppRgb444);
        }

        public void SetPixel(int x, int y, ushort color)
        {
            int index;
            //one of 2 possible write patterns 
            if (x % 2 == 0)
            {
                //1st byte RRRRGGGG
                //2nd byte BBBB
                index = ((x + y * Width) * 3 / 2);
                Buffer[index] = (byte)(color >> 4); //think this is correct - grab the r & g values
                index++;
                Buffer[index] = (byte)((Buffer[index] & 0x0F) | (color << 4));
            }
            else
            {
                //1st byte     RRRR
                //2nd byte GGGGBBBB
                index = ((x - 1 + y * Width) * 3 / 2) + 1;
                Buffer[index] = (byte)((Buffer[index] & 0xF0) | (color >> 8));
                Buffer[++index] = (byte)color; //just the lower 8 bits
            }
        }

        public override void Fill(Color color)
        {
            // could do a minor optimization by caching the ushort 444 value 
            Buffer[0] = (byte)  (color.Color12bppRgb444 >> 4);
            Buffer[1] = (byte) ((color.Color12bppRgb444 << 4) | (color.Color12bppRgb444 >> 8));
            Buffer[2] = (byte)   color.Color12bppRgb444;

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 3; copyLength < arrayMidPoint; copyLength <<= 1)
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
            var uColor = color.Color12bppRgb444;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    SetPixel(x + i, y + j, uColor);
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

            WriteBufferSlow(x, y, buffer); //ToDo - optimize 
        }
    }
}
