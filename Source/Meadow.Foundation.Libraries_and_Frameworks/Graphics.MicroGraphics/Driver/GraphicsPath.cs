using System;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// The path direction
    /// </summary>
    public enum PathDirection
    {
        /// <summary>
        /// Clockwise
        /// </summary>
        Clockwise,
        /// <summary>
        /// Counter-clockwise
        /// </summary>
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

    /// <summary>
    /// Represents a 2D graphics paths
    /// </summary>
    public class GraphicsPath
    {
        /// <summary>
        /// The last point added to the path
        /// </summary>
        public Point LastPoint => PathActions.LastOrDefault().PathPoint;

        /// <summary>
        /// The number of points in th path
        /// </summary>
        public int PointCount => PathActions.Count;

        /// <summary>
        /// The collection of points 
        /// </summary>
        public Point[]? Points;

        /// <summary>
        /// The number of verbs/actions used
        /// </summary>
        public int VerbCount => PathActions.Count; //need to figure out if/when this wouldn't be equal to PointCount

        /// <summary>
        /// The collection of actions 
        /// </summary>
        internal List<PathAction> PathActions { get; private set; } = new List<PathAction>();

        /// <summary>
        /// A rect that defines the full bounds of the path
        /// </summary>
        public Rect Bounds
        {
            get
            {
                if (Points == null)
                {
                    return new Rect(0, 0, 0, 0);
                }

                Point min = Points[0];
                Point max = Points[0];

                foreach (var p in Points)
                {
                    min.X = Math.Min(min.X, p.X);
                    min.Y = Math.Min(min.Y, p.Y);
                    max.X = Math.Max(max.X, p.X);
                    max.Y = Math.Min(max.X, p.Y);
                }

                return new Rect(min.X, min.Y, max.X, max.Y);
            }
        }

        /// <summary>
        /// Create a new GraphicsPath object
        /// </summary>
        public GraphicsPath()
        { }

        /// <summary>
        /// Create a new GraphicsPath object
        /// </summary>
        /// <param name="path">Initial path data</param>
        public GraphicsPath(GraphicsPath path)
        {
            AddPath(path);
        }

        /// <summary>
        /// Reset the path
        /// </summary>
        public void Reset()
        {
            PathActions = new List<PathAction>();
        }

        /// <summary>
        /// Add a new point to the path
        /// </summary>
        /// <param name="x">The x position</param>
        /// <param name="y">The y position</param>
        public void MoveTo(int x, int y)
        {
            MoveTo(new Point(x, y));
        }

        /// <summary>
        /// Add a new point to the path
        /// </summary>
        /// <param name="point">The point position</param>
        public void MoveTo(Point point)
        {
            if (PathActions.Count > 0)
            {
                var last = GetLastAction();

                if (last.Verb == VerbType.Move)
                {
                    last.PathPoint = point;
                    return;
                }
            }

            PathActions.Add(new PathAction(point, VerbType.Move));
        }

        /// <summary>
        /// Add a new relative point to the path
        /// </summary>
        /// <param name="x">The relative x position</param>
        /// <param name="y">The relative y position</param>
        public void RelativeMoveTo(int x, int y)
        {
            int count = PathActions.Count;

            if (count > 0)
            {
                PathActions.Add(new PathAction(new Point(x, y) + PathActions[count - 1].PathPoint, VerbType.Move));
            }
            else
            {
                MoveTo(x, y);
            }
        }

        /// <summary>
        /// Add a new relative point to the path
        /// </summary>
        /// <param name="point">The relative point position</param>
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

        /// <summary>
        /// Add a line to the path
        /// </summary>
        /// <param name="x">The x line end location</param>
        /// <param name="y">The y line end location</param>
        public void LineTo(int x, int y)
        {
            if (PathActions.Count == 0)
            {
                MoveTo(x, y);
                return;
            }

            PathActions.Add(new PathAction(new Point(x, y), VerbType.Line));
        }

        /// <summary>
        /// Add a line to the path
        /// </summary>
        /// <param name="point">The point line end location</param>
        public void LineTo(Point point)
        {
            if (PathActions.Count == 0)
            {
                MoveTo(point);
                return;
            }

            PathActions.Add(new PathAction(point, VerbType.Line));
        }

        /// <summary>
        /// Add a line to the path
        /// </summary>
        /// <param name="x">The relative x line end location</param>
        /// <param name="y">The relative y line end location</param>
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

        /// <summary>
        /// Add a line to the path
        /// </summary>
        /// <param name="point">The relative point line end location</param>
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

        /// <summary>
        /// Add an array of lines to the path
        /// </summary>
        /// <param name="points">The points defining the lines to add</param>
        public void AddPolyLine(Point[] points)
        {
            foreach (var point in points)
            {
                LineTo(point);
            }
        }

        /// <summary>
        /// Add an existing path to the end of the path
        /// </summary>
        /// <param name="path">The path to add</param>
        public void AddPath(GraphicsPath path)
        {
            foreach (var action in path.PathActions)
            {
                PathActions.Add(action);
            }
        }

        /// <summary>
        /// Add an existing path to the end of the path in reverse order
        /// </summary>
        /// <param name="path">The path to add</param>
        public void AddPathReverse(GraphicsPath path)
        {
            for (int i = path.PathActions.Count - 1; i > 0; i--)
            {
                PathActions.Add(path.PathActions[i]);
            }
        }

        /// <summary>
        /// Close the path
        /// </summary>
        public void Close()
        {
            if (PathActions.Count == 0)
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

            if (action.Verb == VerbType.Close)
            {
                var index = PathActions.IndexOf(action);
                if (index < PathActions.Count - 1 && PathActions[index + 1].Verb == VerbType.Move)
                {
                    index++;
                }
                return PathActions[index];

            }
            return PathActions[0];
        }
    }
}