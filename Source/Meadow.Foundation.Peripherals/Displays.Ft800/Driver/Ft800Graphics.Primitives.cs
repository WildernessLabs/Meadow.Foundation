using System;

namespace Meadow.Foundation.Displays;

/// <summary>
/// FT800Graphics primitive drawing methods (lines, rectangles, circles, etc.)
/// </summary>
public partial class Ft800Graphics
{
    // ========================================================================
    // Points
    // ========================================================================

    /// <summary>
    /// Draw a point (circle) at the specified location
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="color">Point color</param>
    /// <param name="radius">Point radius in pixels (default 1)</param>
    public void DrawPoint(int x, int y, Color color, float radius = 1)
    {
        SetColor(color);
        AddCommand(Ft800Defs.DL_POINT_SIZE | (uint)(radius * 16));
        AddCommand(Ft800Defs.DL_BEGIN | Ft800Defs.PRIM_POINTS);
        AddCommand(Vertex2F(x, y));
        AddCommand(Ft800Defs.DL_END);
    }

    /// <summary>
    /// Draw a point using the current pen color
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    public void DrawPoint(int x, int y)
    {
        DrawPoint(x, y, currentColor, currentPointSize / 16f);
    }

    /// <summary>
    /// Draw a single pixel
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="color">Pixel color</param>
    public void DrawPixel(int x, int y, Color color)
    {
        DrawPoint(x, y, color, 0.5f);
    }

    // ========================================================================
    // Lines
    // ========================================================================

    /// <summary>
    /// Draw a line between two points
    /// </summary>
    /// <param name="x0">Start X coordinate</param>
    /// <param name="y0">Start Y coordinate</param>
    /// <param name="x1">End X coordinate</param>
    /// <param name="y1">End Y coordinate</param>
    /// <param name="color">Line color</param>
    /// <param name="width">Line width in pixels (default 1)</param>
    public void DrawLine(int x0, int y0, int x1, int y1, Color color, float width = 1)
    {
        SetColor(color);
        AddCommand(Ft800Defs.DL_LINE_WIDTH | (uint)(width * 16));
        AddCommand(Ft800Defs.DL_BEGIN | Ft800Defs.PRIM_LINES);
        AddCommand(Vertex2F(x0, y0));
        AddCommand(Vertex2F(x1, y1));
        AddCommand(Ft800Defs.DL_END);
    }

    /// <summary>
    /// Draw a line using the current pen color and line width
    /// </summary>
    /// <param name="x0">Start X coordinate</param>
    /// <param name="y0">Start Y coordinate</param>
    /// <param name="x1">End X coordinate</param>
    /// <param name="y1">End Y coordinate</param>
    public void DrawLine(int x0, int y0, int x1, int y1)
    {
        DrawLine(x0, y0, x1, y1, currentColor, currentLineWidth / 16f);
    }

    /// <summary>
    /// Draw a horizontal line
    /// </summary>
    /// <param name="x">Start X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="length">Line length in pixels</param>
    /// <param name="color">Line color</param>
    public void DrawHorizontalLine(int x, int y, int length, Color color)
    {
        DrawLine(x, y, x + length - 1, y, color);
    }

    /// <summary>
    /// Draw a vertical line
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Start Y coordinate</param>
    /// <param name="length">Line length in pixels</param>
    /// <param name="color">Line color</param>
    public void DrawVerticalLine(int x, int y, int length, Color color)
    {
        DrawLine(x, y, x, y + length - 1, color);
    }

    /// <summary>
    /// Draw a connected series of lines (line strip)
    /// </summary>
    /// <param name="points">Array of (x, y) points</param>
    /// <param name="color">Line color</param>
    /// <param name="width">Line width in pixels</param>
    public void DrawLineStrip((int x, int y)[] points, Color color, float width = 1)
    {
        if (points == null || points.Length < 2) return;

        SetColor(color);
        AddCommand(Ft800Defs.DL_LINE_WIDTH | (uint)(width * 16));
        AddCommand(Ft800Defs.DL_BEGIN | Ft800Defs.PRIM_LINE_STRIP);

        foreach (var point in points)
        {
            AddCommand(Vertex2F(point.x, point.y));
        }

        AddCommand(Ft800Defs.DL_END);
    }

    // ========================================================================
    // Rectangles
    // ========================================================================

