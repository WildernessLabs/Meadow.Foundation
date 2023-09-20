using System;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Represents a integer based rectangle
    /// </summary>
    public struct Rect
    {
        /// <summary>
        /// Create an empty / zero rect
        /// </summary>
        public static Rect Empty => new Rect(0, 0, 0, 0);

        /// <summary>
        /// The bottom rect value
        /// </summary>
        public int Bottom { get; set; }

        /// <summary>
        /// The top rect value
        /// </summary>
        public int Top { get; set; }

        /// <summary>
        /// The left rect value
        /// </summary>
        public int Left { get; set; }

        /// <summary>
        /// The right rect value
        /// </summary>
        public int Right { get; set; }

        /// <summary>
        /// The x mid value
        /// </summary>
        public int MidX => Right - Left / 2;

        /// <summary>
        /// The y mid value
        /// </summary>
        public int MidY => Bottom - Top / 2;

        /// <summary>
        /// The rect width
        /// </summary>
        public int Width => Right - Left;

        /// <summary>
        /// The rect height
        /// </summary>
        public int Height => Bottom - Top;

        /// <summary>
        /// Is the rect empty / zero
        /// </summary>
        public bool IsEmpty => Bottom == 0 && Top == 0 && Left == 0 && Right == 0;

        /// <summary>
        /// Create a new rect struct with inital values
        /// </summary>
        /// <param name="left">Left value</param>
        /// <param name="top">Top value</param>
        /// <param name="right">Right value</param>
        /// <param name="bottom">Bottom value</param>
        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        /// <summary>
        /// Is an x,y coordinate within the rect
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <returns>True if the corrindate is within the rect</returns>
        public bool Contains(int x, int y)
        {
            return (x >= Left &&
                    x <= Right &&
                    y >= Bottom &&
                    y <= Top);
        }

        /// <summary>
        /// Is a point coordinate within the rect
        /// </summary>
        /// <param name="point">The point</param>
        /// <returns>True if the point is within the rect</returns>
        public bool Contains(Point point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>
        /// Is another rect within this rect
        /// </summary>
        /// <param name="rect">The rect to compare</param>
        /// <returns>True if the rect is fully contained</returns>
        public bool Contains(Rect rect)
        {
            return Contains(rect.Left, rect.Top) && Contains(rect.Right, rect.Bottom);
        }

        /// <summary>
        /// Increase the size in both dimensions
        /// This makes the rect wider by increasign the right value
        /// And taller by increasing the top value
        /// </summary>
        /// <param name="width">The amount to increase horizontally (right)</param>
        /// <param name="height">The amount to increase vertically (top)</param>
        public void Inflate(int width, int height)
        {
            Right += width;
            Top += height;
        }

        /// <summary>
        /// Increase the size in both dimensions
        /// This makes the rect wider by increasing the right value
        /// And taller by increasing the top value
        /// </summary>
        /// <param name="size">The amount to increase</param>
        public void Inflate(Size size)
        {
            Inflate(size.Width, size.Height);
        }

        /// <summary>
        /// Increase the size in all directions with values from another rect
        /// </summary>
        /// <param name="rect">The rect values to inflate</param>
        public void Inflate(Rect rect)
        {
            Left += rect.Left;
            Right += rect.Right;
            Top += rect.Top;
            Bottom += rect.Bottom;
        }

        /// <summary>
        /// Does a rect intersect with this rect
        /// </summary>
        /// <param name="rect">True if the rects overlap any amount</param>
        /// <returns></returns>
        public bool Intersects(Rect rect)
        {
            return Contains(rect.Left, rect.Top) ||
                   Contains(rect.Left, rect.Bottom) ||
                   Contains(rect.Right, rect.Top) ||
                   Contains(rect.Right, rect.Bottom);
        }
        
        /// <summary>
        /// Combine two rects (take the minimum values in all directions)
        /// </summary>
        /// <param name="rect">The rect to intersect</param>
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

        /// <summary>
        /// Offset the rect
        /// </summary>
        /// <param name="x">The x amount to offset</param>
        /// <param name="y">The y amount to offset</param>
        public void OffSet(int x, int y)
        {
            Left += x;
            Right += x;
            Top += y;
            Bottom += y;
        }

        /// <summary>
        /// Offset the rect
        /// </summary>
        /// <param name="point">The point values to offset</param>
        public void Offset(Point point)
        {
            OffSet(point.X, point.Y);
        }

        /// <summary>
        /// Union two rects (take the maximum values in all directions)
        /// </summary>
        /// <param name="rect">The rect to union</param>
        public void Union(Rect rect)
        {
            Left = Math.Min(Left, rect.Left);
            Top = Math.Max(Top, rect.Top);
            Right = Math.Max(Right, rect.Right);
            Bottom = Math.Min(Bottom, rect.Bottom);
        }

        /// <summary>
        /// Add two rects
        /// </summary>
        /// <param name="rect">The rect</param>
        /// <param name="amount">The amount to add</param>
        /// <returns></returns>
        public static Rect operator +(Rect rect, Rect amount)
        {
            return new Rect(rect.Left + amount.Left,
                rect.Top + amount.Top,
                rect.Right + amount.Right,
                rect.Bottom + amount.Bottom);
        }

        /// <summary>
        /// Subtract two rects
        /// </summary>
        /// <param name="rect">The rect</param>
        /// <param name="amount">The amount to subtract</param>
        /// <returns></returns>
        public static Rect operator -(Rect rect, Rect amount)
        {
            return new Rect(rect.Left - amount.Left,
                rect.Top - amount.Top,
                rect.Right - amount.Right,
                rect.Bottom - amount.Bottom);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left is equal to right; false otherwise.</returns>
        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left is not equal to right; false otherwise.</returns>
        public static bool operator !=(Rect left, Rect right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this instance is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object instance to compare to.</param>
        /// <returns>True, if both instances are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Rect)
            {
                return Equals((Rect)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A <see cref="System.Int32"/> that represents the hash code for this instance./></returns>
        public override int GetHashCode()
        {
            return Left.GetHashCode() ^ Top.GetHashCode() ^ Right.GetHashCode() ^ Bottom.GetHashCode();
        }

        /// <summary>
        /// Get a string represention of the rect values
        /// </summary>
        /// <returns>The string with left, top, right and bottom values</returns>
        public override string ToString()
        {
            return $"Left: {Left}, Top: {Top}, Right: {Right}, Bottom {Bottom}";
        }
    }
}