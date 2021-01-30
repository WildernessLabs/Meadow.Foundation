using Meadow.Foundation.Displays;
using Meadow.Peripherals.Displays;
using System;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    ///     Provide high level graphics functions
    /// </summary>
    public partial class GraphicsLibrary 
    {
        protected class CanvasState
        {
            public FontBase CurrentFont { get; set; }
            public int Stroke { get; set; }
            public RotationType Rotation { get; set; }
        }

        private readonly DisplayBase display;

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
                    Width = (ushort)(Width / currentFont.Width),
                    Height = (ushort)(Height / CurrentFont.Height)
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

        public enum ScaleFactor : int
        {
            X1 = 1,
            X2 = 2,
            X3 = 3,
            X4 = 4,
        }

        public enum TextAlignment
        {
            Left,
            Center,
            Right
        }

        /// <summary>
        /// Return the height of the display after accounting for the rotation.
        /// </summary>
        public int Height => Rotation == RotationType.Default || Rotation == RotationType._180Degrees ? display.Height : display.Width;

        /// <summary>
        /// Return the width of the display after accounting for the rotation.
        /// </summary>
        public int Width => Rotation == RotationType.Default || Rotation == RotationType._180Degrees ? display.Width : display.Height;

        public TextDisplayConfig DisplayConfig { get; private set; }

        protected CanvasState canvasState;

        /// <summary>
        /// </summary>
        /// <param name="display"></param>
        public GraphicsLibrary(DisplayBase display)
        {
            this.display = display;
            CurrentFont = null;
        }

        /// <summary>
        /// Save any state variables
        /// Includes: CurrentFont, Stroke, & Rotation
        /// </summary>
        public void SaveState()
        {
            if (canvasState == null)
            {
                canvasState = new CanvasState();
            }

            canvasState.CurrentFont = currentFont;
            canvasState.Stroke = Stroke;
            canvasState.Rotation = Rotation;
        }

        /// <summary>
        /// Restore saved state variables and apply them to the GraphicsLibrary instance 
        /// Includes: CurrentFont, Stroke, & Rotation
        /// </summary>
        public void RestoreState()
        {
            if (canvasState == null)
            {
                throw new NullReferenceException("GraphicsLibary: State not saved, no state to restore.");
            }

            currentFont = canvasState.CurrentFont;
            Stroke = canvasState.Stroke;
            Rotation = canvasState.Rotation;
        }

        /// <summary>
        ///     Draw a single pixel using the pen color
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        public void DrawPixel(int x, int y)
        {
            display.DrawPixel(GetXForRotation(x, y), GetYForRotation(x, y));
        }

        /// <summary>
        ///     Draw a single pixel using the pen color
        /// </summary>
        /// <param name="index">pixel location in buffer</param>
        public void DrawPixel(int index)
        {   //need to move this to the display driver TODO
            display.DrawPixel((int)(index % display.Width), (int)(index / display.Width));
        }

        /// <summary>
        ///     Invert the color of the pixel at the given location
        /// </summary>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        public void InvertPixel(int x, int y)
        {
            display.InvertPixel(GetXForRotation(x, y), GetYForRotation(x, y));
        }

        /// <summary>
        ///     Invert all pixels within a rectangle 
        /// </summary>
        /// <param name="x">x start</param>
        /// <param name="y">y start</param>
        /// /// <param name="width">width of area to invert</param>
        /// <param name="height">height of area to invert</param>
        public void InvertRectangle(int x, int y, int width, int height)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    InvertPixel(i + x, j + y);
                }
            }
        }

        /// <summary>
        ///     Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public void DrawPixel (int x, int y, bool colored)
        {
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
        ///     Draw a line using polar coordinates
        /// </summary>
        /// <remarks>
        /// <param name="x">Abscissa of the starting point of the line</param>
        /// <param name="y">Ordinate of the starting point of the line</param>
        /// <param name="length">Length of line.</param>
        /// <param name="angle">Angle in radians</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public void DrawLine(int x, int y, int length, float angle, bool colored)
        {
            DrawLine(x, y, length, angle, (colored ? Color.White : Color.Black));
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

            if (Stroke == 1)
            {
                DrawLine(x0, y0, x1, y1);
                return;
            }

            if (IsTallerThanWide(x0, y0, x1, y1))
            {
                int xOffset = Stroke >> 1;

                for (int i = 0; i < Stroke; i++)
                {
                    DrawLine(x0 - xOffset + i, y0, x1 - xOffset + i, y1);
                }
            }
            else
            {
                int yOffset = Stroke >> 1;

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

        /// <summary>
        ///     Draw a line from a point to a position defined by a radius and an angle
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y">Ordinate of the starting point of the line</param>
        /// <param name="length">length of line</param>
        /// <param name="angle">angle to draw line in radians</param>
        /// <param name="color">The color of the line</param>
        public void DrawLine(int x, int y, int length, float angle, Color color)
        {
            int x1 = (int)(length * MathF.Cos(angle)) + x;
            int y1 = y - (int)(length * MathF.Sin(angle));

            DrawLine(x, y, x1, y1, color);
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
                Swap(ref x0, ref y0);
                Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Swap(ref x0, ref x1);
                Swap(ref y0, ref y1);
            }
            var dx = x1 - x0;
            var dy = Math.Abs(y1 - y0);
            var error = dx >> 1;
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
        /// <param name="x">Abscissa of the starting point of the line.</param>
        /// <param name="y">Ordinate of the starting point of the line.</param>
        /// <param name="length">Length of the line to draw.</param>
        /// <param name="colored">Turn the pixel on (true) or off (false).</param>
        public void DrawHorizontalLine(int x, int y, int length, bool colored)
        {
            DrawHorizontalLine(x, y, length, (colored ? Color.White : Color.Black));
        }

        /// <summary>
        ///     Draw a horizontal line.
        /// </summary>
        /// <param name="x">Abscissa of the starting point of the line.</param>
        /// <param name="y">Ordinate of the starting point of the line.</param>
        /// <param name="length">Length of the line to draw.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawHorizontalLine(int x, int y, int length, Color color)
        {
            display.SetPenColor(color);

            if (Stroke == 1)
            {
                DrawHorizontalLine(x, y, length);
            }
            else
            {
                int yOffset = Stroke >> 1;

                for (int i = 0; i < Stroke; i++)
                {
                    DrawHorizontalLine(x, y - yOffset + i, length);
                }
            } 
        }

        private void DrawHorizontalLine(int x, int y, int length)
        {
            if (length < 0)
            {
                x += length;
                length *= -1;
            }

            for (var i = x; (i - x) <= length; i++)
            {
                DrawPixel(i, y);
            }
        }

        /// <summary>
        ///     Draw a vertical line.
        /// </summary>
        /// <param name="x">Abscissa of the starting point of the line.</param>
        /// <param name="y">Ordinate of the starting point of the line.</param>
        /// <param name="length">Length of the line to draw.</param>
        /// <param name="colored">Show the line when (true) or off (false).</param>
        public void DrawVerticalLine(int x, int y, int length, bool colored)
        {
            DrawVerticalLine(x, y, length, (colored ? Color.White : Color.Black));
        }

        /// <summary>
        ///     Draw a vertical line.
        /// </summary>
        /// <param name="x">Abscissa of the starting point of the line.</param>
        /// <param name="y">Ordinate of the starting point of the line.</param>
        /// <param name="length">Length of the line to draw.</param>
        /// <param name="color">The color of the line.</param>
        public void DrawVerticalLine(int x, int y, int length, Color color)
        {
            display.SetPenColor(color);

            if (Stroke == 1)
            {
                DrawVerticalLine(x, y, length);
            }
            else
            {
                int xOffset = Stroke >> 1;

                for (int i = 0; i < Stroke; i++)
                {
                    DrawVerticalLine(x - xOffset + i, y, length);
                }
            }
        }

        private void DrawVerticalLine(int x, int y, int length)
        {
            if (length < 0)
            {
                y += length;
                length *= -1;
            }

            for (var i = y; (i - y) < length; i++)
            {
                DrawPixel(x, i);
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
                int offset = Stroke >> 1;

                for (int i = 0; i < Stroke; i++)
                {
                    DrawCircleOutline(centerX, centerY, radius - offset + i, centerBetweenPixels);
                }
            }
        }

        public void DrawCircleQuadrant(int centerX, int centerY, int radius, int quadrant, bool colored = true, bool filled = false, bool centerBetweenPixels = false)
        {
            DrawCircleQuadrant(centerX, centerY, radius, quadrant, (colored ? Color.White : Color.Black), filled, centerBetweenPixels);
        }

        public void DrawCircleQuadrant(int centerX, int centerY, int radius, int quadrant, Color color, bool filled = false, bool centerBetweenPixels = false)
        {
            if (quadrant < 0 || quadrant > 3) { throw new ArgumentOutOfRangeException("DrawCircleQuadrant: quadrant must be between 0 & 3 inclusive"); }

            display.SetPenColor(color);

            if (filled)
            {
                DrawCircleQuadrantFilled(centerX, centerY, radius, quadrant, centerBetweenPixels);
            }
            else
            {
                int offset = Stroke >> 1;

                for (int i = 0; i < Stroke; i++)
                {
                    DrawCircleQuadrantOutline(centerX, centerY, radius - offset + i, quadrant, centerBetweenPixels);
                }
            }
        }

        private void DrawCircleQuadrantFilled(int centerX, int centerY, int radius, int quadrant, bool centerBetweenPixels = false)
        {
            var d = 3 - 2 * radius;
            var x = 0;
            var y = radius;

            int offset = centerBetweenPixels ? 1 : 0;

            while (x <= y)
            {
                switch (quadrant)
                {
                    case 3:
                        DrawLine(centerX + x - offset, centerY + y - offset, centerX - offset, centerY + y - offset);
                        DrawLine(centerX + y - offset, centerY + x - offset, centerX - offset, centerY + x - offset);
                        break;
                    case 2:
                        DrawLine(centerX - y, centerY + x - offset, centerX, centerY + x - offset);
                        DrawLine(centerX - x, centerY + y - offset, centerX, centerY + y - offset);
                        break;
                    case 1:
                        DrawLine(centerX - x, centerY - y, centerX, centerY - y);
                        DrawLine(centerX - y, centerY - x, centerX, centerY - x);
                        break;
                    case 0:
                        DrawLine(centerX + x - offset, centerY - y, centerX - offset, centerY - y);
                        DrawLine(centerX + y - offset, centerY - x, centerX - offset, centerY - x);
                        break;
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

        private void DrawCircleQuadrantOutline(int centerX, int centerY, int radius, int quadrant, bool centerBetweenPixels = false)
        {
            var d = 3 - 2 * radius; // (5 - (radius * 4)) / 4;
            var x = 0;
            var y = radius;

            int offset = centerBetweenPixels ? 1 : 0;

            while (x <= y)
            {
                switch(quadrant)
                {
                    case 3:
                        DrawPixel(centerX + x - offset, centerY + y - offset);
                        DrawPixel(centerX + y - offset, centerY + x - offset);
                        break;
                    case 2:
                        DrawPixel(centerX - y, centerY + x - offset);
                        DrawPixel(centerX - x, centerY + y - offset);
                        break;
                    case 1:
                        DrawPixel(centerX - x, centerY - y);
                        DrawPixel(centerX - y, centerY - x);
                        break;
                    case 0:
                        DrawPixel(centerX + x - offset, centerY - y);
                        DrawPixel(centerX + y - offset, centerY - x);
                        break;
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
        /// <param name="x">Abscissa of the top left corner.</param>
        /// <param name="y">Ordinate of the top left corner.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="colored">Draw the pixel (true) or turn the pixel off (false).</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default).</param>
        public void DrawRectangle(int x, int y, int width, int height, bool colored = true, bool filled = false)
        {
            width--;
            height--;

            if (filled)
            {
                for (var i = 0; i <= height; i++)
                {
                    DrawLine(x, y + i, x + width, y + i, colored);
                }
            }
            else
            {
                DrawLine(x, y, x + width, y, colored);
                DrawLine(x + width, y, x + width, y + height, colored);
                DrawLine(x + width, y + height, x, y + height, colored);
                DrawLine(x, y, x, y + height, colored);
            }
        }

        /// <summary>
        ///     Draw a rectangle.
        /// </summary>
        /// <param name="x">Abscissa of the top left corner.</param>
        /// <param name="y">Ordinate of the top left corner.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default).</param>
        public void DrawRectangle(int x, int y, int width, int height, Color color, bool filled = false)
        {
            width--;
            height--;

            if (filled)
            {
                for (var i = 0; i <= height; i++)
                {
                    DrawLine(x, y + i, x + width, y + i, color);
                }
            }
            else
            {
                DrawLine(x, y, x + width, y, color);
                DrawLine(x + width, y, x + width, y + height, color);
                DrawLine(x + width, y + height, x, y + height, color);
                DrawLine(x, y, x, y + height, color);
            }
        }

        /// <summary>
        ///     Draw a rounded rectangle.
        /// </summary>
        /// <param name="x">Abscissa of the top left corner.</param>
        /// <param name="y">Ordinate of the top left corner.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="cornerRadius">Radius of the corners of the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default).</param>
        public void DrawRoundedRectangle(int x, int y, int width, int height, int cornerRadius, Color color, bool filled = false)
        {
            if(cornerRadius < 0) { throw new ArgumentOutOfRangeException("Radius must be positive"); }

            if(cornerRadius == 0)
            {
                DrawRectangle(x, y, width, height, color, filled);
                return;
            }

            display.SetPenColor(color);

            if (filled)
            {
                DrawCircleQuadrant(x + width - cornerRadius - 1, y + cornerRadius, cornerRadius, 0, color, true);
                DrawCircleQuadrant(x + cornerRadius, y + cornerRadius, cornerRadius, 1, color, true);

                DrawCircleQuadrant(x + cornerRadius, y + height - cornerRadius - 1, cornerRadius, 2, color, true);
                DrawCircleQuadrant(x + width - cornerRadius - 1, y + height - cornerRadius - 1, cornerRadius, 3, color, true);

                DrawRectangle(x, y + cornerRadius, width, height - 2 * cornerRadius, color, filled);
                DrawRectangle(x + cornerRadius, y, width - 2 * cornerRadius, height, color, filled);
            }
            else
            {
                //corners
                DrawCircleQuadrant(x + width - cornerRadius - 1, y + cornerRadius, cornerRadius, 0, color, false);
                DrawCircleQuadrant(x + cornerRadius, y + cornerRadius, cornerRadius, 1, color, false);

                DrawCircleQuadrant(x + cornerRadius, y + height - cornerRadius - 1, cornerRadius, 2, color, false);
                DrawCircleQuadrant(x + width - cornerRadius - 1, y + height - cornerRadius - 1, cornerRadius, 3, color, false);

                //lines
                DrawLine(x + cornerRadius, y - 1, x + width - cornerRadius, y - 1);
                DrawLine(x + cornerRadius, y + height, x + width - cornerRadius, y + height);

                DrawLine(x, y + cornerRadius, x, y + height - cornerRadius);
                DrawLine(x + width - 1, y + cornerRadius, x + width - 1, y + height - cornerRadius);
            }
        }

        /// <summary>
        ///     Draw a text message on the display using the current font.
        /// </summary>
        /// <param name="x">Abscissa of the location of the text.</param>
        /// <param name="y">Ordinate of the location of the text.</param>
        /// <param name="text">Text to display.</param>
        public void DrawText(int x, int y, string text,
            ScaleFactor scaleFactor = ScaleFactor.X1, TextAlignment alignment = TextAlignment.Left)
        {
            if (CurrentFont == null)
            {
                throw new Exception("CurrentFont must be set before calling DrawText.");
            }

            byte[] bitMap = GetBytesForTextBitmap(text);

            if(alignment == TextAlignment.Center)
            {
                x = x - text.Length * ((int)scaleFactor * CurrentFont.Width >> 1);
            }
            else if(alignment == TextAlignment.Right)
            {
                x = x - text.Length * (int)scaleFactor * CurrentFont.Width;
            }

            DrawBitmap(x, y, bitMap.Length / CurrentFont.Height * 8, CurrentFont.Height, bitMap, DisplayBase.BitmapMode.And, scaleFactor);
        }

        /// <summary>
        ///     Draw a text message on the display using the current font.
        /// </summary>
        /// <param name="x">Abscissa of the location of the text.</param>
        /// <param name="y">Ordinate of the location of the text.</param>
        /// <param name="text">Text to display.</param>
        /// <param name="color">Color of the text.</param>
        public void DrawText(int x, int y, string text, Color color,
            ScaleFactor scaleFactor = ScaleFactor.X1, TextAlignment alignment = TextAlignment.Left)
        {
            if (CurrentFont == null)
            {
                throw new Exception("CurrentFont must be set before calling DrawText.");
            }

            if (alignment == TextAlignment.Center)
            {
                x = x - text.Length * ((int)scaleFactor * CurrentFont.Width >> 1);
            }
            else if (alignment == TextAlignment.Right)
            {
                x = x - text.Length * (int)scaleFactor * CurrentFont.Width;
            }

            byte[] bitmap = GetBytesForTextBitmap(text);
            
            DrawBitmap(x, y, bitmap.Length / CurrentFont.Height * 8, CurrentFont.Height, bitmap, color, scaleFactor);
        }

        private byte[] GetBytesForTextBitmap(string text)
        {
            byte[] bitmap;

            if (CurrentFont.Width == 8) //just copy bytes
            {
                bitmap = new byte[text.Length * CurrentFont.Height * (CurrentFont.Width >> 3)];

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
                var len = ((text.Length + text.Length % 2) * 3) >> 1;
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
                var len = (text.Length + text.Length % 2) >> 1;
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
        ///     Clear the display to a color
        /// </summary>
        /// <param name="updateDisplay">Update the display immediately when true.</param>
        /// <param name="color">Color to set display.</param>
        public void Clear(Color color, bool updateDisplay = false)
        {
            DrawRectangle(0, 0, (int)Width, (int)Height, color);

            if(updateDisplay) { Show(); }
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
                            {   //1x
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
    }
}