using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Graphics
{
    public enum PathDirection
    {
        Clockwise,
        CounterClockwise
    }

    //https://api.skia.org/classSkPath.html#ac36f638ac96f3428626e993eacf84ff0ab2c02031eada4693dcf0f0724aec22a6
    internal enum VerbType //from Skia, could change
    {
        Move,
        Line,
        Close,
    }

    internal struct PathAction
    {
        public Point PathPoint { get; internal set; }
        public VerbType Verb { get; internal set; }

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
            MoveTo(new Point(x, y));
        }

        public void MoveTo(Point point)
        {
            if(PathActions.Count > 0)
            {
                var last = GetLastAction();

                if(last.Verb == VerbType.Move)
                {
                    last.PathPoint = point;
                    return;
                }
            }

            PathActions.Add(new PathAction(point, VerbType.Move));
        }

        public void RelativeMoveTo(int x, int y)
        {
            int count = PathActions.Count;

            if(count > 0)
            {
                PathActions.Add(new PathAction(new Point(x, y) + PathActions[count - 1].PathPoint, VerbType.Move));
            }
            else
            {
                MoveTo(x, y);
            }
        }

        public void RelativeMoveTo(Point point)
        {
            int count = PathActions.Count;

            if (count > 0)
            {
                PathActions.Add(new PathAction(point + PathActions[count - 1].PathPoint, VerbType.Move));
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

            PathActions.Add(new PathAction(new Point(x, y), VerbType.Line));
        }

        public void LineTo(Point point)
        {
            if (PathActions.Count == 0)
            {
                MoveTo(point);
                return;
            }

            PathActions.Add(new PathAction(point, VerbType.Line));
        }

        public void RelativeLineTo(int x, int y)
        {
            int count = PathActions.Count;

            if (count > 0)
            {
                PathActions.Add(new PathAction(new Point(x, y) + PathActions[count - 1].PathPoint, VerbType.Line));
            }
            else
            {
                LineTo(x, y);
            }
        }

        public void RelativeLineTo(Point point)
        {
            int count = PathActions.Count;

            if (count > 0)
            {
                PathActions.Add(new PathAction(point + PathActions[count - 1].PathPoint, VerbType.Line));
            }
            else
            {
                LineTo(point);
            }
        }

        public void AddPolyLine(Point[] points)
        {
            foreach(var point in points)
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
            if(PathActions.Count == 0)
            {
                return;
            }

            PathActions.Add(new PathAction(GetPathStart().PathPoint, VerbType.Close));
        }

        PathAction GetLastAction()
        {
            return PathActions.Last();
        }

        PathAction GetPathStart()
        {
            var action = PathActions.Where(p => p.Verb == VerbType.Close).LastOrDefault();

            if(action.Verb == VerbType.Close)
            {
                var index = PathActions.IndexOf(action);
                if(index < PathActions.Count - 1 && PathActions[index +1].Verb == VerbType.Move)
                {
                    index++;
                }
                return PathActions[index];

            }
            return PathActions[0];
        }
    }
}