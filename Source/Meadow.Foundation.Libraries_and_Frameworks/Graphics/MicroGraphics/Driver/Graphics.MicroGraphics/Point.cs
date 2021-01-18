namespace MicroGraphics
{
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public bool IsEmpty => X == 0 && Y == 0;

        public Point (int x = 0, int y = 0)
        {
            X = x;
            Y = y;
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

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }
    }
}