using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace MicroGraphics.Buffers
{
    public class BufferRgb565 : IDisplayBuffer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int ByteCount => Width * Height * 2;

        public GraphicsLibrary.ColorType ColorType => GraphicsLibrary.ColorType.Format16bppRgb565;

        public byte[] Buffer { get; protected set; }

        public BufferRgb565(int width, int height)
        {
            Buffer = new byte[ByteCount];

            Width = width;
            Height = height;
        }

        public ushort GetPixelUShort(int x, int y)
        {
            //get current color
            var index = ((y * Width) + x) * sizeof(ushort);

            return (ushort)(Buffer[index] << 8 | Buffer[++index]);
        }

        public Color GetPixel(int x, int y)
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

        public void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color16bppRgb565);
        }
    }
}
