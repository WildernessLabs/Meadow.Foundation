using Meadow.Foundation.Graphics.Buffers;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Provide high level graphics functions
    /// </summary>
    public partial class MicroGraphics
    {
        /// <summary>
        /// Display object responsible for managing the buffer and rendering
        /// </summary>
        protected readonly IPixelDisplay? display;

        /// <summary>
        /// PixelBuffer draw target
        /// </summary>
        protected IPixelBuffer PixelBuffer => (display != null) ? display.PixelBuffer : memoryBuffer;
        private readonly IPixelBuffer memoryBuffer = default!;

        /// <summary>
        /// ignore pixels that are outside of the pixel buffer coordinate space
        /// </summary>
        public bool IgnoreOutOfBoundsPixels = true;

        /// <summary>
        /// The color used when a pixel is enabled (on)
        /// </summary>
        public Color EnabledColor => display != null ? display.EnabledColor : Color.White;

        /// <summary>
        /// The color used when a pixel is not enabled (off)
        /// </summary>
        public Color DisabledColor => display != null ? display.EnabledColor : Color.Black;

        /// <summary>
        /// Font used for drawing text to the display
        /// </summary>
        public IFont CurrentFont
        {
            get
            {
                currentFont ??= new Font6x8();
                return currentFont;
            }
            set
            {
                currentFont = value;
                if (currentFont == null) { return; }

                DisplayConfig.Width = (ushort)(Width / currentFont.Width);
                DisplayConfig.Height = (ushort)(Height / CurrentFont.Height);
            }
        }

        private IFont? currentFont = null;

        /// <summary>
        /// Current color mode
        /// </summary>
        public ColorMode ColorMode => PixelBuffer.ColorMode;

        /// <summary>
        /// Current rotation used for drawing pixels to the display
        /// </summary>
        public RotationType Rotation
        {
            get
            {
                if (display is IRotatableDisplay { } d) { return d.Rotation; }
                return _rotation;
            }
            set
            {
                if (display is IRotatableDisplay { } d) { d.SetRotation(value); }
                else { _rotation = value; }
            }
        }

        private RotationType _rotation = RotationType.Default;

        /// <summary>
        /// Stroke / line thickness when drawing lines or shape outlines
        /// </summary>
        public int Stroke { get; set; } = 1;

        /// <summary>
        /// Current pen color 
        /// </summary>
        public Color PenColor { get; set; } = Color.White;

        /// <summary>
        /// Return the height of the display after accounting for the rotation
        /// </summary>
        public int Height
        {
            get
            {
                if (display is IRotatableDisplay)
                {
                    return display.Height;
                }
                return Rotation == RotationType.Default || Rotation == RotationType._180Degrees ? PixelBuffer.Height : PixelBuffer.Width;
            }
        }

        /// <summary>
        /// Return the width of the display after accounting for the rotation
        /// </summary>
        public int Width
        {
            get
            {
                if (display is IRotatableDisplay)
                {
                    return display.Width;
                }
                return Rotation == RotationType.Default || Rotation == RotationType._180Degrees ? PixelBuffer.Width : PixelBuffer.Height;
            }
        }

        /// <summary>
        /// Text display configuration for use with text display menu
        /// </summary>
        public TextDisplayConfig DisplayConfig { get; private set; } = new TextDisplayConfig();

        /// <summary>
        /// Optional enforced delay between updates when calling ShowBuffered
        /// </summary>
        public TimeSpan DelayBetweenFrames { get; set; } = TimeSpan.Zero;

        private readonly object _lock = new();
        private bool isUpdating = false;
        private bool isUpdateRequested = false;

        /// <summary>
        /// Time of last display update when calling ShowBuffered
        /// </summary>
        private DateTime lastUpdated;

        /// <summary>
        /// Create a new MicroGraphics instance from a display peripheral driver instance
        /// </summary>
        /// <param name="display">An IPixelDisplay object</param>
        public MicroGraphics(IPixelDisplay display)
        {
            this.display = display;
        }

        /// <summary>
        /// Create a new MicroGraphics instance from a pixel buffer instance
        /// </summary>
        /// <param name="pixelBuffer">The pixel buffer</param>
        /// <param name="initializeBuffer">Initialize the off-screen buffer if true</param>
        public MicroGraphics(IPixelBuffer pixelBuffer, bool initializeBuffer)
        {
            memoryBuffer = pixelBuffer;

            if (initializeBuffer && pixelBuffer is PixelBufferBase buf)
            {
                buf.InitializeBuffer();
            }
        }

        /// <summary>
        /// Draw a single pixel using the pen color
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        /// <param name="color">Color of pixel</param>
        public virtual void DrawPixel(int x, int y, Color color)
        {
            if (IgnoreOutOfBoundsPixels && IsCoordinateInBounds(x, y) == false)
            {
                return;
            }

            if (display is IRotatableDisplay)
            {
                PixelBuffer.SetPixel(x, y, color);
            }
            else
            {
                PixelBuffer.SetPixel(GetXForRotation(x, y), GetYForRotation(x, y), color);
            }
        }

        /// <summary>
        /// Draw a single pixel 
        /// </summary>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        /// <param name="enabled">Turn the pixel on (true) or off (false)</param>
        public void DrawPixel(int x, int y, bool enabled)
        {
            DrawPixel(x, y, enabled ? EnabledColor : DisabledColor);
        }

        /// <summary>
        /// Draw a single pixel 
        /// </summary>
        /// <param name="x">x location </param>
        /// <param name="y">y location</param>
        public virtual void DrawPixel(int x, int y)
        {
            DrawPixel(x, y, PenColor);
        }

        /// <summary>
        /// Draw a single pixel using the pen color
        /// </summary>
        /// <param name="index">pixel location in buffer</param>
        public virtual void DrawPixel(int index)
        {
            if (IgnoreOutOfBoundsPixels && (index < 0 || index >= Width * Height))
            {
                return;
            }

            PixelBuffer.SetPixel(index % PixelBuffer.Width, index / PixelBuffer.Width, PenColor);
        }

        /// <summary>
        /// Invert the color of the pixel at the given location
        /// </summary>
        /// <param name="x">x location</param>
        /// <param name="y">y location</param>
        public void InvertPixel(int x, int y)
        {
            if (IgnoreOutOfBoundsPixels && IsCoordinateInBounds(x, y) == false)
            {
                return;
            }

            PixelBuffer.InvertPixel(GetXForRotation(x, y), GetYForRotation(x, y));
        }

        /// <summary>
        /// Invert all pixels within a rectangle 
        /// </summary>
        /// <param name="x">x start</param>
        /// <param name="y">y start</param>
        /// /// <param name="width">width of area to invert</param>
        /// <param name="height">height of area to invert</param>
        public virtual void InvertRectangle(int x, int y, int width, int height)
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
        /// Draw a line using Bresenhams line drawing algorithm
        /// </summary>
        /// <param name="x0">Abscissa of the starting point of the line</param>
        /// <param name="y0">Ordinate of the starting point of the line</param>
        /// <param name="x1">Abscissa of the end point of the line</param>
        /// <param name="y1">Ordinate of the end point of the line</param>
        /// <param name="enabled">Turn the pixel on (true) or off (false)</param>
        public void DrawLine(int x0, int y0, int x1, int y1, bool enabled)
        {
            DrawLine(x0, y0, x1, y1, enabled ? EnabledColor : DisabledColor);
        }

        /// <summary>
        /// Draw a line using polar coordinates
        /// </summary>
        /// <param name="x">Abscissa of the starting point of the line</param>
        /// <param name="y">Ordinate of the starting point of the line</param>
        /// <param name="length">Length of line</param>
        /// <param name="angle">Angle in radians</param>
        /// <param name="enabled">Turn the pixel on (true) or off (false)</param>
        public void DrawLine(int x, int y, int length, float angle, bool enabled)
        {
            DrawLine(x, y, length, angle, enabled ? EnabledColor : DisabledColor);
        }

        /// <summary>
        /// Draw a line using Bresenhams line drawing algorithm
        /// </summary>
        /// <remarks>
        /// Bresenhams line drawing algorithm:
        /// https://en.wikipedia.org/wiki/Bresenham's_line_algorithm
        /// </remarks>
        /// <param name="x0">Abscissa of the starting point of the line</param>
        /// <param name="y0">Ordinate of the starting point of the line</param>
        /// <param name="x1">Abscissa of the end point of the line</param>
        /// <param name="y1">Ordinate of the end point of the line</param>
        public void DrawLine(int x0, int y0, int x1, int y1)
        {
            DrawLine(x0, y0, x1, y1, PenColor);
        }

        private bool IsTallerThanWide(int x0, int y0, int x1, int y1)
        {
            return Math.Abs(x0 - x1) < Math.Abs(y0 - y1);
        }

        /// <summary>
        /// Draw a line from a point to a position defined by a radius and an angle
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

        /// <summary>
        /// Draw a line using Bresenhams line drawing algorithm
        /// </summary>
        /// <remarks>
        /// Bresenhams line drawing algorithm:
        /// https://en.wikipedia.org/wiki/Bresenham's_line_algorithm
        /// </remarks>
        /// <param name="x0">Abscissa of the starting point of the line</param>
        /// <param name="y0">Ordinate of the starting point of the line</param>
        /// <param name="x1">Abscissa of the end point of the line</param>
        /// <param name="y1">Ordinate of the end point of the line</param>
        /// <param name="color">Color of the line to be drawn</param>
        public void DrawLine(int x0, int y0, int x1, int y1, Color color)
        {
            if (y0 == y1)
            {
                DrawHorizontalLine(x0, y0, x1 - x0, color);
                return;
            }

            if (x0 == x1)
            {
                DrawVerticalLine(x0, y0, y1 - y0, color);
                return;
            }

            //ToDo ... replace this with DrawQuad that sets all four corners
            if (Stroke == 1)
            {
                DrawSingleWidthLine(x0, y0, x1, y1, color);
            }
            else if (IsTallerThanWide(x0, y0, x1, y1))
            {
                int xOffset = Stroke >> 1;

                for (int i = 0; i < Stroke; i++)
                {
                    DrawSingleWidthLine(x0 - xOffset + i, y0, x1 - xOffset + i, y1, color);
                }
            }
            else
            {
                int yOffset = Stroke >> 1;

                for (int i = 0; i < Stroke; i++)
                {
                    DrawSingleWidthLine(x0, y0 - yOffset + i, x1, y1 - yOffset + i, color);
                }
            }
        }

        // Helper method, can be integrated with DrawLine after we add DrawQuad
        private void DrawSingleWidthLine(int x0, int y0, int x1, int y1, Color color)
        {
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
                DrawPixel(steep ? y : x, steep ? x : y, color);
                error -= dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }

        /// <summary>
        /// Draw a horizontal line.
        /// </summary>
        /// <param name="x">Abscissa of the starting point of the line</param>
        /// <param name="y">Ordinate of the starting point of the line</param>
        /// <param name="length">Length of the line to draw</param>
        /// <param name="enabled">Turn the pixel on (true) or off (false)</param>
        public void DrawHorizontalLine(int x, int y, int length, bool enabled)
        {
            DrawHorizontalLine(x, y, length, enabled ? EnabledColor : DisabledColor);
        }

        /// <summary>
        /// Draw a horizontal line
        /// </summary>
        /// <param name="x">Abscissa of the starting point of the line</param>
        /// <param name="y">Ordinate of the starting point of the line</param>
        /// <param name="length">Length of the line to draw</param>
        public void DrawHorizontalLine(int x, int y, int length)
        {
            DrawHorizontalLine(x, y, length, PenColor);
        }

        /// <summary>
        /// Draw a horizontal line
        /// </summary>
        /// <param name="x">Abscissa of the starting point of the line</param>
        /// <param name="y">Ordinate of the starting point of the line</param>
        /// <param name="length">Length of the line to draw</param>
        /// <param name="color">The color of the line</param>
        public void DrawHorizontalLine(int x, int y, int length, Color color)
        {
            if (length == 0)
            {
                return;
            }

            if (length < 0)
            {
                x += length;
                length *= -1;
            }

            int yOffset = 0;
            int height = 1;

            if (Stroke > 1)
            {
                yOffset = Stroke >> 1;
                height = Stroke;
            }

            Fill(x, y - yOffset, length, height, color);
        }

        /// <summary>
        /// Draw a vertical line.
        /// </summary>
        /// <param name="x">Abscissa of the starting point of the line</param>
        /// <param name="y">Ordinate of the starting point of the line</param>
        /// <param name="length">Length of the line to draw</param>
        /// <param name="enabled">Show the line when (true) or off (false)</param>
        public void DrawVerticalLine(int x, int y, int length, bool enabled)
        {
            DrawVerticalLine(x, y, length, enabled ? EnabledColor : DisabledColor);
        }

        /// <summary>
        /// Draw a vertical line
        /// </summary>
        /// <param name="x">Abscissa of the starting point of the line</param>
        /// <param name="y">Ordinate of the starting point of the line</param>
        /// <param name="length">Length of the line to draw</param>
        public void DrawVerticalLine(int x, int y, int length)
        {
            DrawVerticalLine(x, y, length, PenColor);

            if (Stroke == 1)
            {

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

        /// <summary>
        /// Draw a vertical line
        /// </summary>
        /// <param name="x">Abscissa of the starting point of the line</param>
        /// <param name="y">Ordinate of the starting point of the line</param>
        /// <param name="length">Length of the line to draw</param>
        /// <param name="color">The color of the line</param>
        public void DrawVerticalLine(int x, int y, int length, Color color)
        {
            if (length < 0)
            {
                y += length;
                length *= -1;
            }

            int yOffset = 0;
            int width = 1;

            if (Stroke > 1)
            {
                yOffset = Stroke >> 1;
                width = Stroke;
            }

            Fill(x, y - yOffset, width, length, color);
        }

        /// <summary>
        /// Draw a circular arc between two angles
        /// </summary>
        /// <remarks>
        /// Note that y axis is inverted so the arc will be flipped from the standard cartesian plain
        /// </remarks>
        /// <param name="centerX">Abscissa of the center point of the circle</param>
        /// <param name="centerY">Ordinate of the center point of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="startAngle">The arc starting angle</param>
        /// <param name="endAngle">The arc ending angle</param>
        /// <param name="color">The color of the circle</param>
        /// <param name="centerBetweenPixels">If true, the center of the arc is between the assigned pixel and the next pixel, false it's directly on the center pixel</param>
        public void DrawArc(int centerX, int centerY, int radius, Angle startAngle, Angle endAngle, Color color, bool centerBetweenPixels = true)
        {
            var d = 3 - (2 * radius);
            var x = 0;
            var y = radius;

            int offset = centerBetweenPixels ? 1 : 0;

            double startAngleRadians = startAngle.Radians;
            double endAngleRadians = endAngle.Radians;

            if (startAngleRadians > endAngleRadians)
            {
                (endAngleRadians, startAngleRadians) = (startAngleRadians, endAngleRadians);
            }

            void DrawArcPoint(int x, int y, Color color)
            {
                if (Stroke == 1)
                {
                    DrawPixel(x, y, color);
                }
                else
                {
                    DrawCircleFilled(x, y, Stroke / 2, true, color);
                }
            }

            while (x <= y)
            {
                double angle1 = Math.Atan2(y, -x);
                double angle2 = Math.Atan2(x, -y);
                double angle3 = Math.Atan2(-x, -y);
                double angle4 = Math.Atan2(-y, -x);
                double angle5 = Math.Atan2(-y, x);
                double angle6 = Math.Atan2(-x, y);
                double angle7 = Math.Atan2(x, y);
                double angle8 = Math.Atan2(y, x);

                if (angle1 >= startAngleRadians && angle1 <= endAngleRadians) { DrawArcPoint(centerX + y - offset, centerY - x, color); }
                if (angle2 >= startAngleRadians && angle2 <= endAngleRadians) { DrawArcPoint(centerX + x - offset, centerY - y, color); }
                if (angle3 >= startAngleRadians && angle3 <= endAngleRadians) { DrawArcPoint(centerX - x, centerY - y, color); }
                if (angle4 >= startAngleRadians && angle4 <= endAngleRadians) { DrawArcPoint(centerX - y, centerY - x, color); }
                if (angle5 >= startAngleRadians && angle5 <= endAngleRadians) { DrawArcPoint(centerX - y, centerY + x - offset, color); }
                if (angle6 >= startAngleRadians && angle6 <= endAngleRadians) { DrawArcPoint(centerX - x, centerY + y - offset, color); }
                if (angle7 >= startAngleRadians && angle7 <= endAngleRadians) { DrawArcPoint(centerX + x - offset, centerY + y - offset, color); }
                if (angle8 >= startAngleRadians && angle8 <= endAngleRadians) { DrawArcPoint(centerX + y - offset, centerY + x - offset, color); }

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
        /// Draw a circular arc between two angles
        /// </summary>
        /// <remarks>
        /// Note that y axis is inverted so the arc will be flipped from the standard Cartesian plain
        /// </remarks>
        /// <param name="centerX">Abscissa of the center point of the circle</param>
        /// <param name="centerY">Ordinate of the center point of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="startAngle">The arc starting angle</param>
        /// <param name="endAngle">The arc ending angle</param>
        /// <param name="enabled">Should draw the arc (true) or remove (false)</param>
        /// <param name="centerBetweenPixels">If true, the center of the arc is between the assigned pixel and the next pixel, false it's directly on the center pixel</param>
        public void DrawArc(int centerX, int centerY, int radius, Angle startAngle, Angle endAngle, bool enabled = true, bool centerBetweenPixels = true)
        {
            DrawArc(centerX, centerY, radius, startAngle, endAngle, enabled ? EnabledColor : DisabledColor, centerBetweenPixels);
        }

        /// <summary>
        /// Draw a circular arc between two angles using PenColor
        /// </summary>
        /// <remarks>
        /// Note that y axis is inverted so the arc will be flipped from the standard Cartesian plain
        /// </remarks>
        /// <param name="centerX">Abscissa of the center point of the circle</param>
        /// <param name="centerY">Ordinate of the center point of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="startAngle">The arc starting angle</param>
        /// <param name="endAngle">The arc ending angle</param>
        /// <param name="centerBetweenPixels">If true, the center of the arc is between the assigned pixel and the next pixel, false it's directly on the center pixel</param>
        public void DrawArc(int centerX, int centerY, int radius, Angle startAngle, Angle endAngle, bool centerBetweenPixels = true)
        {
            DrawArc(centerX, centerY, radius, startAngle, endAngle, PenColor, centerBetweenPixels);
        }

        /// <summary>
        /// Draw a  triangle
        /// </summary>
        /// <param name="x0">Vertex #0 x coordinate</param>
        /// <param name="y0">Vertex #0 y coordinate</param>
        /// <param name="x1">Vertex #1 x coordinate</param>
        /// <param name="y1">Vertex #1 y coordinate</param>
        /// <param name="x2">Vertex #2 x coordinate</param>
        /// <param name="y2">Vertex #2 y coordinate</param>
        /// <param name="color">Color of triangle</param>
        /// <param name="filled">Draw a filled triangle?</param>
        public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Color color, bool filled = false)
        {
            if (filled)
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
        /// Draw a triangle
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
        /// <param name="enabled">Should the triangle add (true) or remove (false)</param>
        /// <param name="filled">Draw a filled triangle?</param>
        public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, bool enabled = true, bool filled = false)
        {
            DrawTriangle(x0, y0, x1, y1, x2, y2, enabled ? EnabledColor : DisabledColor, filled);
        }

        /// <summary>
        /// Draw a triangle
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
        /// <param name="filled">Draw a filled triangle?</param>
        public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, bool filled = false)
        {
            DrawTriangle(x0, y0, x1, y1, x2, y2, PenColor, filled);
        }

        private void Swap(ref int value1, ref int value2)
        {
            (value2, value1) = (value1, value2);
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
        private void DrawTriangleFilled(int x0, int y0, int x1, int y1, int x2, int y2, Color color)
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
                a = x0 + (sa / dy01);
                b = x0 + (sb / dy02);
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
                a = x1 + (sa / dy12);
                b = x0 + (sb / dy02);
                sa += dx12;
                sb += dx02;

                if (a > b) { Swap(ref a, ref b); }
                DrawHorizontalLine(a, y, b - a + 1, color);
            }
        }

        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <remarks>
        /// This algorithm draws the circle by splitting the full circle into eight segments.
        /// This method uses the Midpoint algorithm:
        /// https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        /// </remarks>
        /// <param name="centerX">Abscissa of the center point of the circle</param>
        /// <param name="centerY">Ordinate of the center point of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="enabled">Show the circle when true</param>
        /// <param name="filled">Draw a filled circle?</param>
        /// <param name="centerBetweenPixels">Set center between pixels</param>
        public void DrawCircle(int centerX, int centerY, int radius, bool enabled, bool filled = false, bool centerBetweenPixels = false)
        {
            DrawCircle(centerX, centerY, radius, enabled ? EnabledColor : DisabledColor, filled, centerBetweenPixels);
        }

        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <remarks>
        /// This algorithm draws the circle by splitting the full circle into eight segments
        /// This method uses the Midpoint algorithm:
        /// https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        /// </remarks>
        /// <param name="centerX">Abscissa of the center point of the circle</param>
        /// <param name="centerY">Ordinate of the center point of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="color">The color of the circle</param>
        /// <param name="filled">Draw a filled circle</param>
        /// <param name="centerBetweenPixels">If true, the center of the circle is between the assigned pixel and the next pixel, false it's directly on the center pixel</param>
        public void DrawCircle(int centerX, int centerY, int radius, Color color, bool filled = false, bool centerBetweenPixels = false)
        {
            if (filled)
            {
                DrawCircleFilled(centerX, centerY, radius, centerBetweenPixels, color);
            }
            else
            {
                if (Stroke == 1)
                {
                    DrawCircleOutline(centerX, centerY, radius, centerBetweenPixels, color);
                }
                else
                {
                    int offset = (Stroke - 1) >> 1;

                    for (int i = 0; i < Stroke; i++)
                    {
                        DrawCircleOutline(centerX, centerY, radius - offset + i, centerBetweenPixels, color);
                    }
                }
            }
        }

        /// <summary>
        /// Draw a circle
        /// </summary>
        /// <remarks>
        /// This algorithm draws the circle by splitting the full circle into eight segments
        /// This method uses the Midpoint algorithm:
        /// https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        /// </remarks>
        /// <param name="centerX">Abscissa of the center point of the circle</param>
        /// <param name="centerY">Ordinate of the center point of the circle</param>
        /// <param name="radius">Radius of the circle</param>
        /// <param name="filled">Draw a filled circle?</param>
        /// <param name="centerBetweenPixels">If true, the center of the circle is between the assigned pixel and the next pixel, false it's directly on the center pixel</param>
        public void DrawCircle(int centerX, int centerY, int radius, bool filled = false, bool centerBetweenPixels = false)
        {
            DrawCircle(centerX, centerY, radius, PenColor, filled, centerBetweenPixels);
        }

        /// <summary>
        /// Draws a circle quadrant (quarter circle)
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        /// <param name="quadrant"></param>
        /// <param name="enabled"></param>
        /// <param name="filled"></param>
        /// <param name="centerBetweenPixels"></param>
        public void DrawCircleQuadrant(int centerX, int centerY, int radius, int quadrant, bool enabled = true, bool filled = false, bool centerBetweenPixels = false)
        {
            DrawCircleQuadrant(centerX, centerY, radius, quadrant, enabled ? EnabledColor : DisabledColor, filled, centerBetweenPixels);
        }

        /// <summary>
        /// Draws a circle quadrant (quarter circle)
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        /// <param name="quadrant"></param>
        /// <param name="filled"></param>
        /// <param name="centerBetweenPixels"></param>
        public void DrawCircleQuadrant(int centerX, int centerY, int radius, int quadrant, bool filled = false, bool centerBetweenPixels = false)
        {
            DrawCircleQuadrant(centerX, centerY, radius, quadrant, PenColor, filled, centerBetweenPixels);
        }

        /// <summary>
        /// Draws a circle quadrant (quarter circle)
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        /// <param name="quadrant"></param>
        /// <param name="color"></param>
        /// <param name="filled"></param>
        /// <param name="centerBetweenPixels"></param>
        public void DrawCircleQuadrant(int centerX, int centerY, int radius, int quadrant, Color color, bool filled = false, bool centerBetweenPixels = false)
        {
            if (quadrant < 0 || quadrant > 3) { throw new ArgumentOutOfRangeException("DrawCircleQuadrant: quadrant must be between 0 & 3 inclusive"); }

            if (filled)
            {
                DrawCircleQuadrantFilled(centerX, centerY, radius, quadrant, color, centerBetweenPixels);
            }
            else
            {
                int offset = Stroke >> 1;

                for (int i = 0; i < Stroke; i++)
                {
                    DrawCircleQuadrantOutline(centerX, centerY, radius - offset + i, quadrant, color, centerBetweenPixels);
                }
            }
        }

        private void DrawCircleQuadrantFilled(int centerX, int centerY, int radius, int quadrant, Color color, bool centerBetweenPixels = false)
        {
            var d = 3 - (2 * radius);
            var x = 0;
            var y = radius;

            int offset = centerBetweenPixels ? 1 : 0;

            while (x <= y)
            {
                switch (quadrant)
                {
                    case 3:
                        DrawLine(centerX + x - offset, centerY + y - offset, centerX - offset, centerY + y - offset, color);
                        DrawLine(centerX + y - offset, centerY + x - offset, centerX - offset, centerY + x - offset, color);
                        break;
                    case 2:
                        DrawLine(centerX - y, centerY + x - offset, centerX, centerY + x - offset, color);
                        DrawLine(centerX - x, centerY + y - offset, centerX, centerY + y - offset, color);
                        break;
                    case 1:
                        DrawLine(centerX - x, centerY - y, centerX, centerY - y, color);
                        DrawLine(centerX - y, centerY - x, centerX, centerY - x, color);
                        break;
                    case 0:
                        DrawLine(centerX + x - offset, centerY - y, centerX - offset, centerY - y, color);
                        DrawLine(centerX + y - offset, centerY - x, centerX - offset, centerY - x, color);
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

        private void DrawCircleQuadrantOutline(int centerX, int centerY, int radius, int quadrant, Color color, bool centerBetweenPixels = false)
        {
            var d = 3 - (2 * radius); // (5 - (radius * 4)) / 4;
            var x = 0;
            var y = radius;

            int offset = centerBetweenPixels ? 1 : 0;

            while (x <= y)
            {
                switch (quadrant)
                {
                    case 3:
                        DrawPixel(centerX + x - offset, centerY + y - offset, color);
                        DrawPixel(centerX + y - offset, centerY + x - offset, color);
                        break;
                    case 2:
                        DrawPixel(centerX - y, centerY + x - offset, color);
                        DrawPixel(centerX - x, centerY + y - offset, color);
                        break;
                    case 1:
                        DrawPixel(centerX - x, centerY - y, color);
                        DrawPixel(centerX - y, centerY - x, color);
                        break;
                    case 0:
                        DrawPixel(centerX + x - offset, centerY - y, color);
                        DrawPixel(centerX + y - offset, centerY - x, color);
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

        private void DrawCircleOutline(int centerX, int centerY, int radius, bool centerBetweenPixels, Color color)
        {
            //I prefer the look of the original Bresenham’s decision param calculation
            var d = 3 - (2 * radius);
            var x = 0;
            var y = radius;

            int offset = centerBetweenPixels ? 1 : 0;

            while (x <= y)
            {
                DrawPixel(centerX + x - offset, centerY + y - offset, color);
                DrawPixel(centerX + y - offset, centerY + x - offset, color);

                DrawPixel(centerX - y, centerY + x - offset, color);
                DrawPixel(centerX - x, centerY + y - offset, color);

                DrawPixel(centerX - x, centerY - y, color);
                DrawPixel(centerX - y, centerY - x, color);

                DrawPixel(centerX + x - offset, centerY - y, color);
                DrawPixel(centerX + y - offset, centerY - x, color);

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

        private void DrawCircleFilled(int centerX, int centerY, int radius, bool centerBetweenPixels, Color color)
        {
            if (Stroke > 1)
            {
                radius += Stroke >> 1;
            }

            var d = 3 - (2 * radius);
            var x = 0;
            var y = radius;

            int offset = centerBetweenPixels ? 1 : 0;

            //override the stroke behavior 
            var stroke = Stroke;
            Stroke = 1;

            while (x <= y)
            {
                DrawHorizontalLine(centerX - x, centerY + y - offset, (2 * x) - offset + 1, color);
                DrawHorizontalLine(centerX - x, centerY - y, (2 * x) - offset + 1, color);
                DrawHorizontalLine(centerX - y, centerY + x - offset, (2 * y) - offset + 1, color);
                DrawHorizontalLine(centerX - y, centerY - x, (2 * y) - offset + 1, color);

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

            Stroke = stroke;
        }

        /// <summary>
        /// Draw a rectangle
        /// </summary>
        /// <param name="x">Abscissa of the top left corner</param>
        /// <param name="y">Ordinate of the top left corner</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="enabled">Turn pixels on (true) or turn pixels off (false)</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default)</param>
        public void DrawRectangle(int x, int y, int width, int height, bool enabled, bool filled = false)
        {
            DrawRectangle(x, y, width, height, enabled ? EnabledColor : DisabledColor, filled);
        }

        /// <summary>
        /// Draw a rectangle
        /// </summary>
        /// <param name="x">Abscissa of the top left corner</param>
        /// <param name="y">Ordinate of the top left corner</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default)</param>
        public void DrawRectangle(int x, int y, int width, int height, bool filled = false)
        {
            DrawRectangle(x, y, width, height, PenColor, filled);
        }

        /// <summary>
        /// Draw a rectangle
        /// </summary>
        /// <param name="x">Abscissa of the top left corner</param>
        /// <param name="y">Ordinate of the top left corner</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="color">The color of the rectangle</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default)</param>
        public void DrawRectangle(int x, int y, int width, int height, Color color, bool filled = false)
        {
            if (width < 0)
            {
                width *= -1;
                x -= width;
            }

            if (height < 0)
            {
                height *= -1;
                y -= height;
            }

            if (filled)
            {
                Fill(x, y, width, height, color);
            }
            else
            {
                width--;
                height--;

                DrawLine(x, y, x + width, y, color);
                DrawLine(x + width, y, x + width, y + height, color);
                DrawLine(x, y + height, x + width + 1, y + height, color);
                DrawLine(x, y, x, y + height, color);
            }
        }

        /// <summary>
        /// Draw a horizontal gradient filled rectangle
        /// </summary>
        /// <param name="x">Abscissa of the top left corner</param>
        /// <param name="y">Ordinate of the top left corner</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="colorLeft">The start (left) color of the gradient</param>
        /// <param name="colorRight">The end (right) color of the gradient</param>
        public void DrawHorizontalGradient(int x, int y, int width, int height, Color colorLeft, Color colorRight)
        {
            for (int i = 0; i < height; i++)
            {
                var color = colorLeft.Blend(colorRight, (float)i / height);
                DrawLine(x, i + y, x + width, i + y, color);
            }
        }

        /// <summary>
        /// Draw a vertical gradient filled rectangle
        /// </summary>
        /// <param name="x">Abscissa of the top left corner</param>
        /// <param name="y">Ordinate of the top left corner</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="colorTop">The start (top) color of the gradient</param>
        /// <param name="colorBottom">The end (bottom) color of the gradient</param>
        public void DrawVerticalGradient(int x, int y, int width, int height, Color colorTop, Color colorBottom)
        {
            for (int i = 0; i < width; i++)
            {
                var color = colorTop.Blend(colorBottom, (float)i / height);
                DrawLine(x + i, y, x + i, y + height, color);
            }
        }

        /// <summary>
        /// Draw a rounded rectangle
        /// </summary>
        /// <param name="x">Abscissa of the top left corner</param>
        /// <param name="y">Ordinate of the top left corner</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="cornerRadius">Radius of the corners of the rectangle</param>
        /// <param name="enabled">Turn pixels on (true) or turn pixels off (false)</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default)</param>
        public void DrawRoundedRectangle(int x, int y, int width, int height, int cornerRadius, bool enabled, bool filled = false)
        {
            DrawRoundedRectangle(x, y, width, height, cornerRadius, enabled ? EnabledColor : DisabledColor, filled);
        }

        /// <summary>
        /// Draw a rounded rectangle
        /// </summary>
        /// <param name="x">Abscissa of the top left corner</param>
        /// <param name="y">Ordinate of the top left corner</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="cornerRadius">Radius of the corners of the rectangle</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default)</param>
        public void DrawRoundedRectangle(int x, int y, int width, int height, int cornerRadius, bool filled = false)
        {
            DrawRoundedRectangle(x, y, width, height, cornerRadius, PenColor, filled);
        }

        /// <summary>
        /// Draw a rounded rectangle
        /// </summary>
        /// <param name="x">Abscissa of the top left corner</param>
        /// <param name="y">Ordinate of the top left corner</param>
        /// <param name="width">Width of the rectangle</param>
        /// <param name="height">Height of the rectangle</param>
        /// <param name="cornerRadius">Radius of the corners of the rectangle</param>
        /// <param name="color">The color of the rectangle</param>
        /// <param name="filled">Fill the rectangle (true) or draw the outline (false, default)</param>
        public void DrawRoundedRectangle(int x, int y, int width, int height, int cornerRadius, Color color, bool filled = false)
        {
            if (cornerRadius < 0) { throw new ArgumentOutOfRangeException("Radius must be positive"); }

            if (cornerRadius == 0)
            {
                DrawRectangle(x, y, width, height, color, filled);
                return;
            }

            if (filled)
            {
                DrawCircleQuadrant(x + width - cornerRadius - 1, y + cornerRadius, cornerRadius, 0, color, true);
                DrawCircleQuadrant(x + cornerRadius, y + cornerRadius, cornerRadius, 1, color, true);

                DrawCircleQuadrant(x + cornerRadius, y + height - cornerRadius - 1, cornerRadius, 2, color, true);
                DrawCircleQuadrant(x + width - cornerRadius - 1, y + height - cornerRadius - 1, cornerRadius, 3, color, true);

                DrawRectangle(x, y + cornerRadius, width, height - (2 * cornerRadius), color, filled);
                DrawRectangle(x + cornerRadius, y, width - (2 * cornerRadius), height, color, filled);
            }
            else
            {
                //corners
                DrawCircleQuadrant(x + width - cornerRadius - 1, y + cornerRadius, cornerRadius, 0, color, false);
                DrawCircleQuadrant(x + cornerRadius, y + cornerRadius, cornerRadius, 1, color, false);

                DrawCircleQuadrant(x + cornerRadius, y + height - cornerRadius - 1, cornerRadius, 2, color, false);
                DrawCircleQuadrant(x + width - cornerRadius - 1, y + height - cornerRadius - 1, cornerRadius, 3, color, false);

                //lines
                DrawLine(x + cornerRadius, y - 1, x + width - cornerRadius, y - 1, color);
                DrawLine(x + cornerRadius, y + height, x + width - cornerRadius, y + height, color);

                DrawLine(x, y + cornerRadius, x, y + height - cornerRadius, color);
                DrawLine(x + width - 1, y + cornerRadius, x + width - 1, y + height - cornerRadius, color);
            }
        }

        /// <summary>
        /// Get the size in pixels of a string using the current font
        /// </summary>
        /// <param name="text">The string to measure</param>
        /// <param name="scaleFactor">Scale factor used to calculate the size</param>
        public Size MeasureText(string text, ScaleFactor scaleFactor = ScaleFactor.X1)
        {
            return MeasureText(text, CurrentFont, scaleFactor);
        }

        /// <summary>
        /// Get the size in pixels of a string for a given font and scale factor
        /// </summary>
        /// <param name="text">The string to measure</param>
        /// <param name="font">The font used to calculate the text size</param>
        /// <param name="scaleFactor">Scale factor used to calculate the size</param>
        public Size MeasureText(string text, IFont font, ScaleFactor scaleFactor = ScaleFactor.X1)
        {
            return new Size(text.Length * (int)scaleFactor * font.Width, (int)scaleFactor * font.Height);
        }

        /// <summary>
        /// Draw a text message on the display using the current font.
        /// </summary>
        /// <param name="x">Abscissa of the location of the text</param>
        /// <param name="y">Ordinate of the location of the text</param>
        /// <param name="text">Text to display</param>
        /// <param name="color">Color of the text</param>
        /// <param name="scaleFactor">Scale factor used to calculate the size</param>
        /// <param name="alignmentH">Horizontal alignment: Left, Center or right aligned text</param>
        /// <param name="alignmentV">Vertical alignment: Top, Center or bottom aligned text</param>
        /// <param name="font">Optional font used to draw the text</param>
        public void DrawText(int x, int y, string text, Color color,
            ScaleFactor scaleFactor = ScaleFactor.X1,
            HorizontalAlignment alignmentH = HorizontalAlignment.Left,
            VerticalAlignment alignmentV = VerticalAlignment.Top,
            IFont? font = null)
        {
            if (string.IsNullOrEmpty(text)) return;

            var fontToDraw = (font ?? CurrentFont) ?? throw new Exception("CurrentFont must be set before calling DrawText.");

            byte[] bitMap = GetBytesForTextBitmap(text, fontToDraw);

            x = GetXForAlignment(x, MeasureText(text, fontToDraw, scaleFactor).Width, alignmentH);
            y = GetYForAlignment(y, MeasureText(text, fontToDraw, scaleFactor).Height, alignmentV);

            DrawBitmap(x, y, bitMap.Length / fontToDraw.Height * 8, fontToDraw.Height, bitMap, color, scaleFactor);
        }

        /// <summary>
        /// Draw a buffer onto the display buffer at the given location
        /// For best performance, source buffer should be the same color depth as the target display
        /// </summary>
        /// <param name="x">x location of target to draw buffer</param>
        /// <param name="y">y location of target to draw buffer</param>
        /// <param name="buffer">the source buffer to write to the display buffer</param>
        /// <param name="alignmentH">Horizontal alignment: Left, Center or right align the buffer to the x location</param>
        /// <param name="alignmentV">Vertical alignment: Top, Center or bottom align the buffer to the y location</param>
        public void DrawBuffer(int x, int y, IPixelBuffer buffer,
            HorizontalAlignment alignmentH = HorizontalAlignment.Left,
            VerticalAlignment alignmentV = VerticalAlignment.Top)
        {
            x = GetXForAlignment(x, buffer.Width, alignmentH);
            y = GetYForAlignment(y, buffer.Height, alignmentV);

            DrawBuffer(x, y, buffer);
        }

        /// <summary>
        /// Draw a buffer onto the display buffer at the given location
        /// For best performance, source buffer should be the same color depth as the target display
        /// </summary>
        /// <param name="x">x location of target to draw buffer</param>
        /// <param name="y">y location of target to draw buffer</param>
        /// <param name="buffer">the source buffer to write to the display buffer</param>
        public void DrawBuffer(int x, int y, IPixelBuffer buffer)
        {
            if (x >= Width || y >= Height || x + buffer.Width < 0 || y + buffer.Height < 0)
            {   //nothing to do 
                return;
            }

            int xStartIndex = 0;
            int yStartIndex = 0;
            int widthToDraw = buffer.Width;
            int heightToDraw = buffer.Height;

            bool isInBounds = true;

            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0)
                {
                    xStartIndex = 0 - x;
                    isInBounds = false;
                }
                if (y < 0)
                {
                    yStartIndex = 0 - y;
                    isInBounds = false;
                }

                if (x + buffer.Width > Width)
                {
                    widthToDraw = Width - x;
                    isInBounds = false;
                }

                if (y + buffer.Height > Height)
                {
                    heightToDraw = Height - y;
                    isInBounds = false;
                }
            }

            //fast and happy path
            if ((display is IRotatableDisplay || Rotation == RotationType.Default) && isInBounds)
            {
                PixelBuffer.WriteBuffer(x, y, buffer);
            }
            else  //loop over every pixel
            {
                for (int i = xStartIndex; i < widthToDraw; i++)
                {
                    for (int j = yStartIndex; j < heightToDraw; j++)
                    {
                        PixelBuffer.SetPixel(GetXForRotation(x + i, y + j),
                            GetYForRotation(x + i, y + j),
                            buffer.GetPixel(i, j));
                    }
                }
            }
        }

        /// <summary>
        /// Draw a buffer onto the display buffer at the given location
        /// with a transparency color that will not be drawn
        /// </summary>
        /// <param name="x">x location of target to draw buffer</param>
        /// <param name="y">y location of target to draw buffer</param>
        /// <param name="buffer">the source buffer to write to the display buffer</param>
        /// <param name="transparencyColor">the color to ignore when drawing the buffer</param>
        public void DrawBufferWithTransparencyColor(int x, int y, IPixelBuffer buffer, Color transparencyColor)
        {
            if (x >= Width || y >= Height || x + buffer.Width < 0 || y + buffer.Height < 0)
            {   //nothing to do 
                return;
            }
            int xStartIndex = 0;
            int yStartIndex = 0;
            int widthToDraw = buffer.Width;
            int heightToDraw = buffer.Height;

            if (IgnoreOutOfBoundsPixels)
            {
                if (x < 0)
                {
                    xStartIndex = 0 - x;
                }
                if (y < 0)
                {
                    yStartIndex = 0 - y;
                }

                if (x + buffer.Width > Width)
                {
                    widthToDraw = Width - x;
                }

                if (y + buffer.Height > Height)
                {
                    heightToDraw = Height - y;
                }
            }

            for (int i = xStartIndex; i < widthToDraw; i++)
            {
                for (int j = yStartIndex; j < heightToDraw; j++)
                {
                    var pixel = buffer.GetPixel(i, j);
                    if (pixel != transparencyColor)
                    {
                        PixelBuffer.SetPixel(GetXForRotation(x + i, y + j),
                                             GetYForRotation(x + i, y + j),
                                             pixel);
                    }
                }
            }
        }

        /// <summary>
        /// Draw an Image onto the display buffer at the specified location
        /// </summary>
        /// <param name="x">x location of target to draw buffer</param>
        /// <param name="y">x location of target to draw buffer</param>
        /// <param name="image">the source image to write to the display buffer</param>
        /// <param name="alignmentH">Horizontal alignment: Left, Center or right align the image to the x location</param>
        /// <param name="alignmentV">Vertical alignment: Top, Center or bottom align the image to the y location</param>
        public void DrawImage(int x, int y, Image image,
            HorizontalAlignment alignmentH = HorizontalAlignment.Left,
            VerticalAlignment alignmentV = VerticalAlignment.Top)
        {
            if (image.DisplayBuffer is not null)
            {
                DrawBuffer(x, y, image.DisplayBuffer, alignmentH, alignmentV);
            }
            else
            {
                throw new Exception("Image does not have a display buffer");
            }
        }

        /// <summary>
        /// Draw an Image onto the display buffer at the specified location
        /// </summary>
        /// <param name="x">x location of target to draw buffer</param>
        /// <param name="y">x location of target to draw buffer</param>
        /// <param name="image">the source image to write to the display buffer</param>
        public void DrawImage(int x, int y, Image image)
            => DrawImage(x, y, image, HorizontalAlignment.Left, VerticalAlignment.Top);

        /// <summary>
        /// Draw an Image onto the display buffer at (0, 0)
        /// </summary>
        /// <param name="image">the source image to write to the display buffer</param>
        public void DrawImage(Image image)
            => DrawImage(0, 0, image, HorizontalAlignment.Left, VerticalAlignment.Top);

        /// <summary>
        /// Draw a text message on the display using the current font
        /// </summary>
        /// <param name="x">Abscissa of the location of the text</param>
        /// <param name="y">Ordinate of the location of the text</param>
        /// <param name="text">Text to display</param>
        /// <param name="scaleFactor">Scale factor used to calculate the size</param>
        /// <param name="alignmentH">Horizontal alignment: Left, Center or right aligned text</param>
        /// <param name="alignmentV">Vertical alignment: Top, Center or bottom aligned text</param>
        public void DrawText(int x, int y, string text,
            ScaleFactor scaleFactor = ScaleFactor.X1,
            HorizontalAlignment alignmentH = HorizontalAlignment.Left,
            VerticalAlignment alignmentV = VerticalAlignment.Top)
        {
            DrawText(x, y, text, PenColor, scaleFactor, alignmentH, alignmentV);
        }

        private byte[] GetBytesForTextBitmap(string text, IFont font)
        {
            byte[] bitmap;

            if (font.Width == 8) //just copy bytes
            {
                bitmap = new byte[text.Length * font.Height * (font.Width >> 3)];

                byte[] characterMap;

                for (int i = 0; i < text.Length; i++)
                {
                    characterMap = font[text[i]];

                    //copy data for 1 character at a time going top to bottom
                    for (int segment = 0; segment < font.Height; segment++)
                    {
                        bitmap[i + (segment * text.Length)] = characterMap[segment];
                    }
                }
            }
            else if (font.Width == 12)
            {
                var len = ((text.Length + (text.Length % 2)) * 3) >> 1;
                bitmap = new byte[len * font.Height];

                byte[] charMap1, charMap2;
                int index = 0;

                for (int i = 0; i < text.Length; i += 2) //2 characters, 3 bytes ... 24 bytes total so the math is good
                {
                    //grab two characters at once
                    charMap1 = font[text[i]];
                    charMap2 = (i + 1 < text.Length) ? font[text[i + 1]] : font[' '];

                    int cIndex = 0;
                    for (int j = 0; j < font.Height; j += 2)
                    {
                        //first row - spans 3 bytes (for 2 chars)
                        bitmap[index + (j * len) + 0] = charMap1[cIndex]; //good
                        bitmap[index + (j * len) + 1] = (byte)((charMap1[cIndex + 1] & 0x0F) | (charMap2[cIndex] << 4));
                        bitmap[index + (j * len) + 2] = (byte)((charMap2[cIndex] >> 4) | (charMap2[cIndex + 1] << 4)); //good

                        //2nd row
                        bitmap[index + ((j + 1) * len) + 0] = (byte)((charMap1[cIndex + 1] >> 4) | (charMap1[cIndex + 2] << 4)); //good
                        bitmap[index + ((j + 1) * len) + 1] = (byte)((charMap1[cIndex + 2] >> 4) | (charMap2[cIndex + 1] & 0xF0));
                        bitmap[index + ((j + 1) * len) + 2] = charMap2[cIndex + 2]; //good

                        cIndex += 3;
                    }
                    index += 3;
                }
            }
            else if (font.Width == 6)
            {
                var len = text.Length;

                if (text.Length % 4 != 0)
                {
                    len += 4 - (text.Length % 4); //character length
                }
                len = len * 3 / 4; //length in bytes

                bitmap = new byte[len * font.Height];

                byte[] charMap1, charMap2, charMap3, charMap4;
                int index = 0;

                for (int i = 0; i < len; i += 3)
                {
                    //grab four characters at once
                    charMap1 = font[text[index++]];
                    charMap2 = (index < text.Length) ? font[text[index++]] : font[' '];
                    charMap3 = (index < text.Length) ? font[text[index++]] : font[' '];
                    charMap4 = (index < text.Length) ? font[text[index++]] : font[' '];

                    int cIndex = 0;
                    for (int j = 0; j < font.Height; j += 4)
                    {
                        //first row
                        bitmap[i + ((j + 0) * len) + 0] = (byte)((charMap1[cIndex] & 0x3F) | (charMap2[cIndex] << 6));
                        bitmap[i + ((j + 0) * len) + 1] = (byte)(((charMap2[cIndex] >> 2) & 0x0F) | (charMap3[cIndex] << 4));
                        bitmap[i + ((j + 0) * len) + 2] = (byte)(((charMap3[cIndex] >> 4) & 0x03) | (charMap4[cIndex] << 2));

                        //2nd row
                        bitmap[i + ((j + 1) * len) + 0] = (byte)((charMap1[cIndex] >> 6) | ((charMap1[cIndex + 1] << 2) & 0x3C) | (charMap2[cIndex] & 0xC0));
                        bitmap[i + ((j + 1) * len) + 1] = (byte)((charMap2[cIndex + 1] & 0x0F) | ((charMap3[cIndex] >> 2) & 0x30) | ((charMap3[cIndex + 1] << 6) & 0xC0));
                        bitmap[i + ((j + 1) * len) + 2] = (byte)(((charMap3[cIndex + 1] >> 2) & 0x03) | ((charMap4[cIndex] >> 4) & 0x0C) | (charMap4[cIndex + 1] << 4));

                        //3rd row
                        bitmap[i + ((j + 2) * len) + 0] = (byte)((charMap1[cIndex + 1] >> 4) | ((charMap1[cIndex + 2] << 4) & 0x30) | ((charMap2[cIndex + 1] << 2) & 0xC0)); //good
                        bitmap[i + ((j + 2) * len) + 1] = (byte)((charMap2[cIndex + 1] >> 6) | ((charMap2[cIndex + 2] << 2) & 0x0C) | (charMap3[cIndex + 1] & 0xF0)); //good
                        bitmap[i + ((j + 2) * len) + 2] = (byte)((charMap3[cIndex + 2] & 0x03) | ((charMap4[cIndex + 1] >> 2) & 0x3C) | (charMap4[cIndex + 2] << 6)); //good

                        //4th row
                        bitmap[i + ((j + 3) * len) + 0] = (byte)((charMap1[cIndex + 2] >> 2) | ((charMap2[cIndex + 2] << 4) & 0xC0));  //g
                        bitmap[i + ((j + 3) * len) + 1] = (byte)((charMap2[cIndex + 2] >> 4) | ((charMap3[cIndex + 2] << 2) & 0xF0)); //g
                        bitmap[i + ((j + 3) * len) + 2] = (byte)((charMap3[cIndex + 2] >> 6) | (charMap4[cIndex + 2] & 0xFC));

                        cIndex += 3;
                    }
                }
            }
            else if (font.Width == 4)
            {
                var len = (text.Length + (text.Length % 2)) >> 1;
                bitmap = new byte[len * font.Height];
                byte[] charMap1, charMap2;

                for (int i = 0; i < len; i++)
                {
                    //grab two characters at once to fill a complete byte
                    charMap1 = font[text[2 * i]];
                    charMap2 = ((2 * i) + 1 < text.Length) ? font[text[(2 * i) + 1]] : font[' '];

                    for (int j = 0; j < charMap1.Length; j++)
                    {
                        bitmap[i + (((j * 2) + 0) * len)] = (byte)((charMap1[j] & 0x0F) | (charMap2[j] << 4));
                        bitmap[i + (((j * 2) + 1) * len)] = (byte)((charMap1[j] >> 4) | (charMap2[j] & 0xF0));
                    }
                }
            }
            else
            {
                throw new Exception("Font width must be 4, 6, 8, or 12");
            }
            return bitmap;
        }

        /// <summary>
        /// Update the display target from the buffer (thread safe)
        /// </summary>
        public virtual void Show()
        {
            lock (_lock)
            {
                if (isUpdating)
                {
                    return;
                }

                isUpdating = true;
            }

            display?.Show();

            isUpdating = false;
        }

        /// <summary>
        /// Update the display target from the buffer (thread safe) while respecting MinimumTimeBetweenUpdates
        /// </summary>
        public virtual async Task ShowBuffered()
        {
            lock (_lock)
            {
                if (isUpdating)
                {
                    isUpdateRequested = true;
                    return;
                }
            }

            isUpdating = true;

            var timeSinceLastUpdate = DateTime.UtcNow - lastUpdated;

            if (timeSinceLastUpdate < DelayBetweenFrames)
            {
                await Task.Delay(DelayBetweenFrames - timeSinceLastUpdate);
            }

            await Task.Run(() => display?.Show());
            lastUpdated = DateTime.UtcNow;

            if (isUpdateRequested)
            {
                isUpdateRequested = false;
                await Task.Delay(DelayBetweenFrames);
                await ShowBuffered();
            }

            isUpdating = false;
        }

        /// <summary>
        /// Update the display target from the buffer (not thread safe)
        /// </summary>
        public virtual void ShowUnsafe()
        {
            display?.Show();
        }

        /// <summary>
        /// Update a region of the display
        /// Note: not all displays support partial updates
        /// </summary>
        public virtual void ShowUnsafe(int left, int top, int right, int bottom)
        {
            display?.Show(left, top, right, bottom);
        }

        /// <summary>
        /// Update a region of the the display target from the buffer (thread safe)
        /// Note: not all displays support partial updates
        /// </summary>
        /// <param name="left">The left coordinate of the display area to update</param>
        /// <param name="top">The top coordinate of the display area to update</param>
        /// <param name="right">The right coordinate of the display area to update</param>
        /// <param name="bottom">The bottom coordinate of the display area to update</param>
        public virtual void Show(int left, int top, int right, int bottom)
        {
            lock (_lock)
            {
                if (isUpdating)
                {
                    return;
                }

                isUpdating = true;
            }

            display?.Show(left, top, right, bottom);

            isUpdating = false;
        }

        /// <summary>
        /// Update a region of the display
        /// Note: not all displays support partial updates
        /// </summary>
        public virtual void Show(Rect rect)
        {
            Show(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        /// <summary>
        /// Clear the pixel buffer
        /// </summary>
        /// <param name="updateDisplay">Update the display immediately when true</param>
        public virtual void Clear(bool updateDisplay = false)
        {
            if (display == null || display.DisabledColor == Color.Black)
            {
                PixelBuffer.Clear();
            }
            else
            {
                PixelBuffer.Fill(display.DisabledColor);
            }

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Clear a region of the display pixel buffer
        /// </summary>
        /// <param name="originX">The X coord to start</param>
        /// <param name="originY">The Y coord to start</param>
        /// <param name="width">The width of the region to clear</param>
        /// <param name="height">The height of the region to clear</param>
        /// <param name="updateDisplay">Update the display immediately when true</param>
        public virtual void Clear(int originX, int originY, int width, int height, bool updateDisplay = false)
        {
            PixelBuffer.Fill(originX, originY, width, height, DisabledColor);

            if (updateDisplay)
            {
                Show();
            }
        }

        /// <summary>
        /// Clear the pixel buffer to a color
        /// </summary>
        /// <param name="updateDisplay">Update the display immediately when true</param>
        /// <param name="color">Color to set display</param>
        public virtual void Clear(Color color, bool updateDisplay = false)
        {
            DrawRectangle(0, 0, Width, Height, color, true);

            if (updateDisplay) { Show(); }
        }

        /// <summary>
        /// Writes a 1-bit bitmap stored in a byte array
        /// </summary>
        /// <param name="x">Abscissa of the top left corner of the bitmap</param>
        /// <param name="y">Ordinate of the top left corner of the bitmap</param>
        /// <param name="width">Width of the bitmap in pixels</param>
        /// <param name="height">Height of the bitmap in pixels</param>
        /// <param name="bitmap">Bitmap to display</param>
        /// <param name="color">The color of the bitmap</param>
        /// <param name="scaleFactor">The integer scale factor (default is 1)</param>
        protected void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, Color color, ScaleFactor scaleFactor = ScaleFactor.X1)
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
                                Fill(x: x + (8 * abscissa * scale) + (pixel * scale),
                                    y: y + (ordinate * scale),
                                    width: scale,
                                    height: scale,
                                    color: color);
                            }
                            else
                            {   //1x
                                DrawPixel(x + (8 * abscissa) + pixel, y + ordinate, color);
                            }
                        }
                        mask <<= 1;
                    }
                }
            }
        }

        /// <summary>
        /// Writes a 1-bit bitmap to the buffer - used for font rendering
        /// </summary>
        /// <param name="x">Abscissa of the top left corner of the bitmap</param>
        /// <param name="y">Ordinate of the top left corner of the bitmap</param>
        /// <param name="width">Width of the bitmap in pixels</param>
        /// <param name="height">Height of the bitmap in pixels</param>
        /// <param name="bitmap">Bitmap to display</param>
        /// <param name="scaleFactor">The integer scale factor (default is 1)</param>
        protected void DrawBitmap(int x, int y, int width, int height, byte[] bitmap, ScaleFactor scaleFactor = ScaleFactor.X1)
        {
            DrawBitmap(x, y, width, height, bitmap, PenColor, scaleFactor);
        }

        /// <summary>
        /// Get x pixel position for the current graphics rotation
        /// </summary>
        /// <param name="x">The non-rotated x position</param>
        /// <param name="y">The non-rotated y position</param>
        /// <returns></returns>
        public int GetXForRotation(int x, int y)
        {
            if (display is IRotatableDisplay) { return x; }

            return Rotation switch
            {
                RotationType._90Degrees => PixelBuffer.Width - y - 1,
                RotationType._180Degrees => PixelBuffer.Width - x - 1,
                RotationType._270Degrees => y,
                _ => x,
            };
        }

        /// <summary>
        /// Get y pixel position for the current graphics rotation
        /// </summary>
        /// <param name="x">The non-rotated x position</param>
        /// <param name="y">The non-rotated y position</param>
        /// <returns></returns>
        public int GetYForRotation(int x, int y)
        {
            if (display is IRotatableDisplay) { return y; }

            return Rotation switch
            {
                RotationType._90Degrees => x,
                RotationType._180Degrees => PixelBuffer.Height - y - 1,
                RotationType._270Degrees => PixelBuffer.Height - x - 1,
                _ => y,
            };
        }

        private bool IsCoordinateInBounds(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return false;

            return true;
        }

        private void Fill(int x, int y, int width, int height, Color color)
        {
            if (IgnoreOutOfBoundsPixels)
            {
                if (x >= Width ||
                    y >= Height ||
                    width < 1 ||
                    height < 1)
                {
                    return;
                }

                if (x < 0) x = 0;
                if (y < 0) y = 0;

                if (x + width >= Width) width = Width - x;
                if (y + height >= Height) height = Height - y;
            }

            if (display is IRotatableDisplay)
            {
                PixelBuffer.Fill(x, y, width, height, color);
                return;
            }

            switch (Rotation)
            {
                case RotationType.Default:
                    PixelBuffer.Fill(x, y, width, height, color);
                    break;
                case RotationType._90Degrees:
                    PixelBuffer.Fill(GetXForRotation(x, y) - height + 1, GetYForRotation(x, y), height, width, color);
                    break;
                case RotationType._180Degrees:
                    PixelBuffer.Fill(GetXForRotation(x, y) - width + 1, GetYForRotation(x, y) - height + 1, width, height, color);
                    break;
                case RotationType._270Degrees:
                    PixelBuffer.Fill(GetXForRotation(x, y), GetYForRotation(x, y) - width + 1, height, width, color);
                    break;
            }
        }

        private int GetXForAlignment(int x, int width, HorizontalAlignment alignmentH)
        {
            if (alignmentH == HorizontalAlignment.Center)
            {
                x -= width / 2;
            }
            else if (alignmentH == HorizontalAlignment.Right)
            {
                x -= width;
            }
            return x;
        }

        private int GetYForAlignment(int y, int height, VerticalAlignment alignmentV)
        {
            if (alignmentV == VerticalAlignment.Center)
            {
                y -= height / 2;
            }
            else if (alignmentV == VerticalAlignment.Bottom)
            {
                y -= height;
            }
            return y;
        }
    }
}