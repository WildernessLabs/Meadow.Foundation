using Meadow.Foundation;
using static Meadow.Foundation.Graphics.GraphicsLibrary;

namespace MicroGraphics
{
    public interface IDisplayBuffer
    {
        int Width { get; }

        int Height { get; }

        int ByteCount { get; }

        ColorType ColorType { get; }

        byte[] Buffer { get; }

        void SetPixel(int x, int y, Color color);

        Color GetPixel(int x, int y);
    }
}