    /// <summary>
    /// Draw a rectangle
    /// </summary>
    /// <param name="x">X coordinate of top-left corner</param>
    /// <param name="y">Y coordinate of top-left corner</param>
    /// <param name="width">Rectangle width</param>
    /// <param name="height">Rectangle height</param>
    /// <param name="color">Rectangle color</param>
    /// <param name="filled">If true, fill the rectangle; otherwise draw outline only</param>
    public void DrawRectangle(int x, int y, int width, int height, Color color, bool filled = false)
    {
        if (filled)
        {
            DrawFilledRectangle(x, y, width, height, color);
        }
        else
        {
            DrawRectangleOutline(x, y, width, height, color);
        }
    }

    /// <summary>
    /// Draw a filled rectangle using the RECTS primitive
    /// </summary>
    private void DrawFilledRectangle(int x, int y, int width, int height, Color color)
    {
        SetColor(color);
        AddCommand(Ft800Defs.DL_LINE_WIDTH | 16);  // 1 pixel for sharp corners
        AddCommand(Ft800Defs.DL_BEGIN | Ft800Defs.PRIM_RECTS);
        AddCommand(Vertex2F(x, y));
        AddCommand(Vertex2F(x + width, y + height));
        AddCommand(Ft800Defs.DL_END);
    }

    /// <summary>
    /// Draw a rectangle outline using lines
    /// </summary>
    private void DrawRectangleOutline(int x, int y, int width, int height, Color color)
    {
        SetColor(color);
        AddCommand(Ft800Defs.DL_LINE_WIDTH | (uint)currentLineWidth);
        AddCommand(Ft800Defs.DL_BEGIN | Ft800Defs.PRIM_LINE_STRIP);
        AddCommand(Vertex2F(x, y));
        AddCommand(Vertex2F(x + width, y));
        AddCommand(Vertex2F(x + width, y + height));
        AddCommand(Vertex2F(x, y + height));
        AddCommand(Vertex2F(x, y));
        AddCommand(Ft800Defs.DL_END);
    }

    /// <summary>
    /// Draw a rounded rectangle
    /// </summary>
    /// <param name="x">X coordinate of top-left corner</param>
    /// <param name="y">Y coordinate of top-left corner</param>
    /// <param name="width">Rectangle width</param>
    /// <param name="height">Rectangle height</param>
    /// <param name="cornerRadius">Corner radius in pixels</param>
    /// <param name="color">Rectangle color</param>
    /// <param name="filled">If true, fill the rectangle</param>
    public void DrawRoundedRectangle(int x, int y, int width, int height, int cornerRadius, Color color, bool filled = true)
    {
        // FT800's RECTS primitive with LINE_WIDTH creates rounded corners
        SetColor(color);
        AddCommand(Ft800Defs.DL_LINE_WIDTH | (uint)(cornerRadius * 16));
        AddCommand(Ft800Defs.DL_BEGIN | Ft800Defs.PRIM_RECTS);
        AddCommand(Vertex2F(x + cornerRadius, y + cornerRadius));
        AddCommand(Vertex2F(x + width - cornerRadius, y + height - cornerRadius));
        AddCommand(Ft800Defs.DL_END);
    }

    // ========================================================================
    // Circles
    // ========================================================================

    /// <summary>
    /// Draw a circle
    /// </summary>
    /// <param name="cx">Center X coordinate</param>
    /// <param name="cy">Center Y coordinate</param>
    /// <param name="radius">Circle radius in pixels</param>
    /// <param name="color">Circle color</param>
    /// <param name="filled">If true, fill the circle</param>
    public void DrawCircle(int cx, int cy, int radius, Color color, bool filled = false)
    {
        if (filled)
        {
            // A filled circle is just a large point
            DrawPoint(cx, cy, color, radius);
        }
        else
        {
            // Draw circle outline using points around the circumference
            // For better results, use the co-processor or approximate with line segments
            DrawCircleOutline(cx, cy, radius, color);
        }
    }

    /// <summary>
    /// Draw a circle outline using line segments
    /// </summary>
    private void DrawCircleOutline(int cx, int cy, int radius, Color color)
    {
        SetColor(color);
        AddCommand(Ft800Defs.DL_LINE_WIDTH | (uint)currentLineWidth);
        AddCommand(Ft800Defs.DL_BEGIN | Ft800Defs.PRIM_LINE_STRIP);

        // Approximate circle with 32 segments
        int segments = 32;
        for (int i = 0; i <= segments; i++)
        {
            double angle = 2 * Math.PI * i / segments;
            int x = cx + (int)(radius * Math.Cos(angle));
            int y = cy + (int)(radius * Math.Sin(angle));
            AddCommand(Vertex2F(x, y));
        }

        AddCommand(Ft800Defs.DL_END);
    }

