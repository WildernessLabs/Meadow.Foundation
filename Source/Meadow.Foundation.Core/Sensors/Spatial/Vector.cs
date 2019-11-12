namespace Meadow.Foundation.Spatial
{
    /// <summary>
    ///     Vector in three dimensional space.
    /// </summary>
    public class Vector
    {
        /// <summary>
        ///     X component of the vector.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        ///     Y component of the vector.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        ///     Z component of the vector.
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        ///     Create a new Vector object with the specified X, y & Z components.
        /// </summary>
        /// <param name="x">X component of the vector.</param>
        /// <param name="y">Y component of the vector.</param>
        /// <param name="z">Z component of the vector.</param>
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}