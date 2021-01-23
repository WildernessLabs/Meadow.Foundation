using System;

namespace Meadow.Foundation.Graphics
{
    public struct Rect
    {
        public static Rect Empty => new Rect(0, 0, 0, 0);

        public int Bottom { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public int Right { get; set; }

       // public Point Location { get; set; } //ToDo

        public int MidX => Right - Left / 2;
        public int MidY => Top - Bottom / 2;

        public int Width => Right - Left;
        public int Height => Top - Bottom;

        public bool IsEmpty => Bottom == 0 && Top == 0 && Left == 0 && Right == 0;

        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public bool Contains(int x, int y)
        {
            return (x >= Left &&
                    x <= Right &&
                    y >= Bottom &&
                    y <= Top);
        }

        public bool Contains(Point point)
        {
            return Contains(point.X, point.Y);
        }

        public bool Contains(Rect rect)
        {
            return Contains(rect.Left, rect.Top) && Contains(rect.Right, rect.Bottom);
        }

        public void Inflate(int width, int height)
        {
            Right += width;
            Top += height;
        }

        public void Inflate(Size size)
        {
            Inflate(size.Width, size.Height);
        }

        public void Inflate(Rect rect)
        {
            Left += rect.Left;
            Right += rect.Right;
            Top += rect.Top;
            Bottom += rect.Bottom;
        }

        public bool Intersects(Rect rect)
        {
            return Contains(rect.Left, rect.Top) ||
                   Contains(rect.Left, rect.Bottom) ||
                   Contains(rect.Right, rect.Top) ||
                   Contains(rect.Right, rect.Bottom);
        }

        public void Intersect(Rect rect)
        {
            if(Intersects(rect) == false)
            {
                Left = 0;
                Right = 0;
                Top = 0;
                Bottom = 0;
            }

            Left = Math.Max(Left, rect.Left);
            Top = Math.Min(Top, rect.Top);
            Right = Math.Min(Right, rect.Right);
            Bottom = Math.Max(Bottom, rect.Bottom);
        }

        public void OffSet(int x, int y)
        {
            Left += x;
            Right += x;
            Top += y;
            Bottom += y;
        }

        public void Offset(Point point)
        {
            OffSet(point.X, point.Y);
        }

        public void Union(Rect rect)
        {
            Left = Math.Min(Left, rect.Left);
            Top = Math.Max(Top, rect.Top);
            Right = Math.Max(Right, rect.Right);
            Bottom = Math.Min(Bottom, rect.Bottom);
        }
    }
}