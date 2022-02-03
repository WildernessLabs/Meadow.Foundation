using Meadow.Units;

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
        public Angle Heading { get; set; }

        /// <summary>
        ///     Roll angle.
        /// </summary>
        public Angle Roll { get; set; }

        /// <summary>
        ///     Pitch angle.
        /// </summary>
        public Angle Pitch { get; set; }

        /// <summary>
        ///     Create a new EulerAngles object.
        /// </summary>
        /// <param name="heading">Heading reading.</param>
        /// <param name="roll">Roll angle.</param>
        /// <param name="pitch">Pitch angle.</param>
        public EulerAngles (Angle heading, Angle roll, Angle pitch)
        {
            Heading = heading;
            Roll = roll;
            Pitch = pitch;
        }
    }
}