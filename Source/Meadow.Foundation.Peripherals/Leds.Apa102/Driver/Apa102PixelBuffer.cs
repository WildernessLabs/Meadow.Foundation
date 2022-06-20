using Meadow.Foundation.Graphics.Buffers;

namespace Meadow.Foundation.Leds
{
    public partial class Apa102 : IPixelBuffer
    {
        public int BitDepth => 24;

        public int ByteCount => NumberOfLeds * 3;

        public byte[] Buffer => throw new System.NotImplementedException();

        public void Fill(Color color) => Fill(color, false);

        public Color GetPixel(int x, int y)
        {
            throw new System.NotImplementedException();
        }

        public void SetPixel(int x, int y, Color color) => SetLed(x, color);
    }
}