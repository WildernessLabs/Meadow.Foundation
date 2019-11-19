using Meadow.Foundation.Displays;
using System;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    ///     Provide high level graphics functions
    /// </summary>
    public class GraphicsLibrary
    {
        #region Member variables / fields

        private readonly DisplayBase _display;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Current font used for displaying text on the display.
        /// </summary>
        public FontBase CurrentFont { get; set; }

        /// <summary>
        /// Current rotation used for drawing pixels to the display
        /// </summary>
        public RotationType Rotation { get; set; } = RotationType.Default;

        #endregion Properties

        /// <summary>
        /// Display rotation 
        /// </summary>
        public enum RotationType
        {
            Default,
            _90Degrees,
            _180Degrees,
            _270Degrees
        }

        #region Constructors

        /// <summary>
        /// </summary>
        /// <param name="display"></param>
        public GraphicsLibrary(DisplayBase display)
        {
            _display = display;
            CurrentFont = null;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Draw a single pixel using the pen color
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        public void DrawPixel(int x, int y)
        {
            _display.DrawPixel(GetXForRotation(x, y), GetYForRotation(x, y));
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public void DrawPixel (int x, int y, bool colored)
        {


            _display.DrawPixel(GetXForRotation(x,y), GetYForRotation(x,y), colored);
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">Color of pixel.</param>
        public void DrawPixel (int x, int y, Color color)
        {
            _display.DrawPixel(GetXForRotation(x, y), GetYForRotation(x, y), color);
        }

        /// <summary>
        ///     Draw a line using Bresenhams line drawing algorithm.
        /// </summary>
        /// <remarks>
        ///     Bresenhams line drawing algoritm:
        ///     https://en.wikipedia.org/wiki/Bresenham's_line_algorithm
        ///     C# Implementation:
        ///     https://en.wikipedia.org/wiki/Bresenham's_line_algorithm
        /// </remarks>
        /// <param name="x0">Abscissa of the starting point of the line.</param>
        /// <param name="y0">Ordinate of the starting point of the line</param>
        /// <param name="x1">Abscissa of the end point of the line.</param>
        /// <param name="y1">Ordinate of the end point of the line</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public void DrawLine(int x0, int y0, int x1, int y1, bool colored = true)
        {
            DrawLine(x0, y0, x1, y1, (colored ? Color.White : Color.Black));
        }

        /// <summary>
        ///     Draw a line using Bresenhams line drawing algorithm.
        /// </summary>
        /// <remarks>
        ///     Bresenhams line drawing algoritm:
        ///     https://en.wikipedia.org/wiki/Bresenham's_line_algorithm
        ///     C# Implementation:
        ///     https://en.wikipedia.org/wiki/Bresenham's_line_algorithm
        /// </remarks>
        /// <param name="x0">Abscissa of the starting point of the line.</param>
        /// <param name="y0">Ordinate of the starting point of the line</param>
        /// <param name="x1">Abscissa of the end point of the line.</param>
        /// <param name="y1">Ordinate of the end point of the line</param>
        /// <param name="color">The color of the line.</param>
        public void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            _display.SetPenColor(color);

            var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            var dx = x1 - x0;
            var dy = Math.Abs(y1 - y0);
            var error = dx / 2;
            var ystep = y0 < y1 ? 1 : -1;
            var y = y0;
            for (var x = x0; x <= x1; x++)
            {
                DrawPixel(steep ? y : x, steep ? x : y);
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }

        /// <summary>
        ///     Draw a horizontal line.
        /// </summary>
        /// <param name="x0">Abscissa of the starting point of the line.</param>
        /// <param name="y0">Ordinate of the starting point of the line.</param>
        /// <param name="length">Length of the line to draw.</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public void DrawHorizontalLine(int x0, int y0, int length, bool colored = true)
        {
            for (var x = x0; (x - x0) < length; x++)
            {
                DrawPixel(x, y0, colored);
            }
        }

        /// <summary>
        ///     Draw a horizontal line.
        /// </summary>
        /// <param name="x0">Abscissa of the starting point of the line.</param>
        /// <param name="y0">Ordinate of the starting point of the line.</param>
        /// <param name="length">Length of the line to draw.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawHorizontalLine(int x0, int y0, int length, Color color)
        {
            _display.SetPenColor(color);

            for (var x = x0; (x - x0) < length; x++)
            {
                DrawPixel(x, y0);
            }
        }

        /// <summary>
        ///     Draw a vertical line.
        /// </summary>
        /// <param name="x0">Abscissa of the starting point of the line.</param>
        /// <param name="y0">Ordinate of the starting point of the line.</param>
        /// <param name="length">Length of the line to draw.</param>
        /// <param name="colored">Show the line when (true) or off (false).</param>
        public void DrawVerticalLine(int x0, int y0, int length, bool colored = true)
        {
            for (var y = y0; (y - y0) < length; y++)
            {
                DrawPixel(x0, y, colored);
            }
        }

        /// <summary>
        ///     Draw a vertical line.
        /// </summary>
        /// <param name="x0">Abscissa of the starting point of the line.</param>
        /// <param name="y0">Ordinate of the starting point of the line.</param>
        /// <param name="length">Length of the line to draw.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawVerticalLine(int x0, int y0, int length, Color color)
        {
            _display.SetPenColor(color);

            for (var y = y0; (y - y0) < length; y++)
            {
                DrawPixel(x0, y);
            }
        }

        /// <summary>
        ///  Draw a  triangle
        /// </summary>
        ///  <param name="x0">Vertex #0 x coordinate</param>
        ///  <param name="y0">Vertex #0 y coordinate</param>
        ///  <param name="x1">Vertex #1 x coordinate</param>
        ///  <param name="y1">Vertex #1 y coordinate</param>
        ///  <param name="x2">Vertex #2 x coordinate</param>
        ///  <param name="y2">Vertex #2 y coordinate</param>
        ///  <param name="color">Color of triangle</param>
        ///  <param name="filled">Draw a filled triangle?</param>
        public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Color color, bool filled = false)
        {
            if(filled)
            {
                DrawTriangleFilled(x0, y0, x1, y1, x2, y2, color);
            }
            else
            {
                DrawLine(x0, y0, x1, y1, color);
                DrawLine(x1, y1, x2, y2, color);
                DrawLine(x2, y2, x0, y0, color);
            }
        }

        /// <summary>
        ///  Draw a  triangle
        /// </summary>
        /// <remarks>
        /// Draw triangle method for 1 bit displays
        /// </remarks>
        /// <param name="x0">Vertex #0 x coordinate</param>
        /// <param name="y0">Vertex #0 y coordinate</param>
        /// <param name="x1">Vertex #1 x coordinate</param>
        /// <param name="y1">Vertex #1 y coordinate</param>
        /// <param name="x2">Vertex #2 x coordinate</param>
        /// <param name="y2">Vertex #2 y coordinate</param>
        /// <param name="colored">Should the triangle add (true) or remove (false)</param>
        /// <param name="filled">Draw a filled triangle?</param>
        public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, bool colored, bool filled = false)
        {
            if (filled)
                DrawTriangleFilled(x0, y0, x1, y1, x2, y2, colored ? Color.White : Color.Black);
            else 
                DrawTriangle(x0, y0, x1, y1, x2, y2, colored ? Color.White : Color.Black);
        }

        void Swap(ref int value1, ref int value2)
        {
            int temp = value1;
            value1 = value2;
            value2 = temp;
        }

        /// <summary>
        /// Draw a filled triangle
        /// </summary>
        /// <param name="x0">Vertex #0 x coordinate</param>
        /// <param name="y0">Vertex #0 y coordinate</param>
        /// <param name="x1">Vertex #1 x coordinate</param>
        /// <param name="y1">Vertex #1 y coordinate</param>
        /// <param name="x2">Vertex #2 x coordinate</param>
        /// <param name="y2">Vertex #2 y coordinate</param>
        /// <param name="color">Color to fill/draw with</param>
        void DrawTriangleFilled(int x0, int y0, int x1, int y1, int x2, int y2, Color color)
        {
            // Sort coordinates by Y order (y2 >= y1 >= y0)
            if (y0 > y1)
            {
                Swap(ref y0, ref y1);
                Swap(ref x0, ref x1);
            }
            if (y1 > y2)
            {
                Swap(ref y2, ref y1);
                Swap(ref x2, ref x1);
            }
            if (y0 > y1)
            {
                Swap(ref y0, ref y1);
                Swap(ref x0, ref x1);
            }

            if (y0 == y2)
            { // Handle awkward all-on-same-line case as its own thing
                int x = x0, len = x0;
                if (x1 < x) x = x1;
                else if (x1 > len) len = x1;
                if (x2 < x) x = x2;
                else if (x2 > len) len = x2;
                DrawHorizontalLine(x, y0, len - x + 1, color);
                return;
            }

            int dx01 = x1 - x0,
                dy01 = y1 - y0,
                dx02 = x2 - x0,
                dy02 = y2 - y0,
                dx12 = x2 - x1,
                dy12 = y2 - y1;
            int sa = 0, sb = 0;

            int last = (y1 == y2) ? y1 : y1 - 1;

            int a, b, y;
            for (y = y0; y <= last; y++)
            {
                a = x0 + sa / dy01;
                b = x0 + sb / dy02;
                sa += dx01;
                sb += dx02;

                if (a > b)
                    Swap(ref a, ref b);
                DrawHorizontalLine(a, y, b - a + 1, color);
            }

            // For lower part of triangle, find scanline crossings for segments
            // 0-2 and 1-2.  This loop is skipped if y1=y2.
            sa = dx12 * (y - y1);
            sb = dx02 * (y - y0);
            for (; y <= y2; y++)
            {
                a = x1 + sa / dy12;
                b = x0 + sb / dy02;
                sa += dx12;
                sb += dx02;

                if (a > b) Swap(ref a, ref b);
                DrawHorizontalLine(a, y, b - a + 1, color);
            }
        }

        /// <summary>
        ///     Draw a dircle.
        /// </summary>
        /// <remarks>
        ///     This algorithm draws the circle by splitting the full circle into eight
        ///     segments.
        ///     This method uses the Midpoint algorithm:
        ///     https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        ///     A C# implementation can be found here:
        ///     https://rosettacode.org/wiki/Bitmap/Midpoint_circle_algorithm#C.23
        /// </remarks>
        /// <param name="centerX">Abscissa of the centre point of the circle.</param>
        /// <param name="centerY">Ordinate of the centre point of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="colored">Show the circle when true.</param>
        /// <param name="filled">Draw a filled circle?</param>
        public void DrawCircle(int centerX, int centerY, int radius, bool colored = true, bool filled = false)
        {
            DrawCircle(centerX, centerY, radius, (colored ? Color.White : Color.Black), filled);
        }

        /// <summary>
        ///     Draw a dircle.
        /// </summary>
        /// <remarks>
        ///     This algorithm draws the circle by splitting the full circle into eight
        ///     segments.
        ///     This method uses the Midpoint algorithm:
        ///     https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        ///     A C# implementation can be found here:
        ///     https://rosettacode.org/wiki/Bitmap/Midpoint_circle_algorithm#C.23
        /// </remarks>
        /// <param name="centerX">Abscissa of the centre point of the circle.</param>
        /// <param name="centerY">Ordinate of the centre point of the circle.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <param name="color">The color of the circle.</param>
        /// <param name="filled">Draw a filled circle?</param>
        public void DrawCircle(int centerX, int centerY, int radius, Color color, bool filled = false)
        {
            _display.SetPenColor(color);

            var d = (5 - (radius * 4)) / 4;
            var x = 0;
            var y = radius;
            while (x <= y)
            {
                if (filled)
                {
                    DrawLine(centerX + x, centerY + y, centerX - x, centerY + y);
                    DrawLine(centerX + x, centerY - y, centerX - x, centerY - y);
                    DrawLine(centerX - y, centerY + x, centerX + y, centerY + x);
                    DrawLine(centerX - y, centerY - x, centerX + y, centerY - x);
                }
                else
                {
                    DrawPixel(centerX + x, centerY + y);
                    DrawPixel(centerX + y, centerY + x);
                    DrawPixel(centerX - y, centerY + x);
                    DrawPixel(centerX - x, centerY + y);
                    DrawPixel(centerX - x, centerY - y);
                    DrawPixel(centerX - y, centerY - x);
                    DrawPixel(centerX + x, centerY - y);
                    DrawPixel(centerX + y, centerY - x);
                }
                if (d < 0)
                {
                    d += (2 * x) + 1;
                }
                else
                {
                    d += (2 * (x - y)) + 1;
                    y--;
                }
                x++;
            }
        }

        /// <summary>
        ///     Draw a rectangle.
        /// </summary>
        /// <param name="xLeft">Abscissa of the top left corner.</param>
        /// <param name="yTop">Ordinate of the top left corner.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="colored">Draw the pixel (true) or turn the pixel off (false).</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default).</param>
        public void DrawRectangle(int xLeft, int yTop, int width, int height, bool colored = true, bool filled = false)
        {
            width--;
            height--;
            if (filled)
            {
                for (var i = 0; i <= height; i++)
                {
                    DrawLine(xLeft, yTop + i, xLeft + width, yTop + i, colored);
                }
            }
            else
            {
                DrawLine(xLeft, yTop, xLeft + width, yTop, colored);
                DrawLine(xLeft + width, yTop, xLeft + width, yTop + height, colored);
                DrawLine(xLeft + width, yTop + height, xLeft, yTop + height, colored);
                DrawLine(xLeft, yTop, xLeft, yTop + height, colored);
            }
        }

        /// <summary>
        ///     Draw a rectangle.
        /// </summary>
        /// <param name="xLeft">Abscissa of the top left corner.</param>
        /// <param name="yTop">Ordinate of the top left corner.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default).</param>
        public void DrawRectangle(int xLeft, int yTop, int width, int height, Color color, bool filled = false)
        {
            width--;
            height--;
            if (filled)
            {
                for (var i = 0; i <= height; i++)
                {
                    DrawLine(xLeft, yTop + i, xLeft + width, yTop + i, color);
                }
            }
            else
            {
                DrawLine(xLeft, yTop, xLeft + width, yTop, color);
                DrawLine(xLeft + width, yTop, xLeft + width, yTop + height, color);
                DrawLine(xLeft + width, yTop + height, xLeft, yTop + height, color);
                DrawLine(xLeft, yTop, xLeft, yTop + height, color);
            }
        }

        /// <summary>
        ///     Draw a text message on the display using the current font.
        /// </summary>
        /// <param name="x">Abscissa of the location of the text.</param>
        /// <param name="y">Ordinate of the location of the text.</param>
        /// <param name="text">Text to display.</param>
        public void DrawText(int x, int y, string text)
        {
            if (CurrentFont == null)
                throw new Exception("CurrentFont must be set before calling DrawText.");

            byte[] bitMap = GetBytesForTextBitmap(text);

            DrawBitmap(x, y, bitMap.Length / CurrentFont.Height, CurrentFont.Height, bitMap, DisplayBase.BitmapMode.And);
        }

        /// <summary>
        ///     Draw a text message on the display using the current font.
        /// </summary>
        /// <param name="x">Abscissa of the location of the text.</param>
        /// <param name="y">Ordinate of the location of the text.</param>
        /// <param name="text">Text to display.</param>
        /// <param name="color">Color of the text.</param>
        public void DrawText(int x, int y, string text, Color color)
        {
            if (CurrentFont == null)
            {
                throw new Exception("CurrentFont must be set before calling DrawText.");
            }

            byte[] bitMap = GetBytesForTextBitmap(text);
            
            DrawBitmap(x, y, bitMap.Length / CurrentFont.Height, CurrentFont.Height, bitMap, color);
        }

        private byte[] GetBytesForTextBitmap(string text)
        {
            byte[] bitMap;

            if (CurrentFont.Width == 8) //just copy bytes
            {
                bitMap = new byte[text.Length * CurrentFont.Height * CurrentFont.Width / 8];

                byte[] characterMap;

                for (int i = 0; i < text.Length; i++)
                {
                    characterMap = CurrentFont[text[i]];

                    //copy data for 1 character at a time going top to bottom
                    for (int segment = 0; segment < CurrentFont.Height; segment++)
                    {
                        bitMap[i + (segment * text.Length)] = characterMap[segment];
                    }
                }
            }
            else if (CurrentFont.Width == 12)
            {
                var len = (text.Length + text.Length % 2) * 3 / 2;
                bitMap = new byte[len * CurrentFont.Height];

                byte[] charMap1, charMap2;
                int index = 0;

                for (int i = 0; i < text.Length; i += 2) //2 chracters, 3 bytes ... 24 bytes total so the math is good
                {
                    //grab two characters at once
                    charMap1 = CurrentFont[text[i]];
                    charMap2 = (i + 1 < text.Length) ? CurrentFont[text[i + 1]] : CurrentFont[' '];
                    
                    int cIndex = 0;
                    for (int j = 0; j < CurrentFont.Height; j += 2)
                    {
                        //first row - spans 3 bytes (for 2 chars)
                        bitMap[index + (j + 0) * len + 0] = charMap1[cIndex]; //good
                        bitMap[index + (j + 0) * len + 1] = (byte)((charMap1[cIndex + 1] & 0x0F) | (charMap2[cIndex] << 4)); //bad?
                        bitMap[index + (j + 0) * len + 2] = (byte)((charMap2[cIndex] >> 4) | (charMap2[cIndex + 1] << 4)); //good

                        //2nd row
                        bitMap[index + (j + 1) * len + 0] = (byte)((charMap1[cIndex + 1] >> 4) | charMap1[cIndex + 2] << 4); //good
                        bitMap[index + (j + 1) * len + 1] = (byte)((charMap1[cIndex + 2] >> 4) | charMap2[cIndex + 1] & 0xF0); //bad?
                        bitMap[index + (j + 1) * len + 2] = (byte)((charMap2[cIndex + 2])); //good

                        cIndex += 3;
                    }
                    index += 3;
                }
            }
            else if (CurrentFont.Width == 4)
            {
                var len = (text.Length + text.Length % 2) / 2;
                bitMap = new byte[len * CurrentFont.Height];
                byte[] charMap1, charMap2;

                for (int i = 0; i < len; i++)
                {
                    //grab two characters at once to fill a complete byte
                    charMap1 = CurrentFont[text[2 * i]];
                    charMap2 = (2 * i + 1 < text.Length) ? CurrentFont[text[2 * i + 1]] : CurrentFont[' '];

                    for (int j = 0; j < charMap1.Length; j++)
                    {
                        bitMap[i + (j * 2 + 0) * len] = (byte)((charMap1[j] & 0x0F) | (charMap2[j] << 4));
                        bitMap[i + (j * 2 + 1) * len] = (byte)((charMap1[j] >> 4) | (charMap2[j] & 0xF0));
                    }
                }
            }
            else
            {
                throw new Exception("Font width must be 4, 6, 8, or 12");
            }
            return bitMap;
        }

        byte SetBit(byte value, int position, bool high)
        {
            var compare = (byte)(1 << position);
            return high ? (value |= compare) : (byte)(value & ~compare);
        }

        #endregion Methods

        #region Display

        /// <summary>
        ///     Show the changes on the display.
        /// </summary>
        public void Show()
        {
            _display.Show();
        }

        /// <summary>
        ///     Clear the display.
        /// </summary>
        /// <param name="updateDisplay">Update the display immediately when true.</param>
        public void Clear(bool updateDisplay = false)
        {
            _display.Clear(updateDisplay);
        }

        /// <summary>
        ///     Display a 1-bit bitmap
        /// 
        ///     This method simply calls a similar method in the display hardware.
        /// </summary>
        /// <param name="x">Abscissa of the top left corner of the bitmap.</param>
        /// <param name="y">Ordinate of the top left corner of the bitmap.</param>
        /// <param name="width">Width of the bitmap in bytes.</param>
        /// <param name="height">Height of the bitmap in bytes.</param>
        /// <param name="bitmap">Bitmap to display.</param>
        /// <param name="bitmapMode">How should the bitmap be transferred to the display?</param>
        public void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, DisplayBase.BitmapMode bitmapMode)
        {
            if ((width * height) != bitmap.Length)
            {
                throw new ArgumentException("Width and height do not match the bitmap size.");
            }

            for (var ordinate = 0; ordinate < height; ordinate++)
            {
                for (var abscissa = 0; abscissa < width; abscissa++)
                {
                    var b = bitmap[(ordinate * width) + abscissa];
                    byte mask = 0x01;

                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        if ((b & mask) > 0)
                            DrawPixel(x + (8 * abscissa) + pixel, y + ordinate);
                        mask <<= 1;
                    }
                }
            }
        }

        /// <summary>
        ///     Display a 1-bit bitmap
        /// 
        ///     This method simply calls a similar method in the display hardware.
        /// </summary>
        /// <param name="x">Abscissa of the top left corner of the bitmap.</param>
        /// <param name="y">Ordinate of the top left corner of the bitmap.</param>
        /// <param name="width">Width of the bitmap in bytes.</param>
        /// <param name="height">Height of the bitmap in bytes.</param>
        /// <param name="bitmap">Bitmap to display.</param>
        /// <param name="color">The color of the bitmap.</param>
        public void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, Color color)
        {
            if ((width * height) != bitmap.Length)
            {
                throw new ArgumentException("Width and height do not match the bitmap size.");
            }

            _display.SetPenColor(color);

            for (var ordinate = 0; ordinate < height; ordinate++)
            {
                for (var abscissa = 0; abscissa < width; abscissa++)
                {
                    var b = bitmap[(ordinate * width) + abscissa];
                    byte mask = 0x01;

                    for (var pixel = 0; pixel < 8; pixel++)
                    {
                        if ((b & mask) > 0)
                        {
                            DrawPixel(x + (8 * abscissa) + pixel, y + ordinate);
                        }
                        mask <<= 1;
                    }
                }
            }
        }

        public int GetXForRotation(int x, int y)
        {
            switch(Rotation)
            {
                case RotationType._90Degrees:
                    return (int)_display.Width - y - 1;
                case RotationType._180Degrees:
                    return (int)_display.Width - x - 1;
                case RotationType._270Degrees:
                    return (int)y;
                case RotationType.Default:
                default:
                    return x;
            }
        }

        public int GetYForRotation(int x, int y)
        {
            switch (Rotation)
            {
                case RotationType._90Degrees:
                    return x; 
                case RotationType._180Degrees:
                    return (int)_display.Height - y - 1;
                case RotationType._270Degrees:
                    return (int)_display.Height - x - 1;
                case RotationType.Default:
                default:
                    return y;
            }
        }

        #endregion Display
    }
}