using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using System;
using System.IO;
using Xunit;

namespace MicroGraphics.Unit.Tests
{
    public class ImageTests
    {
        [Fact]
        public void TestLoadBitmapFilePositive()
        {
            var bpps = new int[] { 32, 24, 16, 8, 4, 1 };

            foreach (var bpp in bpps)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"wl{bpp}.bmp");

                var image = Image.LoadFromFile(path);

                Assert.NotNull(image);
                Assert.Equal(110, image.Width);
                Assert.Equal(53, image.Height);
                Assert.Equal(bpp, image.BitsPerPixel);
            }
        }

        [Fact]
        public void TestLoadBitmapResourcePositive()
        {
            var image = Image.LoadFromResource("wl32_res.bmp");
            Assert.NotNull(image);
        }
    }
}
