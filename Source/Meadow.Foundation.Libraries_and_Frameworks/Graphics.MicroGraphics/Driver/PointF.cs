using System.Drawing;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Represents a 2D point with floating-point coordinates
    /// </summary>
    public struct PointF
    {
        /// <summary>
        /// Gets an empty point at (0, 0)
        /// </summary>
        public static PointF Empty => new(0f, 0f);

        /// <summary>
        /// The X value
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// The Y value
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Indicates whether the point is at (0, 0)
        /// </summary>
        public bool IsEmpty => X == 0f && Y == 0f;

        /// <summary>
        /// Creates a new PointF struct
        /// </summary>
        /// <param name="x">The X value</param>
        /// <param name="y">The Y value</param>
        public PointF(float x = 0f, float y = 0f)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Creates a PointF from a SizeF
        /// </summary>
        /// <param name="size">The SizeF instance</param>
        /// <returns>A new PointF with X and Y from the SizeF</returns>
        public static PointF From(SizeF size)
        {
            return new PointF(size.Width, size.Height);
        }

        /// <summary>
        /// Offsets the point by specified amounts
        /// </summary>
        /// <param name="x">The amount to offset X</param>
        /// <param name="y">The amount to offset Y</param>
        public void Offset(float x, float y)
        {
            X += x;
            Y += y;
        }

        /// <summary>
        /// Offsets the point by another PointF
        /// </summary>
        /// <param name="point">The PointF to offset by</param>
        public void Offset(PointF point)
        {
            X += point.X;
            Y += point.Y;
        }

        /// <summary>
        /// Adds two PointF instances
        /// </summary>
        public static PointF operator +(PointF point, PointF amount)
        {
            return new PointF(point.X + amount.X, point.Y + amount.Y);
        }

        /// <summary>
        /// Subtracts one PointF from another
        /// </summary>
        public static PointF operator -(PointF point, PointF amount)
        {
            return new PointF(point.X - amount.X, point.Y - amount.Y);
        }

        /// <summary>
        /// Compares two PointF instances for equality
        /// </summary>
        public static bool operator ==(PointF left, PointF right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two PointF instances for inequality
        /// </summary>
        public static bool operator !=(PointF left, PointF right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Checks if this instance is equal to another object
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is PointF point)
            {
                return X == point.X && Y == point.Y;
            }
            return false;
        }

        /// <summary>
        /// Gets the hash code for this instance
        /// </summary>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        /// <summary>
        /// Returns a string representation of the point
        /// </summary>
        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }
    }
}