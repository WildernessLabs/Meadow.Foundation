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
        public void TestLoad24bppBitmapRGBAndRowAlign()
        {
            var bpp = 24;

            // these images are 3 rows, in pure R then G then B.  Width is 17 to intentionally not be divisible by 2 or 4.
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"RGB17x3_{bpp}.bmp");

            var image = Image.LoadFromFile(path);

            Assert.NotNull(image);
            Assert.Equal(17, image.Width);
            Assert.Equal(3, image.Height);
            Assert.Equal(bpp, image.BitsPerPixel);

            var index = 0;
            Color pixel;

            for (var r = 0; r < image.Height; r++)
            {
                for (var c = 0; c < image.Width; c++)
                {
                    switch (r)
                    {
                        case 0:
                            pixel = image.DisplayBuffer.GetPixel(c, r);
                            Assert.Equal(Color.Red, pixel);
                            Assert.Equal(255, image.DisplayBuffer.Buffer[index + 0]);
                            break;
                        case 1:
                            pixel = image.DisplayBuffer.GetPixel(c, r);
                            Assert.Equal(Color.Green, pixel);
                            Assert.Equal(255, image.DisplayBuffer.Buffer[index + 1]);
                            break;
                        case 2:
                            pixel = image.DisplayBuffer.GetPixel(c, r);
                            Assert.Equal(Color.Blue, pixel);
                            Assert.Equal(255, image.DisplayBuffer.Buffer[index + 2]);
                            break;
                    }

                    index += 3; // 24bpp
                }
            }
        }

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
