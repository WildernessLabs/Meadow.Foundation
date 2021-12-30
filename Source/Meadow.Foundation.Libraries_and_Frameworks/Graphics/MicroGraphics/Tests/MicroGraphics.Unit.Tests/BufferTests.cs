using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using System;
using System.IO;
using Xunit;

namespace MicroGraphics.Unit.Tests
{
    public class BufferTests
    {
        [Fact]
        public void TestLoad16bppReversedDataBitmapRGBAndRowAlign()
        {
            var bpp = 16;

            // these images are 3 rows, in pure R then G then B.  Width is 17 to intentionally not be divisible by 2 or 4.
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"RGB17x3_{bpp}_rev.bmp");

            // this is a 565 image

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
                    var color = BitConverter.ToUInt16(image.DisplayBuffer.Buffer, index);

                    switch (r)
                    {
                        case 0:
                            // in 565 red is 500 (as in which bits are on)
                            Assert.Equal(0b1111100000000000, color);

                            pixel = image.DisplayBuffer.GetPixel(c, r);
                            //                            Assert.Equal(Color.Red, pixel);
                            break;
                        case 1:
                            // in 565 green is 060 (as in which bits are on)
                            Assert.Equal(0b0000011111100000, color);

                            pixel = image.DisplayBuffer.GetPixel(c, r);
                            //                            Assert.Equal(Color.Lime, pixel);
                            break;
                        case 2:
                            // in 565 blue is 005 (as in which bits are on)
                            Assert.Equal(0b0000000000011111, color);

                            pixel = image.DisplayBuffer.GetPixel(c, r);
                            //                            Assert.Equal(Color.Blue, pixel);
                            break;
                    }

                    index += 2; // 16bpp
                }
            }
        }

        [Fact]
        public void TestLoad16bppBitmapRGBAndRowAlign()
        {
            var bpp = 16;

            // these images are 3 rows, in pure R then G then B.  Width is 17 to intentionally not be divisible by 2 or 4.
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"RGB17x3_{bpp}.bmp");

            // this is a 565 image

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
                    var color = BitConverter.ToUInt16(image.DisplayBuffer.Buffer, index);

                    switch (r)
                    {
                        case 0:
                            // in 565 red is 500 (as in which bits are on)
                            Assert.Equal(0b1111100000000000, color);

                            pixel = image.DisplayBuffer.GetPixel(c, r);
                            Assert.Equal(Color.Red, pixel);
                            break;
                        case 1:
                            // in 565 green is 060 (as in which bits are on)
                            Assert.Equal(0b0000011111100000, color);

                            pixel = image.DisplayBuffer.GetPixel(c, r);
                            Assert.Equal(Color.Lime, pixel);
                            break;
                        case 2:
                            // in 565 blue is 005 (as in which bits are on)
                            Assert.Equal(0b0000000000011111, color);

                            pixel = image.DisplayBuffer.GetPixel(c, r);
                            Assert.Equal(Color.Blue, pixel);
                            break;
                    }

                    index += 2; // 16bpp
                }
            }
        }

        [Fact]
        public void TestLoad24bppBitmapRGBAndRowAlign()
        {
            var bpp = 24;

            // these images are 3 rows, in pure R then G then B.  Width is 17 to intentionally not be divisible by 2 or 4.
            // first pixel in each row is 0xaaaaaa to help find row start in hex editors
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"RGB17x3_{bpp}_row_marker.bmp");

            var image = Image.LoadFromFile(path);

            Assert.NotNull(image);
            Assert.Equal(17, image.Width);
            Assert.Equal(3, image.Height);
            Assert.Equal(bpp, image.BitsPerPixel);

            var index = 0;
            Color pixel;
            Color rowStartColor = new Color(0xaa, 0xaa, 0xaa);
            for (var r = 0; r < image.Height; r++)
            {
                for (var c = 0; c < image.Width; c++)
                {
                    pixel = image.DisplayBuffer.GetPixel(c, r);

                    if (c == 0)
                    {
                        Assert.Equal(rowStartColor, pixel);
                    }
                    else
                    {
                        switch (r)
                        {
                            case 0:
                                Assert.Equal(Color.Red, pixel);
                                Assert.Equal(255, image.DisplayBuffer.Buffer[index + 0]);
                                break;
                            case 1:
                                Assert.Equal(Color.Lime, pixel);
                                Assert.Equal(255, image.DisplayBuffer.Buffer[index + 1]);
                                break;
                            case 2:
                                Assert.Equal(Color.Blue, pixel);
                                Assert.Equal(255, image.DisplayBuffer.Buffer[index + 2]);
                                break;
                        }
                    }

                    index += 3; // 24bpp
                }
            }
        }

        [Fact]
        public void TestLoad32bppBitmapRGBAndRowAlign()
        {
            var bpp = 32;

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
                            Assert.Equal(Color.Lime, pixel);
                            Assert.Equal(255, image.DisplayBuffer.Buffer[index + 1]);
                            break;
                        case 2:
                            pixel = image.DisplayBuffer.GetPixel(c, r);
                            Assert.Equal(Color.Blue, pixel);
                            Assert.Equal(255, image.DisplayBuffer.Buffer[index + 2]);
                            break;
                    }

                    //look at the A value

                    Assert.Equal(255, image.DisplayBuffer.Buffer[index + 3]);

                    index += 4; // 32bpp
                }
            }
        }
    }
}
