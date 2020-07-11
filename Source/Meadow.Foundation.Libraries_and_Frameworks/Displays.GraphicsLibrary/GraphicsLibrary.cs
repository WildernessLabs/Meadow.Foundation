using Meadow.Foundation.Displays;
using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    ///     Provide high level graphics functions
    /// </summary>
    public class GraphicsLibrary : ITextDisplay
    {
        #region Member variables / fields

        private readonly DisplayBase display;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Current font used for displaying text on the display.
        /// </summary>
        public FontBase CurrentFont
        {
            get => currentFont;
            set
            {
                currentFont = value;
                if(currentFont == null) { return; }
                DisplayConfig = new TextDisplayConfig()
                {
                    Width = (ushort)((int)this.Width / currentFont.Width),
                    Height = (ushort)((int)this.Height / CurrentFont.Height)
                };
            }
        }
        FontBase currentFont;

        /// <summary>
        /// Current rotation used for drawing pixels to the display
        /// </summary>
        public RotationType Rotation { get; set; } = RotationType.Default;

        /// <summary>
        /// Stroke / line thickness when drawing lines or shape outlines
        /// </summary>
        public int Stroke { get; set; } = 1;

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

        public enum ScaleFactor
        {
            X1 = 1,
            X2 = 2,
            X3 = 3,
        }

        /// <summary>
        /// Return the height of the display after accounting for the rotation.
        /// </summary>
        public uint Height =>  Rotation == RotationType.Default || Rotation == RotationType._180Degrees ? display.Height : display.Width;

        /// <summary>
        /// Return the width of the display after accounting for the rotation.
        /// </summary>
        public uint Width => Rotation == RotationType.Default || Rotation == RotationType._180Degrees ? display.Width : display.Height;

        public TextDisplayConfig DisplayConfig { get; private set; }

        #region Constructors

        /// <summary>
        /// </summary>
        /// <param name="display"></param>
        public GraphicsLibrary(DisplayBase display)
        {
            this.display = display;
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
            if (IsPixelInBounds(x, y) == false)
            {
                return;
            }
            display.DrawPixel(GetXForRotation(x, y), GetYForRotation(x, y));
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public void DrawPixel (int x, int y, bool colored)
        {
            if (IsPixelInBounds(x, y) == false)
            {
                return;
            }
            display.DrawPixel(GetXForRotation(x, y), GetYForRotation(x, y), colored);
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">Color of pixel.</param>
        public void DrawPixel (int x, int y, Color color)
        {
            if(IsPixelInBounds(x, y) == false)
            {
                return;
            }
            display.DrawPixel(GetXForRotation(x, y), GetYForRotation(x, y), color);
        }

        private bool IsPixelInBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
            {
                return false;
            }
            return true;
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
        public void DrawLine(int x0, int y0, int x1, int y1, bool colored)
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
            display.SetPenColor(color);

            if(Stroke == 1)
            {
                DrawLine(x0, y0, x1, y1);
                return;
            }

            if(IsTallerThanWide(x0, y0, x1, y1))
            {
                int xOffset = Stroke / 2;

                for(int i = 0; i < Stroke; i++)
                {
                    DrawLine(x0 - xOffset + i, y0, x1 - xOffset + i, y1);
                }
            }
            else
            {
                int yOffset = Stroke / 2;

                for (int i = 0; i < Stroke; i++)
                {
                    DrawLine(x0, y0 - yOffset + i, x1, y1 - yOffset + i);
                }
            }
        }

        private bool IsTallerThanWide(int x0, int y0, int x1, int y1)
        {
            return Math.Abs(x0 - x1) < Math.Abs(y0 - y1);
        }

        private void DrawLine(int x0, int y0, int x1, int y1)
        { 
            if(y0 == y1)
            {
                DrawHorizontalLine(x0, y0, x1 - x0);
                return;
            }

            if (x0 == x1)
            {
                DrawVerticalLine(x0, y0, y1 - y0);
                return;
            } 

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
        public void DrawHorizontalLine(int x0, int y0, int length, bool colored)
        {
            if(length < 0)
            {
                x0 += length;
                length *= -1;
            }

            for (var x = x0; (x - x0) <= length; x++)
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
            display.SetPenColor(color);
            DrawHorizontalLine(x0, y0, length);

        }
        private void DrawHorizontalLine(int x0, int y0, int length)
        {
            if (length < 0)
            {
                x0 += length;
                length *= -1;
            }

            for (var x = x0; (x - x0) <= length; x++)
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
        public void DrawVerticalLine(int x0, int y0, int length, bool colored)
        {
            if (length < 0)
            {
                y0 += length;
                length *= -1;
            }

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
            display.SetPenColor(color);

            DrawVerticalLine(x0, y0, length);
        }

        private void DrawVerticalLine(int x0, int y0, int length)
        {
            if (length < 0)
            {
                y0 += length;
                length *= -1;
            }

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
        public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, bool colored = true, bool filled = false)
        {
            if (filled)
            {
                DrawTriangleFilled(x0, y0, x1, y1, x2, y2, colored ? Color.White : Color.Black);
            }
            else
            {
                DrawTriangle(x0, y0, x1, y1, x2, y2, colored ? Color.White : Color.Black);
            }
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
                if (x1 < x) { x = x1; }
                else if (x1 > len) { len = x1; }
                if (x2 < x) { x = x2; }
                else if (x2 > len) { len = x2; }
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
                {
                    Swap(ref a, ref b);
                }
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

                if (a > b) { Swap(ref a, ref b); }
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
        public void DrawCircle(int centerX, int centerY, int radius, bool colored = true, bool filled = false, bool centerBetweenPixels = false)
        {
            DrawCircle(centerX, centerY, radius, (colored ? Color.White : Color.Black), filled, centerBetweenPixels);
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
        public void DrawCircle(int centerX, int centerY, int radius, Color color, bool filled = false, bool centerBetweenPixels = false)
        {
            display.SetPenColor(color);

            if (filled)
            {
                DrawCircleFilled(centerX, centerY, radius, centerBetweenPixels);
            }
            else
            {
                int offset = Stroke / 2;

                for (int i = 0; i < Stroke; i++)
                {
                    DrawCircleOutline(centerX, centerY, radius - offset + i, centerBetweenPixels);
                }
            }
        }

        private void DrawCircleOutline(int centerX, int centerY, int radius, bool centerBetweenPixels)
        {
            //I prefer the look of the original Bresenham’s decision param calculation
            var d = 3 - 2 * radius; // (5 - (radius * 4)) / 4;
            var x = 0;
            var y = radius;

            int offset = centerBetweenPixels ? 1 : 0;

            while (x <= y)
            {
                DrawPixel(centerX + x - offset, centerY + y - offset);
                DrawPixel(centerX + y - offset, centerY + x - offset);

                DrawPixel(centerX - y, centerY + x - offset);
                DrawPixel(centerX - x, centerY + y - offset);

                DrawPixel(centerX - x, centerY - y);
                DrawPixel(centerX - y, centerY - x);

                DrawPixel(centerX + x - offset, centerY - y);
                DrawPixel(centerX + y - offset, centerY - x);

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

        private void DrawCircleFilled(int centerX, int centerY, int radius, bool centerBetweenPixels)
        {
            var d = 3 - 2 * radius;
            var x = 0;
            var y = radius;

            int offset = centerBetweenPixels ? 1 : 0;

            while (x <= y)
            {
                DrawLine(centerX + x - offset, centerY + y - offset, centerX - x, centerY + y - offset);
                DrawLine(centerX + x - offset, centerY - y, centerX - x, centerY - y);
                DrawLine(centerX - y, centerY + x - offset, centerX + y - offset, centerY + x - offset);
                DrawLine(centerX - y, centerY - x, centerX + y - offset, centerY - x);

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
        public void DrawText(int x, int y, string text, ScaleFactor scaleFactor = ScaleFactor.X1)
        {
            if (CurrentFont == null)
            {
                throw new Exception("CurrentFont must be set before calling DrawText.");
            }

            byte[] bitMap = GetBytesForTextBitmap(text);

            DrawBitmap(x, y, bitMap.Length / CurrentFont.Height * 8, CurrentFont.Height, bitMap, DisplayBase.BitmapMode.And, scaleFactor);
        }

        /// <summary>
        ///     Draw a text message on the display using the current font.
        /// </summary>
        /// <param name="x">Abscissa of the location of the text.</param>
        /// <param name="y">Ordinate of the location of the text.</param>
        /// <param name="text">Text to display.</param>
        /// <param name="color">Color of the text.</param>
        public void DrawText(int x, int y, string text, Color color, ScaleFactor scaleFactor = ScaleFactor.X1)
        {
            if (CurrentFont == null)
            {
                throw new Exception("CurrentFont must be set before calling DrawText.");
            }

            byte[] bitmap = GetBytesForTextBitmap(text);
            
            DrawBitmap(x, y, bitmap.Length / CurrentFont.Height * 8, CurrentFont.Height, bitmap, color, scaleFactor);
        }

        private byte[] GetBytesForTextBitmap(string text)
        {
            byte[] bitmap;

            if (CurrentFont.Width == 8) //just copy bytes
            {
                bitmap = new byte[text.Length * CurrentFont.Height * CurrentFont.Width / 8];

                byte[] characterMap;

                for (int i = 0; i < text.Length; i++)
                {
                    characterMap = CurrentFont[text[i]];

                    //copy data for 1 character at a time going top to bottom
                    for (int segment = 0; segment < CurrentFont.Height; segment++)
                    {
                        bitmap[i + (segment * text.Length)] = characterMap[segment];
                    }
                }
            }
            else if (CurrentFont.Width == 12)
            {
                var len = (text.Length + text.Length % 2) * 3 / 2;
                bitmap = new byte[len * CurrentFont.Height];

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
                        bitmap[index + (j + 0) * len + 0] = charMap1[cIndex]; //good
                        bitmap[index + (j + 0) * len + 1] = (byte)((charMap1[cIndex + 1] & 0x0F) | (charMap2[cIndex] << 4)); //bad?
                        bitmap[index + (j + 0) * len + 2] = (byte)((charMap2[cIndex] >> 4) | (charMap2[cIndex + 1] << 4)); //good

                        //2nd row
                        bitmap[index + (j + 1) * len + 0] = (byte)((charMap1[cIndex + 1] >> 4) | charMap1[cIndex + 2] << 4); //good
                        bitmap[index + (j + 1) * len + 1] = (byte)((charMap1[cIndex + 2] >> 4) | charMap2[cIndex + 1] & 0xF0); //bad?
                        bitmap[index + (j + 1) * len + 2] = (byte)((charMap2[cIndex + 2])); //good

                        cIndex += 3;
                    }
                    index += 3;
                }
            }
            else if (CurrentFont.Width == 4)
            {
                var len = (text.Length + text.Length % 2) / 2;
                bitmap = new byte[len * CurrentFont.Height];
                byte[] charMap1, charMap2;

                for (int i = 0; i < len; i++)
                {
                    //grab two characters at once to fill a complete byte
                    charMap1 = CurrentFont[text[2 * i]];
                    charMap2 = (2 * i + 1 < text.Length) ? CurrentFont[text[2 * i + 1]] : CurrentFont[' '];

                    for (int j = 0; j < charMap1.Length; j++)
                    {
                        bitmap[i + (j * 2 + 0) * len] = (byte)((charMap1[j] & 0x0F) | (charMap2[j] << 4));
                        bitmap[i + (j * 2 + 1) * len] = (byte)((charMap1[j] >> 4) | (charMap2[j] & 0xF0));
                    }
                }
            }
            else
            {
                throw new Exception("Font width must be 4, 6, 8, or 12");
            }
            return bitmap;
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
            display.Show();
        }

        /// <summary>
        ///     Clear the display.
        /// </summary>
        /// <param name="updateDisplay">Update the display immediately when true.</param>
        public void Clear(bool updateDisplay = false)
        {
            display.Clear(updateDisplay);
        }

        /// <summary>
        ///     Display a 1-bit bitmap
        /// 
        ///     This method simply calls a similar method in the display hardware.
        /// </summary>
        /// <param name="x">Abscissa of the top left corner of the bitmap.</param>
        /// <param name="y">Ordinate of the top left corner of the bitmap.</param>
        /// <param name="width">Width of the bitmap in pixels.</param>
        /// <param name="height">Height of the bitmap in pixels.</param>
        /// <param name="bitmap">Bitmap to display.</param>
        /// <param name="bitmapMode">How should the bitmap be transferred to the display?</param>
        public void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, DisplayBase.BitmapMode bitmapMode, ScaleFactor scaleFactor = ScaleFactor.X1)
        {
            width /= 8;

            if ((width * height) != bitmap.Length)
            {
                throw new ArgumentException("Width and height do not match the bitmap size.");
            }

            int scale = (int)scaleFactor;

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
                            //not elegant but works for now
                       /*     if(scaleFactor == ScaleFactor.X2)
                            {
                                //hard code for 2x for now
                                DrawPixel(x + (8 * abscissa) * 2 + pixel * 2,     y + ordinate * 2);
                                DrawPixel(x + (8 * abscissa) * 2 + pixel * 2 + 1, y + ordinate * 2);
                                DrawPixel(x + (8 * abscissa) * 2 + pixel * 2,     y + ordinate * 2 + 1);
                                DrawPixel(x + (8 * abscissa) * 2 + pixel * 2 + 1, y + ordinate * 2 + 1);
                            } */
                            if (scaleFactor != ScaleFactor.X1)
                            {
                                for (int i = 0; i < scale; i++)
                                {
                                    for (int j = 0; j < scale; j++)
                                    {
                                        DrawPixel(x + (8 * abscissa) * scale + pixel * scale + i, y + ordinate * scale + j);
                                    }
                                }
                            }
                            else
                            {
                                //1x
                                DrawPixel(x + (8 * abscissa) + pixel, y + ordinate);
                            }
                        }
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
        /// <param name="width">Width of the bitmap in pixels.</param>
        /// <param name="height">Height of the bitmap in pixels.</param>
        /// <param name="bitmap">Bitmap to display.</param>
        /// <param name="color">The color of the bitmap.</param>
        public void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, Color color, ScaleFactor scaleFactor = ScaleFactor.X1)
        {
            display.SetPenColor(color);

            DrawBitmap(x, y, width, height, bitmap, DisplayBase.BitmapMode.Copy, scaleFactor);
        }

        public int GetXForRotation(int x, int y)
        {
            switch(Rotation)
            {
                case RotationType._90Degrees:
                    return (int)display.Width - y - 1;
                case RotationType._180Degrees:
                    return (int)display.Width - x - 1;
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
                    return (int)display.Height - y - 1;
                case RotationType._270Degrees:
                    return (int)display.Height - x - 1;
                case RotationType.Default:
                default:
                    return y;
            }
        }

        public void Write(string text)
        {
            if (CurrentFont == null)
            {
                throw new Exception("GraphicsLibrary.Write requires CurrentFont to be set");
            }
            DrawText(CurrentFont.Width * CursorColumn, CurrentFont.Height * CursorLine, text);
            Show();
        }

        public void WriteLine(string text, byte lineNumber)
        {
            if(CurrentFont == null)
            {
                throw new Exception("GraphicsLibrary.WriteLine requires CurrentFont to be set");
            }
            DrawText(0, lineNumber * CurrentFont.Height, text);
            Show();
        }

        public void ClearLines()
        {
            Clear(true); //for now
        }

        public void ClearLine(byte lineNumber)
        {
            DrawRectangle(0, CurrentFont.Height * lineNumber, (int)Width, CurrentFont.Height, true, true);
        }

        public byte CursorColumn { get; private set; } = 0;
        public byte CursorLine { get; private set; } = 0;
        public void SetCursorPosition(byte column, byte line)
        {
            CursorColumn = column;
            CursorLine = line;
        }

        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
          //  throw new NotImplementedException();
        }

        #endregion Display
    }
}