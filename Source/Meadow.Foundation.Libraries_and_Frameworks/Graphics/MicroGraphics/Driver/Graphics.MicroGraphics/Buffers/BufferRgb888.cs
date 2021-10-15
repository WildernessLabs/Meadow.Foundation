using System;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;

namespace MicroGraphics.Buffers
{
    public class BufferRgb888 : IDisplayBuffer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int ByteCount => Width * Height * 3;

        public GraphicsLibrary.ColorType ColorType => GraphicsLibrary.ColorType.Format24bppRgb888;

        public byte[] Buffer { get; protected set; }

        public BufferRgb888(int width, int height)
        {
            Buffer = new byte[ByteCount];

            Width = width;
            Height = height;
        }

        public int GetPixelInt(int x, int y)
        {
            //get current color
            var index = ((y * Width) + x) * 3;

            return (ushort)(Buffer[index] << 16 | Buffer[++index] << 8 | Buffer[++index]);
        }

        public Color GetPixel(int x, int y)
        {
            var index = ((y * Width) + x) * 3;

            //split into R,G,B & invert
            byte r = Buffer[index];
            byte g = Buffer[index + 1];
            byte b = Buffer[index + 2];

            return new Color(r, g, b);
        }

        public void SetPixel(int x, int y, Color color)
        {
            var index = ((y * Width) + x) * 3;

            Buffer[index] = color.R;
            Buffer[index + 1] = color.G;
            Buffer[index + 2] = color.B;
        }

        public void Clear()
        {
            Array.Clear(Buffer, 0, Buffer.Length);
        }
    }
}
