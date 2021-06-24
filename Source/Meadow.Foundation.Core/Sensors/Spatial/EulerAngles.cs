namespace Meadow.Foundation.Spatial
{
    //TODO: are these in degrees?? should be unitized
    /// <summary>
    ///     Euler angles to define the orientation.
    /// </summary>
    public struct EulerAngles
    {
        /// <summary>
        ///     Heading.
        /// </summary>
        public double Heading { get; set; }

        /// <summary>
        ///     Roll angle.
        /// </summary>
        public double Roll { get; set; }

        /// <summary>
        ///     Pitch angle.
        /// </summary>
        public double Pitch { get; set; }

        /// <summary>
        ///     Create a new EulerAngles object.
        /// </summary>
        /// <param name="heading">Heading reading.</param>
        /// <param name="roll">Roll angle.</param>
        /// <param name="pitch">Pitch angle.</param>
        public EulerAngles (double heading, double roll, double pitch)
        {
            Heading = heading;
            Roll = roll;
            Pitch = pitch;
        }
    }
}