    // ========================================================================
    // Triangles
    // ========================================================================

    /// <summary>
    /// Draw a triangle
    /// </summary>
    /// <param name="x0">First vertex X</param>
    /// <param name="y0">First vertex Y</param>
    /// <param name="x1">Second vertex X</param>
    /// <param name="y1">Second vertex Y</param>
    /// <param name="x2">Third vertex X</param>
    /// <param name="y2">Third vertex Y</param>
    /// <param name="color">Triangle color</param>
    /// <param name="filled">If true, fill the triangle</param>
    public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Color color, bool filled = false)
    {
        if (filled)
        {
            // Use edge strips to fill the triangle
            DrawFilledTriangle(x0, y0, x1, y1, x2, y2, color);
        }
        else
        {
            // Draw outline using line strip
            SetColor(color);
            AddCommand(Ft800Defs.DL_LINE_WIDTH | (uint)currentLineWidth);
            AddCommand(Ft800Defs.DL_BEGIN | Ft800Defs.PRIM_LINE_STRIP);
            AddCommand(Vertex2F(x0, y0));
            AddCommand(Vertex2F(x1, y1));
            AddCommand(Vertex2F(x2, y2));
            AddCommand(Vertex2F(x0, y0));
            AddCommand(Ft800Defs.DL_END);
        }
    }

    /// <summary>
    /// Draw a filled triangle using edge strips
    /// </summary>
    private void DrawFilledTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Color color)
    {
        // Sort vertices by Y coordinate
        if (y0 > y1) { (x0, y0, x1, y1) = (x1, y1, x0, y0); }
        if (y0 > y2) { (x0, y0, x2, y2) = (x2, y2, x0, y0); }
        if (y1 > y2) { (x1, y1, x2, y2) = (x2, y2, x1, y1); }

        SetColor(color);

        // Use EDGE_STRIP_B to draw filled triangle
        AddCommand(Ft800Defs.DL_BEGIN | Ft800Defs.PRIM_EDGE_STRIP_B);
        AddCommand(Vertex2F(x0, y0));
        AddCommand(Vertex2F(x1, y1));
        AddCommand(Vertex2F(x2, y2));
        AddCommand(Ft800Defs.DL_END);
    }

    // ========================================================================
    // Gradients
    // ========================================================================

    /// <summary>
    /// Draw a horizontal gradient
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="width">Gradient width</param>
    /// <param name="height">Gradient height</param>
    /// <param name="colorLeft">Left edge color</param>
    /// <param name="colorRight">Right edge color</param>
    public void DrawHorizontalGradient(int x, int y, int width, int height, Color colorLeft, Color colorRight)
    {
        // Note: True gradients require the co-processor CMD_GRADIENT
        // This is a simple approximation using vertical stripes
        int steps = Math.Min(width, 64);
        int stepWidth = width / steps;

        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / (steps - 1);
            byte r = (byte)(colorLeft.R + (colorRight.R - colorLeft.R) * t);
            byte g = (byte)(colorLeft.G + (colorRight.G - colorLeft.G) * t);
            byte b = (byte)(colorLeft.B + (colorRight.B - colorLeft.B) * t);

            DrawFilledRectangle(x + i * stepWidth, y, stepWidth + 1, height, new Color(r, g, b));
        }
    }

    /// <summary>
    /// Draw a vertical gradient
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="width">Gradient width</param>
    /// <param name="height">Gradient height</param>
    /// <param name="colorTop">Top edge color</param>
    /// <param name="colorBottom">Bottom edge color</param>
    public void DrawVerticalGradient(int x, int y, int width, int height, Color colorTop, Color colorBottom)
    {
        int steps = Math.Min(height, 64);
        int stepHeight = height / steps;

        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / (steps - 1);
            byte r = (byte)(colorTop.R + (colorBottom.R - colorTop.R) * t);
            byte g = (byte)(colorTop.G + (colorBottom.G - colorTop.G) * t);
            byte b = (byte)(colorTop.B + (colorBottom.B - colorTop.B) * t);

            DrawFilledRectangle(x, y + i * stepHeight, width, stepHeight + 1, new Color(r, g, b));
        }
    }
}
