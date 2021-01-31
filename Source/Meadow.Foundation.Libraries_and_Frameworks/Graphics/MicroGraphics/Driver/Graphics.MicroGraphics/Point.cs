using System;

namespace Meadow.Foundation.Graphics
{
    public struct Point
    {
        public static Point Empty => new Point(0, 0);

        public int X { get; set; }
        public int Y { get; set; }

        public double Length => Math.Sqrt(X * X + Y * Y);

        public bool IsEmpty => X == 0 && Y == 0;



        public Point (int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        public static Point From(Size size)
        {
            return new Point(size.Width, size.Height);
        }

        public void Offset(int x, int y)
        {
            X += x;
            Y += y;
        }

        public void Offset(Point point)
        {
            X += point.X;
            Y += point.Y;
        }

        /// <summary>
        /// Translates the specified Point by the specified Size.
        /// </summary>
        /// <param name="point">
        /// The <see cref="Point"/> instance to translate.
        /// </param>
        /// <param name="size">
        /// The <see cref="Size"/> instance to translate point with.
        /// </param>
        /// <returns>
        /// A new <see cref="Point"/> instance translated by size.
        /// </returns>
        public static Point operator +(Point point, Point amount)
        {
            return new Point(point.X + amount.X, point.Y + amount.Y);
        }

        /// <summary>
        /// Translates the specified Point by the negative of the specified Size.
        /// </summary>
        /// <param name="point">
        /// The <see cref="Point"/> instance to translate.
        /// </param>
        /// <param name="size">
        /// The <see cref="Size"/> instance to translate point with.
        /// </param>
        /// <returns>
        /// A new <see cref="Point"/> instance translated by size.
        /// </returns>
        public static Point operator -(Point point, Point amount)
        {
            return new Point(point.X - amount.X, point.Y - amount.Y);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left is equal to right; false otherwise.</returns>
        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left is not equal to right; false otherwise.</returns>
        public static bool operator !=(Point left, Point right)
        {
            return !left.Equals(right);
        }

        // <summary>
        /// Indicates whether this instance is equal to the specified object.
        /// </summary>
        /// <param name="obj">The object instance to compare to.</param>
        /// <returns>True, if both instances are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj is Point)
            {
                return Equals((Point)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A <see cref="System.Int32"/> that represents the hash code for this instance./></returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }
    }
}