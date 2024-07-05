using Meadow.Units;
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

    internal enum VerbType
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
    /// Represents a 2D graphics path
    /// </summary>
    public class GraphicsPath
    {
        /// <summary>
        /// The last point added to the path
        /// </summary>
        public Point LastPoint => PathActions.LastOrDefault().PathPoint;

        /// <summary>
        /// The number of points in the path
        /// </summary>
        public int PointCount => PathActions.Count;

        /// <summary>
        /// The collection of points 
        /// </summary>
        public Point[] Points { get; private set; }

        /// <summary>
        /// The number of verbs/actions used
        /// </summary>
        public int VerbCount => PathActions.Count;

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
                if (PathActions == null || PathActions.Count == 0)
                {
                    return new Rect(0, 0, 0, 0);
                }

                Point min = PathActions[0].PathPoint;
                Point max = PathActions[0].PathPoint;

                foreach (var action in PathActions)
                {
                    min.X = Math.Min(min.X, action.PathPoint.X);
                    min.Y = Math.Min(min.Y, action.PathPoint.Y);
                    max.X = Math.Max(max.X, action.PathPoint.X);
                    max.Y = Math.Max(max.Y, action.PathPoint.Y);
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
            PathActions.Clear();
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
        /// Scales the path by the specified factors in the X and Y directions.
        /// </summary>
        /// <param name="scaleX">The scale factor in the X direction.</param>
        /// <param name="scaleY">The scale factor in the Y direction.</param>
        public void Scale(float scaleX, float scaleY)
        {
            for (int i = 0; i < PathActions.Count; i++)
            {
                var action = PathActions[i];
                action.PathPoint = new Point((int)(action.PathPoint.X * scaleX), (int)(action.PathPoint.Y * scaleY));
                PathActions[i] = action;
            }
        }

        /// <summary>
        /// Translates the path by the specified amounts in the X and Y directions.
        /// </summary>
        /// <param name="dx">The amount to translate in the X direction.</param>
        /// <param name="dy">The amount to translate in the Y direction.</param>
        public void Translate(float dx, float dy)
        {
            for (int i = 0; i < PathActions.Count; i++)
            {
                var action = PathActions[i];
                action.PathPoint = new Point((int)(action.PathPoint.X + dx), (int)(action.PathPoint.Y + dy));
                PathActions[i] = action;
            }
        }

        /// <summary>
        /// Rotates the path by the specified angle.
        /// </summary>
        /// <param name="angle">The angle to rotate the path, in degrees.</param>
        public void Rotate(Angle angle)
        {
            float radians = (float)angle.Radians;
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            for (int i = 0; i < PathActions.Count; i++)
            {
                var action = PathActions[i];
                float x = action.PathPoint.X;
                float y = action.PathPoint.Y;
                action.PathPoint = new Point(
                    (int)(x * cos - y * sin),
                    (int)(x * sin + y * cos));
                PathActions[i] = action;
            }
        }

        /// <summary>
        /// Determines if a point lies on the path within a specified tolerance.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="tolerance">The tolerance within which the point is considered to be on the path.</param>
        /// <returns>True if the point is on the path; otherwise, false.</returns>
        public bool IsPointOnPath(Point point, float tolerance = 0.5f)
        {
            foreach (var action in PathActions)
            {
                if (MathF.Abs(action.PathPoint.X - point.X) <= tolerance && MathF.Abs(action.PathPoint.Y - point.Y) <= tolerance)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Clips the path to a specified rectangular area.
        /// </summary>
        /// <param name="clipRect">The rectangle to which the path will be clipped.</param>
        public void Clip(Rect clipRect)
        {
            PathActions = PathActions.Where(action =>
                action.PathPoint.X >= clipRect.Left && action.PathPoint.X <= clipRect.Right &&
                action.PathPoint.Y >= clipRect.Top && action.PathPoint.Y <= clipRect.Bottom).ToList();
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
