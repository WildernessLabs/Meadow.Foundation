namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// Represents a 2d size
    /// </summary>
    public struct Size
    {
        /// <summary>
        /// The width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Is the size empty / zero
        /// </summary>
        public bool IsEmpty => Width == 0 && Height == 0;

        /// <summary>
        /// Get an empty / zero size
        /// </summary>
        public static Size Empty => new Size(0, 0);

        /// <summary>
        /// Create a new size struct
        /// </summary>
        /// <param name="width">The initial width</param>
        /// <param name="height">The initial height</param>
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Create a new size struct
        /// </summary>
        /// <param name="point">Point values to set the width (x) and height (y)</param>
        public Size(Point point) :
            this(point.X, point.Y)
        { }

        /// <summary>
        /// Get a size from a point
        /// </summary>
        /// <param name="point">The point</param>
        /// <returns>A new Size struct</returns>
        public static Size From(Point point)
        {
            return new Size(point.X, point.Y);
        }

        /// <summary>
        /// Add an amount to a Size in both dimensions
        /// </summary>
        /// <param name="size">The Size struct</param>
        /// <param name="amount">The amount to add</param>
        /// <returns>A new Size struct</returns>
        public static Size operator +(Size size, Size amount)
        {
            return new Size(size.Width + amount.Width, size.Height + amount.Height);
        }

        /// <summary>
        /// Subtract an amount from a Size in both dimensions
        /// </summary>
        /// <param name="size">The Size struct</param>
        /// <param name="amount">The amount to subtract</param>
        /// <returns>A new Size struct</returns>
        public static Size operator -(Size size, Size amount)
        {
            return new Size(size.Width - amount.Width, size.Height - amount.Height);
        }

        /// <summary>
        /// Compares two instances for equality
        /// </summary>
        /// <param name="left">The first instance</param>
        /// <param name="right">The second instance</param>
        /// <returns>True if left is equal to right, false otherwise</returns>
        public static bool operator ==(Size left, Size right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality
        /// </summary>
        /// <param name="left">The first instance</param>
        /// <param name="right">The second instance</param>
        /// <returns>True if left is not equal to right, false otherwise</returns>
        public static bool operator !=(Size left, Size right)
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
            if (obj is Size)
            {
                return Equals((Size)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance
        /// </summary>
        /// <returns>An int that represents the hash code for this instance</returns>
        public override int GetHashCode()
        {
            return Width.GetHashCode() ^ Height.GetHashCode();
        }

        /// <summary>
        /// Get a string representation of the Size
        /// </summary>
        /// <returns>A string with the Width and Height</returns>
        public override string ToString()
        {
            return $"Width: {Width}, Height: {Height}";
        }
    }
}
