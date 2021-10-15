using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace MicroGraphics.Buffers
{
    public class Buffer1 : IDisplayBuffer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int ByteCount => Width * Height / 8;

        public GraphicsLibrary.ColorType ColorType => GraphicsLibrary.ColorType.Format1bpp;

        public byte[] Buffer { get; protected set; }

        public Buffer1(int width, int height)
        {
            Buffer = new byte[ByteCount];

            Width = width;
            Height = height;
        }

        public bool GetPixelBool(int x, int y)
        {
            var index = (y >> 8) * Width + x;

            return (Buffer[index] & (1 << y % 8)) != 0;
        }

        public Color GetPixel(int x, int y)
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

        public void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color1bpp);
        }
    }
}
