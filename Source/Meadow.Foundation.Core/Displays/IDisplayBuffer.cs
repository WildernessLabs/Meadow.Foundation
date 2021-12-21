
namespace Meadow.Foundation.Graphics.Buffers
{
    public interface IDisplayBuffer
    {
        int Width { get; }

        int Height { get; }

        int ByteCount { get; }

        ColorType displayColorMode { get; }

        byte[] Buffer { get; }

        void SetPixel(int x, int y, Color color);

        Color GetPixel(int x, int y);

        bool WriteBuffer(int x, int y, IDisplayBuffer buffer);

        void Clear();

        void Fill(Color color);

        void Fill(Color color, int x, int y, int width, int height);
    }
}