using System;

namespace Meadow.Foundation.Displays
{
    //for ref https://api.skia.org/classSkBitmap.html

    public class Bitmap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public DisplayColorMode ColorMode { get; private set; }

        public Memory<byte> Buffer { get; private set; }

        public Bitmap(int width, int height, DisplayColorMode colorMode, byte[] bitmapData)
        {
            if(GetBufferSize(width, height, colorMode) != bitmapData.Length)
            {
                throw new ArgumentException($"Bitmap width: {width} and hieght {height} do not match bitmapData length: {bitmapData.Length} for colorMode: {colorMode}");
            }

            Height = height;
            Width = width;
            ColorMode = colorMode;

            Buffer = new Memory<byte>(bitmapData);
        }

        public Bitmap(int width, int height, DisplayColorMode colorMode)
        {
            Height = height;
            Width = width;
            ColorMode = colorMode;

            Buffer = new byte[GetBufferSize(width, height, colorMode)];
        }

        //could be useful
        public static int GetBufferSize(int width, int height, DisplayColorMode colorMode)
        {
            int size;

            switch (colorMode)
            {
                case DisplayColorMode.Format1bpp:
                    size = width * height / 8;
                    break;
                case DisplayColorMode.Format2bpp:
                    size = width * height / 4;
                    break;
                case DisplayColorMode.Format4bpp:
                    size = width * height / 2;
                    break;
                case DisplayColorMode.Format8bppMonochome:
                case DisplayColorMode.Format8bppRgb332:
                    size = width * height;
                    break;
                case DisplayColorMode.Format12bppRgb444:
                    size = width * height * 3 / 2;
                    break;
                case DisplayColorMode.Format18bppRgb666:
                    size = width * height * 9 / 2;
                    break;
                case DisplayColorMode.Format24bppRgb888:
                    size = width * height * 3;
                    break;
                case DisplayColorMode.Format16bppRgb555:
                case DisplayColorMode.Format16bppRgb565:
                default:
                    size = width * height * 2;
                    break;
            }
            return size;
        }
    }
}