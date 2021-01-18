using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Graphics
{
    public class GraphicsPath
    {
        List<Point> points = new List<Point>();

        public List<Point> Points => points; //can we do this via the indexer?

        public int PointCount => points.Count;

        // Indexer declaration
        public Point this[int index]
        {
            get => points[index];
            set => points[index] = value;
        }

        public GraphicsPath()
        {
        }

        public GraphicsPath(Point[] points)
        {
            AddPoints(points);
        }

        public void AddPoint(int x, int y)
        {
            points.Add(new Point(x, y));
        }

        public void AddPoint(Point point)
        {
            points.Add(point);
        }

        public void AddPoints(Point[] points)
        {
            foreach (var p in points)
            {
                this.points.Add(p);
            }
        }

        public void AddPath(GraphicsPath path)
        {
            AddPoints(path.Points.ToArray());
        }

        public void ClosePath()
        {
            if(points.Count < 2)
            {
                return;
            }

            if(points[0] != points.Last())
            {
                points.Add(points[0]);
            }
        }
    }
}