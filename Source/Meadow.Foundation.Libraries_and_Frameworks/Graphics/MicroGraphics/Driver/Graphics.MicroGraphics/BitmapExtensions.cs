using Meadow.Foundation;
using Meadow.Foundation.Displays;
using System;

namespace Meadow.Foundation.Displays
{
    public static class BitmapExtensions
    {
        public static Bitmap ConvertBitmap(this Bitmap bitmap, DisplayColorMode convertTo)
        {
            if (bitmap.ColorMode != DisplayColorMode.Format24bppRgb888)
            {
                throw new ArgumentException($"ConvertBitmap can only convert from Format24bppRgb888 Bitmaps");
            }

            switch (convertTo)
            {
                case DisplayColorMode.Format16bppRgb565:
                    return Convert888BitmapTo565Bitmap(bitmap, convertTo);
                case DisplayColorMode.Format12bppRgb444:
                    return Convert888BitmapTo444Bitmap(bitmap, convertTo);
            }
                
            throw new ArgumentException($"ConvertBitmap can only convert to 16bpp or 12bpp Bitmaps");
        }

        static Bitmap Convert888BitmapTo565Bitmap(Bitmap bitmap, DisplayColorMode convertTo)
        {
            var buffer = new byte[Bitmap.GetBufferSize(bitmap.Width, bitmap.Height, convertTo)];

            int x = 0;
            int y = 0;
            byte r, g, b;
            ushort color;

            int index;

            //888 to 565
            for (int i = 0; i < bitmap.Buffer.Length; i+= 3)
            {
                r = bitmap.Buffer.Span[i];
                g = bitmap.Buffer.Span[i + 1];
                b = bitmap.Buffer.Span[i + 2];

                color = Get16BitColorFromRGB(r, g, b);

                index = ((y * bitmap.Width) + x) * sizeof(ushort);

                buffer[index] = (byte)(color >> 8);
                buffer[++index] = (byte)(color);

                x++;

                if (x % bitmap.Width == 0)
                {
                    y++;
                    x = 0;
                }
            }

            return new Bitmap(bitmap.Width, bitmap.Height, convertTo, buffer);
        }

        static Bitmap Convert888BitmapTo444Bitmap(Bitmap bitmap, DisplayColorMode convertTo)
        {
            var buffer = new byte[Bitmap.GetBufferSize(bitmap.Width, bitmap.Height, convertTo)];

            int x = 0;
            int y = 0;
            byte r, g, b;
            ushort color;

            int index;

            //888 to 565
            for (int i = 0; i < bitmap.Buffer.Length; i += 3)
            {
                r = bitmap.Buffer.Span[i];
                g = bitmap.Buffer.Span[i + 1];
                b = bitmap.Buffer.Span[i + 2];

                color = Get12BitColorFromRGB(r, g, b);

                //one of 2 possible write patterns 
                if (x % 2 == 0)
                {   //1st byte RRRRGGGG
                    //2nd byte BBBB
                    index = (x + y * bitmap.Width) * 3 / 2;
                    buffer[index] = (byte)(color >> 4); //think this is correct - grab the r & g values
                    index++;
                    buffer[index] = (byte)((buffer[index] & 0x0F) | (color << 4));
                }
                else
                {   //1st byte     RRRR
                    //2nd byte GGGGBBBB
                    index = ((x - 1 + y * bitmap.Width) * 3 / 2) + 1;
                    buffer[index] = (byte)((buffer[index] & 0xF0) | (color >> 8));
                    buffer[++index] = (byte)color; //just the lower 8 bits
                }

                x++;

                if (x % bitmap.Width == 0)
                {
                    y++;
                    x = 0;
                }
            }

            return new Bitmap(bitmap.Width, bitmap.Height, convertTo, buffer);
        }

        public static ushort ColorTo16BppUshort(Color color)
        {
            //this seems heavy
            byte red = (byte)(color.R * 255.0);
            byte green = (byte)(color.G * 255.0);
            byte blue = (byte)(color.B * 255.0);

            return Get16BitColorFromRGB(red, green, blue);
        }

        public static ushort ColorTo12BppUshort(Color color)
        {
            //this seems heavy
            byte red = (byte)(color.R * 255.0);
            byte green = (byte)(color.G * 255.0);
            byte blue = (byte)(color.B * 255.0);

            return Get12BitColorFromRGB(red, green, blue);
        }

        public static ushort Get12BitColorFromRGB(byte red, byte green, byte blue)
        {
            red >>= 4;
            green >>= 4;
            blue >>= 4;

            return (ushort)(red << 8 | green << 4 | blue);
        }

        public static ushort Get16BitColorFromRGB(byte red, byte green, byte blue)
        {
            red >>= 3;
            green >>= 2;
            blue >>= 3;

            return (ushort)(red << 11 | green << 5 | blue);
        }
    }
}
