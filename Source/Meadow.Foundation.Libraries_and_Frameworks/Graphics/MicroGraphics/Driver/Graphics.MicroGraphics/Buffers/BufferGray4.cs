namespace Meadow.Foundation.Graphics.Buffers
{
    public class BufferGray4 : BufferBase
    {
        public override int ByteCount => Width * Height / 2;

        public override GraphicsLibrary.ColorType ColorType => GraphicsLibrary.ColorType.Format8bppGray;

        public BufferGray4(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public BufferGray4(int width, int height) : base(width, height) { }

        public byte GetPixelByte(int x, int y)
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
            var gray = GetPixelByte(x, y);

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
    }
}