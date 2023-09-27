using Meadow.Units;

namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Servo configuration class used to store servo properties 
    /// </summary>
    public class ServoConfig
    {
        /// <summary>
        /// The servo's minimum angle
        /// </summary>
        public Angle MinimumAngle { get; private set; }

        /// <summary>
        /// The servo's maximum angle
        /// </summary>
        public Angle MaximumAngle { get; private set; } // -1 for continuous rotation

        /// <summary>
        /// The servo's minimum pulse duration
        /// </summary>
        public int MinimumPulseDuration { get; private set; }

        /// <summary>
        /// The servo's maximum pulse duration
        /// </summary>
        public int MaximumPulseDuration { get; private set; }

        /// <summary>
        /// Servo PWM frequency
        /// </summary>
        public Frequency Frequency { get; private set; } // almost always 50hz

        /// <summary>
        /// Create a new ServoConfig object
        /// </summary>
        /// <param name="minimumAngle">The minimum angle, in degrees, that the servo can move to. Default is `0°`.</param>
        /// <param name="maximumAngle">The maximum angle, in degrees, that the servo can move to. Default is `180°`.</param>
        /// <param name="minimumPulseDuration">The minimum angle's pulse duration (in microseconds). Default is 1,000 (1.0 millisecond).</param>
        /// <param name="maximumPulseDuration">The maximum angle's pulse duration (in microseconds). Default is 2,000 (2.0 milliseconds).</param>
        /// <param name="frequency">PWM frequency</param>
        public ServoConfig(
            Frequency frequency,
            Angle? minimumAngle = null, Angle? maximumAngle = null,
            int minimumPulseDuration = 1000, int maximumPulseDuration = 2000)
        {
            MinimumAngle = minimumAngle?? new Angle(0, Angle.UnitType.Degrees);
            MaximumAngle = maximumAngle?? new Angle(180, Angle.UnitType.Degrees);
            MinimumPulseDuration = minimumPulseDuration;
            MaximumPulseDuration = maximumPulseDuration;
            Frequency = frequency;
        }

        /// <summary>
        /// Create a new ServoConfig object
        /// </summary>
        /// <param name="minimumAngle">The minimum angle, in degrees, that the servo can move to. Default is `0°`.</param>
        /// <param name="maximumAngle">The maximum angle, in degrees, that the servo can move to. Default is `180°`.</param>
        /// <param name="minimumPulseDuration">The minimum angle's pulse duration (in microseconds). Default is 1,000 (1.0 millisecond).</param>
        /// <param name="maximumPulseDuration">The maximum angle's pulse duration (in microseconds). Default is 2,000 (2.0 milliseconds).</param>

        public ServoConfig(
            Angle? minimumAngle = null, Angle? maximumAngle = null,
            int minimumPulseDuration = 1000, int maximumPulseDuration = 2000)
            : this(new Frequency(50, Frequency.UnitType.Hertz), 
                minimumAngle, maximumAngle, 
                minimumPulseDuration, maximumPulseDuration)
        { }
    }
}