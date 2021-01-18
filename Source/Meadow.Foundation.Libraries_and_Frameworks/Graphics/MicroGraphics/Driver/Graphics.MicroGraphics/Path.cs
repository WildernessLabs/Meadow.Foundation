using System.Collections.Generic;
using System.Linq;

namespace MicroGraphics
{
    public class Path
    {
        List<Point> points = new List<Point>();

        public List<Point> Points => points;

        public Path()
        {
        }

        public Path(Point[] points)
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

        public void AddPath(Path path)
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