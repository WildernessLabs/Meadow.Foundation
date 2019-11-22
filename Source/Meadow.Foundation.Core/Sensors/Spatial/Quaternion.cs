namespace Meadow.Foundation.Spatial
{
    /// <summary>
    ///     Quaternion coordinates object.
    /// </summary>
    public class Quaternion
    {
        /// <summary>
        ///     W component of the quaternion.
        /// </summary>
        public double W { get; private set; }

        /// <summary>
        ///     X component of the quaternion.
        /// </summary>
        public double X { get; private set; }

        /// <summary>
        ///     Y component of the quaternion.
        /// </summary>
        public double Y { get; private set; }

        /// <summary>
        ///     Z componet of the quaternion.
        /// </summary>
        public double Z { get; private set; }

        /// <summary>
        ///     Create a new Quaternion object setting the relevant properties.
        /// </summary>
        /// <param name="w"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public Quaternion(double w, double x, double y, double z)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }
    }
}