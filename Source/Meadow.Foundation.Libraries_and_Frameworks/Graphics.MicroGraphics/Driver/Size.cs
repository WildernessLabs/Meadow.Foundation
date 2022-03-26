namespace Meadow.Foundation.Graphics
{
    public struct Size
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public bool IsEmpty => Width == 0 && Height == 0;

        public static Size Empty => new Size(0, 0);

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public Size(Point point) :
            this(point.X, point.Y)
        {

        }

        public static Size From(Point point)
        {
            return new Size(point.X, point.Y);
        }

        public static Size operator +(Size size, Size amount)
        {
            return new Size(size.Width + amount.Width, size.Height + amount.Height);
        }


        public static Size operator -(Size size, Size amount)
        {
            return new Size(size.Width - amount.Width, size.Height - amount.Height);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left is equal to right; false otherwise.</returns>
        public static bool operator ==(Size left, Size right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left is not equal to right; false otherwise.</returns>
        public static bool operator !=(Size left, Size right)
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
            if (obj is Size)
            {
                return Equals((Size)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A <see cref="System.Int32"/> that represents the hash code for this instance./></returns>
        public override int GetHashCode()
        {
            return Width.GetHashCode() ^ Height.GetHashCode();
        }

        public override string ToString()
        {
            return $"Width: {Width}, Height: {Height}";
        }
    }
}
