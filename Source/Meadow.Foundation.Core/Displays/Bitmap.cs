using System;
using System.Collections.Generic;
using System.Text;

namespace Meadow.Foundation.Displays
{
    public class Bitmap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public DisplayColorMode ColorMode { get; private set; }

        byte[] buffer;

        public Bitmap(int width, int height, DisplayColorMode colorMode)
        {
            Height = height;
            Width = width;
            colorMode = colorMode;

            //create the buffer
        }
    }
}
