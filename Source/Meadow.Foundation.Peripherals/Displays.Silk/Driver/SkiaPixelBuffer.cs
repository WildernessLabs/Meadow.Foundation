using Meadow.Peripherals.Displays;
using SkiaSharp;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a pixel buffer that uses SkiaSharp for rendering.
    /// </summary>
    public class SkiaPixelBuffer : IPixelBuffer
    {
        /// <summary>
        /// Gets the underlying SkiaSharp bitmap.
        /// </summary>
        public SKBitmap SKBitmap { get; private set; }

        /// <summary>
        /// Gets the width of the pixel buffer.
        /// </summary>
        public int Width => SKBitmap.Width;

        /// <summary>
        /// Gets the height of the pixel buffer.
        /// </summary>
        public int Height => SKBitmap.Height;

        /// <summary>
        /// Gets the color mode of the pixel buffer.
        /// </summary>
        public ColorMode ColorMode => ColorMode.Format32bppRgba8888;

        /// <summary>
        /// Gets the bit depth of the pixel buffer.
        /// </summary>
        public int BitDepth => 32;

        /// <summary>
        /// Gets the byte count of the pixel buffer.
        /// </summary>
        public int ByteCount => SKBitmap.ByteCount;

        /// <summary>
        /// Gets the buffer containing pixel data.
        /// </summary>
        public byte[] Buffer => SKBitmap.GetPixelSpan().ToArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="SkiaPixelBuffer"/> class with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the pixel buffer.</param>
        /// <param name="height">The height of the pixel buffer.</param>
        public SkiaPixelBuffer(int width, int height)
        {
            SKBitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888));
        }

        /// <summary>
        /// Clears the pixel buffer, filling it with black.
        /// </summary>
        public void Clear()
        {
            SKBitmap.Erase(SKColors.Black);
        }

        /// <summary>
        /// Fills the entire pixel buffer with the specified color.
        /// </summary>
        /// <param name="color">The color to fill the pixel buffer with.</param>
        public void Fill(Color color)
        {
            SKBitmap.Erase(new SKColor(color.R, color.G, color.B));
        }

        /// <summary>
        /// Fills a specified region of the pixel buffer with the specified color.
        /// </summary>
        /// <param name="originX">The x-coordinate of the region's origin.</param>
        /// <param name="originY">The y-coordinate of the region's origin.</param>
        /// <param name="width">The width of the region.</param>
        /// <param name="height">The height of the region.</param>
        /// <param name="color">The color to fill the region with.</param>
        public void Fill(int originX, int originY, int width, int height, Color color)
        {
            SKBitmap.Erase(
                new SKColor(color.R, color.G, color.B),
                new SKRectI(originX, originY, originX + width, originY + height));
        }

        /// <summary>
        /// Gets the color of the pixel at the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel.</param>
        /// <param name="y">The y-coordinate of the pixel.</param>
        /// <returns>The color of the pixel at the specified coordinates.</returns>
        public Color GetPixel(int x, int y)
        {
            var px = SKBitmap.GetPixel(x, y);
            return new Color(px.Red, px.Green, px.Blue);
        }

        /// <summary>
        /// Inverts the color of the pixel at the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel.</param>
        /// <param name="y">The y-coordinate of the pixel.</param>
        public void InvertPixel(int x, int y)
        {
            var px = SKBitmap.GetPixel(x, y);
            SKBitmap.SetPixel(x, y, new SKColor((byte)~px.Red, (byte)~px.Green, (byte)~px.Blue));
        }

        /// <summary>
        /// Sets the color of the pixel at the specified coordinates.
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel.</param>
        /// <param name="y">The y-coordinate of the pixel.</param>
        /// <param name="color">The color to set the pixel to.</param>
        public void SetPixel(int x, int y, Color color)
        {
            SKBitmap.SetPixel(x, y, new SKColor(color.R, color.G, color.B));
        }

        /// <summary>
        /// Writes the pixel data from another pixel buffer into this buffer at the specified origin.
        /// </summary>
        /// <param name="originX">The x-coordinate of the origin.</param>
        /// <param name="originY">The y-coordinate of the origin.</param>
        /// <param name="buffer">The pixel buffer to copy data from.</param>
        public void WriteBuffer(int originX, int originY, IPixelBuffer buffer)
        {
            if (buffer is SkiaPixelBuffer skiaBuffer && originX == 0 && originY == 0)
            {
                skiaBuffer.SKBitmap.CopyTo(SKBitmap);
                return;
            }

            for (var x = 0; x < buffer.Width; x++)
            {
                for (var y = 0; y < buffer.Height; y++)
                {
                    SetPixel(originX + x, originY + y, buffer.GetPixel(x, y));
                }
            }
        }
    }
}
