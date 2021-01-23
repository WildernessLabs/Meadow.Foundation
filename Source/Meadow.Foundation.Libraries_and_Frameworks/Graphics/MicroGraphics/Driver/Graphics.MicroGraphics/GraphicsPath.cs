using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Graphics
{
    internal enum VerbType //from Skia, could change
    {
        MoveTo,
        LineTo,
        QuadrantTo, //quarter circle
    }

    internal struct PathAction
    {
        public Point PathPoint { get; private set; }
        public VerbType Verb { get; private set; }

        public PathAction(Point pathPoint, VerbType verb)
        {
            PathPoint = pathPoint;
            Verb = verb;
        }
    }

    public class GraphicsPath
    {
        public Point LastPoint => PathActions.LastOrDefault().PathPoint;
        public int PointCount => PathActions.Count;
        public Point[] Points;
        public int VerbCount => PathActions.Count; //need to figure out if/when this wouldn't be equal to PointCount

        internal List<PathAction> PathActions { get; private set; } = new List<PathAction>();

        public GraphicsPath()
        {
        }

        public GraphicsPath(GraphicsPath path)
        {
            AddPath(path);
        }

        public void Reset()
        {
            PathActions = new List<PathAction>();
        }

        public void MoveTo(int x, int y)
        {
            PathActions.Add(new PathAction(new Point(x, y), VerbType.MoveTo));
        }

        public void MoveTo(Point point)
        {
            PathActions.Add(new PathAction(point, VerbType.MoveTo));
        }

        public void RMoveTo(int x, int y)
        {
            int count = PathActions.Count;

            if(count > 0)
            {
                PathActions.Add(new PathAction(new Point(x, y) + PathActions[count - 1].PathPoint, VerbType.MoveTo);
            }
            else
            {
                MoveTo(x, y);
            }
        }

        public void RMoveTo(Point point)
        {
            int count = PathActions.Count;

            if (count > 0)
            {
                PathActions.Add(new PathAction(point + PathActions[count - 1].PathPoint, VerbType.MoveTo);
            }
            else
            {
                MoveTo(point);
            }
        }

        public void LineTo(int x, int y)
        {
            if(PathActions.Count == 0)
            {
                MoveTo(x, y);
                return;
            }

            PathActions.Add(new PathAction(new Point(x, y), VerbType.LineTo));
        }

        public void LineTo(Point point)
        {
            if (PathActions.Count == 0)
            {
                MoveTo(point);
                return;
            }

            PathActions.Add(new PathAction(point, VerbType.LineTo));
        }

        public void RLineTo(int x, int y)
        {
            int count = PathActions.Count;

            if (count > 0)
            {
                PathActions.Add(new PathAction(new Point(x, y) + PathActions[count - 1].PathPoint, VerbType.LineTo);
            }
            else
            {
                LineTo(x, y);
            }
        }

        public void RLineTo(Point point)
        {
            int count = PathActions.Count;

            if (count > 0)
            {
                PathActions.Add(new PathAction(point + PathActions[count - 1].PathPoint, VerbType.LineTo);
            }
            else
            {
                LineTo(point);
            }
        }

        public void AddPath(GraphicsPath path)
        {
            foreach(var action in path.PathActions)
            {
                PathActions.Add(action);
            }
        }

        public void AddPathReverse(GraphicsPath path)
        {
            for(int i = path.PathActions.Count - 1; i > 0; i--)
            {
                PathActions.Add(path.PathActions[i]);
            }
        }

        public void Close()
        {
            if(PathActions.Count < 2)
            {
                return;
            }

            //need to make this dynamic - i.e. from the last MoveTo
            if(PathActions[0].PathPoint != PathActions.Last().PathPoint)
            {
                PathActions.Add(PathActions[0]);
            }
        }
    }
}