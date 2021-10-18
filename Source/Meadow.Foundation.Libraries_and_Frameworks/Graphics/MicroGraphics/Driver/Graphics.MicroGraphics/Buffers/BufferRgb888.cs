namespace Meadow.Foundation.Graphics.Buffers
{
    public class BufferRgb888 : BufferBase
    {
        public override int ByteCount => Width * Height * 3;

        public override GraphicsLibrary.ColorType ColorType => GraphicsLibrary.ColorType.Format24bppRgb888;

        public BufferRgb888(int width, int height, byte[] buffer) : base(width, height, buffer) { }

        public BufferRgb888(int width, int height) : base(width, height) { }

        public int GetPixelInt(int x, int y)
        {
            //get current color
            var index = ((y * Width) + x) * 3;

            return (ushort)(Buffer[index] << 16 | Buffer[++index] << 8 | Buffer[++index]);
        }

        public override Color GetPixel(int x, int y)
        {
            var index = ((y * Width) + x) * 3;

            //split into R,G,B & invert
            byte r = Buffer[index];
            byte g = Buffer[index + 1];
            byte b = Buffer[index + 2];

            return new Color(r, g, b);
        }

        public override void SetPixel(int x, int y, Color color)
        {
            var index = ((y * Width) + x) * 3;

            Buffer[index] = color.R;
            Buffer[index + 1] = color.G;
            Buffer[index + 2] = color.B;
        }
    }
}
