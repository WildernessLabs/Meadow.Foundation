using System;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Represents a 3D integer value
    /// </summary>
    public struct Point3d
    {
        /// <summary>
        /// Create an empty / zero Point3d struct
        /// </summary>
        public static Point3d Empty => new Point3d(0, 0, 0);

        /// <summary>
        /// The X value
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Y value
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// The Z value
        /// </summary>
        public int Z { get; set; }

        /// <summary>
        /// Length
        /// </summary>
        public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

        /// <summary>
        /// Is the struct empty / zero?
        /// </summary>
        public bool IsEmpty => X == 0 && Y == 0;

        /// <summary>
        /// Create a new Point3d struct
        /// </summary>
        /// <param name="x">X value</param>
        /// <param name="y">Y value</param>
        /// <param name="z">Z value</param>
        public Point3d(int x = 0, int y = 0, int z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Offset the Point3d values
        /// </summary>
        /// <param name="x">X offset amount</param>
        /// <param name="y">Y offset amount</param>
        /// <param name="z">Z offset amount</param>
        public void Offset(int x, int y, int z)
        {
            X += x;
            Y += y;
            Z += z;
        }

        /// <summary>
        /// Offset the current value
        /// </summary>
        /// <param name="point3d">The point3d value to offset</param>
        public void Offset(Point3d point3d)
        {
            X += point3d.X;
            Y += point3d.Y;
            Z += point3d.Z;
        }

        /// <summary>
        /// Translates the specified Point by the specified Size.
        /// </summary>
        /// <param name="point3d">
        /// The <see cref="Point3d"/> instance to translate.
        /// </param>
        /// <param name="amount3d">
        /// The <see cref="Point3d"/> instance to translate point with.
        /// </param>
        /// <returns>
        /// A new <see cref="Point3d"/> instance translated by size.
        /// </returns>
        public static Point3d operator +(Point3d point3d, Point3d amount3d)
        {
            return new Point3d(point3d.X + amount3d.X, point3d.Y + amount3d.Y, point3d.Z + amount3d.Z);
        }

        /// <summary>
        /// Subtract the specified Point by another Point3d values
        /// </summary>
        /// <param name="point3d">The Point3d instance to translate</param>
        /// <param name="amount3d">The amount to subtract</param>
        /// <returns>A new Point3d instance translated by the amount</returns>
        public static Point3d operator -(Point3d point3d, Point3d amount3d)
        {
            return new Point3d(point3d.X - amount3d.X, point3d.Y - amount3d.Y, point3d.Z - amount3d.Z);
        }

        /// <summary>
        /// Compares two instances for equality
        /// </summary>
        /// <param name="left">The first instance</param>
        /// <param name="right">The second instance</param>
        /// <returns>True if left is equal to right, false otherwise</returns>
        public static bool operator ==(Point3d left, Point3d right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality
        /// </summary>
        /// <param name="left">The first instance</param>
        /// <param name="right">The second instance</param>
        /// <returns>True if left is not equal to right, false otherwise</returns>
        public static bool operator !=(Point3d left, Point3d right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this instance is equal to the specified object
        /// </summary>
        /// <param name="obj">The object instance to compare to</param>
        /// <returns>True if both instances are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            if (obj is Point3d)
            {
                return Equals((Point3d)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance
        /// </summary>
        /// <returns>An int that represents the hash code for this instance</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        /// <summary>
        /// String representation of the Point3d values
        /// </summary>
        /// <returns>The string</returns>
        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Z: {Z}";
        }
    }
}