using System;

namespace Meadow.Foundation.Graphics.Buffers
{
    public class BufferRgb565 : BufferBase
    {
        public override int ByteCount => Width * Height * 2;

        public override ColorType displayColorMode => ColorType.Format16bppRgb565;

        public BufferRgb565(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public BufferRgb565(int width, int height) : base(width, height) { }

        public ushort GetPixelUShort(int x, int y)
        {
            //get current color
            var index = ((y * Width) + x) * sizeof(ushort);

            return (ushort)(Buffer[index] << 8 | Buffer[++index]);
        }

        public override Color GetPixel(int x, int y)
        {
            ushort color = GetPixelUShort(x, y);

            //split into R,G,B & invert
            byte r = (byte)((color >> 11) & 0x1F);
            byte g = (byte)((color >> 5) & 0x3F);
            byte b = (byte)((color) & 0x1F);

            return new Color(r, g, b);
        }

        public void SetPixel(int x, int y, ushort color)
        {
            var index = ((y * Width) + x) * 2;

            Buffer[index] = (byte)(color >> 8);
            Buffer[++index] = (byte)color;
        }

        public override void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color16bppRgb565);
        }

        public override void Clear(Color color)
        {
            Clear(color.Color16bppRgb565);
        }

        public void Clear(ushort color)
        { 
            // split the color in to two byte values
            Buffer[0] = (byte)(color >> 8);
            Buffer[0] = (byte)color;

            int arrayMidPoint = Buffer.Length / 2;
            int copyLength;

            for (copyLength = 2; copyLength < arrayMidPoint; copyLength <<= 1)
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

            int sourceIndex, destinationIndex;
            int length = buffer.Width * 2;

            for (int i = 0; i < buffer.Height; i++)
            {
                sourceIndex = length * i;
                destinationIndex = Width * (y + i) * 2 + x * 2;

                Array.Copy(buffer.Buffer, sourceIndex, Buffer, destinationIndex, length); ;
            }
        }
    }
}
