namespace Meadow.Foundation.Graphics.Buffers
{
    public class BufferGray8 : BufferBase
    {
        public override int ByteCount => Width * Height;

        public override GraphicsLibrary.ColorType ColorType => GraphicsLibrary.ColorType.Format8bppGray;

        public BufferGray8(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public BufferGray8(int width, int height) : base(width, height) { }

        public byte GetPixelByte(int x, int y)
        {
            return Buffer[y * Width + x];
        }

        public override Color GetPixel(int x, int y)
        {
            var gray = GetPixelByte(x, y);

            return new Color(gray, gray, gray);
        }

        public void SetPixel(int x, int y, byte gray)
        {
            Buffer[y * Width + x] = gray;
        }

        public override void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.Color8bppGray);
        }
    }
}