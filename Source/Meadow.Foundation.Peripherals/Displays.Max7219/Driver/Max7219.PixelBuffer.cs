using Meadow.Foundation.Graphics.Buffers;

namespace Meadow.Foundation.Displays
{
    public partial class Max7219 : IPixelBuffer
    {
        public int BitDepth => 1;

        public int ByteCount => Width * Height / 8;

        public byte[] Buffer => throw new System.NotImplementedException();

        public void Fill(Color color)
        {
            Fill(color, false);
        }

        public Color GetPixel(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public void SetPixel(int x, int y, Color color) => DrawPixel(x, y, color);
    }
}
