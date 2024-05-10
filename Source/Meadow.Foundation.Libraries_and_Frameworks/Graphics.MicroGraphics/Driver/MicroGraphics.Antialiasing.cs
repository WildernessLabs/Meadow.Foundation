using System;

namespace Meadow.Foundation.Graphics
{
    public partial class MicroGraphics
    {
        /// <summary>
        /// Draws an anti-aliased line between two points using the Xiaolin Wu algorithm
        /// </summary>
        /// <param name="x0">The x-coordinate of the starting point of the line</param>
        /// <param name="y0">The y-coordinate of the starting point of the line</param>
        /// <param name="x1">The x-coordinate of the ending point of the line</param>
        /// <param name="y1">The y-coordinate of the ending point of the line</param>
        /// <param name="color">The color of the line</param>
        /// <remarks>
        /// This method uses the Xiaolin Wu line algorithm to draw an anti-aliased line between two points.
        /// </remarks>
        public void DrawLineAntialiased(float x0, float y0, float x1, float y1, Color color)
        {
            bool steep = MathF.Abs(y1 - y0) > MathF.Abs(x1 - x0);
            float temp;
            if (steep)
            {
                temp = x0; x0 = y0; y0 = temp;
                temp = x1; x1 = y1; y1 = temp;
            }
            if (x0 > x1)
            {
                temp = x0; x0 = x1; x1 = temp;
                temp = y0; y0 = y1; y1 = temp;
            }

            float dx = x1 - x0;
            float dy = y1 - y0;
            float gradient = dy / dx;

            float xEnd = Round(x0);
            float yEnd = y0 + gradient * (xEnd - x0);
            float xGap = GetRFPart(x0 + 0.5f);
            float xPixel1 = xEnd;
            float yPixel1 = GetIPart(yEnd);

            if (steep)
            {
                DrawPixelWithAlpha(yPixel1, xPixel1, color, GetRFPart(yEnd) * xGap);
                DrawPixelWithAlpha(yPixel1 + 1, xPixel1, color, GetFPart(yEnd) * xGap);
            }
            else
            {
                DrawPixelWithAlpha(xPixel1, yPixel1, color, GetRFPart(yEnd) * xGap);
                DrawPixelWithAlpha(xPixel1, yPixel1 + 1, color, GetFPart(yEnd) * xGap);
            }
            float intery = yEnd + gradient;

            xEnd = Round(x1);
            yEnd = y1 + gradient * (xEnd - x1);
            xGap = GetFPart(x1 + 0.5f);

            float xPixel2 = xEnd;
            float yPixel2 = GetIPart(yEnd);
            if (steep)
            {
                DrawPixelWithAlpha(yPixel2, xPixel2, color, GetRFPart(yEnd) * xGap);
                DrawPixelWithAlpha(yPixel2 + 1, xPixel2, color, GetFPart(yEnd) * xGap);
            }
            else
            {
                DrawPixelWithAlpha(xPixel2, yPixel2, color, GetRFPart(yEnd) * xGap);
                DrawPixelWithAlpha(xPixel2, yPixel2 + 1, color, GetFPart(yEnd) * xGap);
            }

            if (steep)
            {
                for (int x = (int)(xPixel1 + 1); x <= xPixel2 - 1; x++)
                {
                    DrawPixelWithAlpha(GetIPart(intery), x, color, GetRFPart(intery));
                    DrawPixelWithAlpha(GetIPart(intery) + 1, x, color, GetFPart(intery));
                    intery += gradient;
                }
            }
            else
            {
                for (int x = (int)(xPixel1 + 1); x <= xPixel2 - 1; x++)
                {
                    DrawPixelWithAlpha(x, GetIPart(intery), color, GetRFPart(intery));
                    DrawPixelWithAlpha(x, GetIPart(intery) + 1, color, GetFPart(intery));
                    intery += gradient;
                }
            }
        }

        /// <summary>
        /// Draw an anti-aliased triangle
        /// </summary>
        /// <param name="x0">Vertex #0 x coordinate</param>
        /// <param name="y0">Vertex #0 y coordinate</param>
        /// <param name="x1">Vertex #1 x coordinate</param>
        /// <param name="y1">Vertex #1 y coordinate</param>
        /// <param name="x2">Vertex #2 x coordinate</param>
        /// <param name="y2">Vertex #2 y coordinate</param>
        /// <param name="color">The color of the triangle</param>
        /// <param name="filled">Draw a filled triangle?</param>
        public void DrawTriangleAntialiased(int x0, int y0, int x1, int y1, int x2, int y2, Color color, bool filled = false)
        {
            DrawLineAntialiased(x0, y0, x1, y1, color);
            DrawLineAntialiased(x1, y1, x2, y2, color);
            DrawLineAntialiased(x2, y2, x0, y0, color);
            if (filled)
            {
                DrawTriangleFilled(x0, y0, x1, y1, x2, y2, color);
            }
        }

        private int GetIPart(float value) => (int)value;

        private int Round(float value) => GetIPart(value + 0.5f);

        private float GetFPart(float value)
        {
            if (value < 0)
            {
                return 1 - (value - MathF.Floor(value));
            }
            return value - MathF.Floor(value);
        }

        private float GetRFPart(float value)
        {
            return 1 - GetFPart(value);
        }
    }
}