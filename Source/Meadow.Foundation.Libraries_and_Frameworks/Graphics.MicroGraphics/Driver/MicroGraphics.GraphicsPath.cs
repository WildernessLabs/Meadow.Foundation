using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Graphics
{
    public partial class MicroGraphics
    {
        /// <summary>
        /// Draw a graphics path 
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="enabled">Should pixels be enabled (on) or disabled (off)</param>
        /// <param name="filled">Fill the path (true) or draw the outline (false, default)</param>
        public void DrawPath(GraphicsPath path, bool enabled, bool filled = false)
        {
            DrawPath(path, enabled ? Color.White : Color.Black, filled);
        }

        /// <summary>
        /// Draw a graphics path 
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="filled">Fill the path (true) or draw the outline (false, default)</param>
        public void DrawPath(GraphicsPath path, bool filled = false)
        {
            DrawPath(path, PenColor, filled);
        }

        /// <summary>
        /// Draw a graphics path
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="color">The color to draw the path</param>
        /// <param name="filled">Fill the path (true) or draw the outline (false, default)</param>
        public void DrawPath(GraphicsPath path, Color color, bool filled = false)
        {
            if (filled)
            {
                FillPath(path, color);
                return;
            }

            for (int i = 0; i < path.PointCount; i++)
            {
                if (path.PathActions[i].Verb == VerbType.Move || i == 0)
                {
                    continue;
                }

                DrawLine(path.PathActions[i - 1].PathPoint.X,
                         path.PathActions[i - 1].PathPoint.Y,
                         path.PathActions[i].PathPoint.X,
                         path.PathActions[i].PathPoint.Y,
                        color);
            }
        }

        /// <summary>
        /// Draw a filled graphics path
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="fillColor">The color to fill the path</param>
        protected void FillPath(GraphicsPath path, Color fillColor)
        {
            if (path.PointCount < 3)
            {
                return; // A path must have at least 3 points to form a filled shape
            }

            // Find the bounding box of the path
            var bounds = path.Bounds;
            int minY = bounds.Top;
            int maxY = bounds.Top + bounds.Height;

            // Create an edge list for each scanline
            List<Edge>[] edges = new List<Edge>[maxY - minY + 1];
            for (int i = 0; i < edges.Length; i++)
            {
                edges[i] = new List<Edge>();
            }

            // Build the edge list
            for (int i = 0; i < path.PointCount; i++)
            {
                var p1 = path.PathActions[i].PathPoint;
                var p2 = path.PathActions[(i + 1) % path.PointCount].PathPoint;

                if (p1.Y == p2.Y)
                {
                    continue; // Skip horizontal lines
                }

                var ymin = Math.Min(p1.Y, p2.Y);
                var ymax = Math.Max(p1.Y, p2.Y);
                var xmin = (p1.Y < p2.Y) ? p1.X : p2.X;

                float invSlope = (float)(p2.X - p1.X) / (p2.Y - p1.Y);

                for (int y = ymin; y < ymax; y++)
                {
                    if (y - minY < 0 || y - minY >= edges.Length)
                    {
                        continue;
                    }

                    edges[y - minY].Add(new Edge { X = xmin + invSlope * (y - ymin), InvSlope = invSlope });
                }
            }

            // Sort the edges and fill the path
            for (int y = minY; y <= maxY; y++)
            {
                var activeEdges = edges[y - minY];
                if (activeEdges.Count == 0)
                {
                    continue;
                }

                activeEdges.Sort((a, b) => a.X.CompareTo(b.X));

                for (int i = 0; i < activeEdges.Count; i += 2)
                {
                    int xStart = (int)Math.Round(activeEdges[i].X);
                    int xEnd = (int)Math.Round(activeEdges[i + 1].X);

                    for (int x = xStart; x < xEnd; x++)
                    {
                        DrawPixel(x, y, fillColor);
                    }
                }

                // Update the edges for the next scanline
                for (int i = 0; i < activeEdges.Count; i++)
                {
                    var edge = activeEdges[i];
                    edge.X += edge.InvSlope;
                    activeEdges[i] = edge;
                }
            }
        }

        struct Edge
        {
            public float X;
            public float InvSlope;
        }
    }
}