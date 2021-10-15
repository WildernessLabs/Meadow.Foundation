using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace MicroGraphics.Buffers
{
    public class BufferGray8 : IDisplayBuffer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int ByteCount => Width * Height;

        public GraphicsLibrary.ColorType ColorType => GraphicsLibrary.ColorType.Format8bppGray;

        public byte[] Buffer { get; protected set; }

        public BufferGray8(int width, int height)
        {
            Buffer = new byte[ByteCount];

            Width = width;
            Height = height;
        }

        public byte GetPixelByte(int x, int y)
        {
            return Buffer[y * Width + x];
        }

        public Color GetPixel(int x, int y)
        {
            var gray = GetPixelByte(x, y);

            return new Color(gray, gray, gray);
        }

        public void SetPixel(int x, int y, byte gray)
        {
            Buffer[y * Width + x] = gray;
        }

        public void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color8bppGray);
        }
    }
}