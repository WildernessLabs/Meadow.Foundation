using System.Diagnostics.Contracts;

namespace Meadow.Foundation.Spatial
{
    /// <summary>
    ///     Vector in three dimensional space.
    /// </summary>
    public struct Vector
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
        ///     Create a new Vector object with the specified X, Y and Z components.
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

        /// <summary>
        /// Vector addition operator
        /// </summary>
        /// <param name="lvalue">left value</param>
        /// <param name="rvalue">right value</param>
        /// <returns></returns>
        [Pure]
        public static Vector operator +(Vector lvalue, Vector rvalue)
        {
            var x = lvalue.X + rvalue.X;
            var y = lvalue.Y + rvalue.Y;
            var z = lvalue.Z + rvalue.Z;

            return new Vector(x, y, z);
        }

        /// <summary>
        /// Vector subtraction operator
        /// </summary>
        /// <param name="lvalue">left value</param>
        /// <param name="rvalue">right value</param>
        /// <returns></returns>

        [Pure]
        public static Vector operator -(Vector lvalue, Vector rvalue)
        {
            var x = lvalue.X - rvalue.X;
            var y = lvalue.Y - rvalue.Y;
            var z = lvalue.Z - rvalue.Z;

            return new Vector(x, y, z);
        }


    }
}