namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Represents a 2D point 
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Get an empty / zero Point
        /// </summary>
        public static Point Empty => new Point(0, 0);

        /// <summary>
        /// The X value
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Y value
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Is the point empty or 0
        /// </summary>
        public bool IsEmpty => X == 0 && Y == 0;

        /// <summary>
        /// Create a new Point struct
        /// </summary>
        /// <param name="x">The x value</param>
        /// <param name="y">The y value</param>
        public Point(int x = 0, int y = 0)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Create a Point from a size
        /// </summary>
        /// <param name="size">The size</param>
        /// <returns>A new Point who's x value is the width and y value is the height</returns>
        public static Point From(Size size)
        {
            return new Point(size.Width, size.Height);
        }

        /// <summary>
        /// Offset the point
        /// </summary>
        /// <param name="x">The x amount to offset</param>
        /// <param name="y">The y amount to offset</param>
        public void Offset(int x, int y)
        {
            X += x;
            Y += y;
        }

        /// <summary>
        /// Offset the point
        /// </summary>
        /// <param name="point">A Point containing the amount to offset</param>
        public void Offset(Point point)
        {
            X += point.X;
            Y += point.Y;
        }

        /// <summary>
        /// Translates the specified Point by the specified amount for both x and y
        /// </summary>
        /// <param name="point">The Point instance to translate</param>
        /// <param name="amount">The amount to translate</param>
        /// <returns>A new Point instance translated by size</returns>
        public static Point operator +(Point point, Point amount)
        {
            return new Point(point.X + amount.X, point.Y + amount.Y);
        }

        /// <summary>
        /// Translates the specified Point by the negative of the specified Size.
        /// </summary>
        /// <param name="point">The Point instance to translate</param>
        /// <param name="amount">Point amount to subtract</param>
        /// <returns>A new Point instance translated by size</returns>
        public static Point operator -(Point point, Point amount)
        {
            return new Point(point.X - amount.X, point.Y - amount.Y);
        }

        /// <summary>
        /// Compares two instances for equality
        /// </summary>
        /// <param name="left">The first instance</param>
        /// <param name="right">The second instance</param>
        /// <returns>True if left is equal to right otherwise false</returns>
        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality
        /// </summary>
        /// <param name="left">The first instance</param>
        /// <param name="right">The second instance</param>
        /// <returns>True if left is not equal to right otherwise false</returns>
        public static bool operator !=(Point left, Point right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this instance is equal to the specified object
        /// </summary>
        /// <param name="obj">The object instance to compare to</param>
        /// <returns>True if both instances are equal otherwise false</returns>
        public override bool Equals(object obj)
        {
            if (obj is Point)
            {
                return Equals((Point)obj);
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
        /// Get a string representation of the point
        /// </summary>
        /// <returns>The x and y values as a string</returns>
        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }
    }
}