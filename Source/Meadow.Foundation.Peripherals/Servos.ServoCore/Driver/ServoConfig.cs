using System;
using Meadow.Units;

namespace Meadow.Foundation.Servos
{
    public class ServoConfig
    {
        public Angle MinimumAngle { get; private set; }
        public Angle MaximumAngle { get; private set; } // -1 for continuous rotation
        public int MinimumPulseDuration { get; private set; }
        public int MaximumPulseDuration { get; private set; }

        public int Frequency { get; private set; } // almost always 50hz

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumAngle">The minimum angle, in degrees, that the servo can move to. Default is `0°`.</param>
        /// <param name="maximumAngle">The maximum angle, in degrees, that the servo can move to. Default is `180°`.</param>
        /// <param name="minimumPulseDuration">The minimum angle's pulse duration (in microseconds). Default is 1,000 (1.0 millisecond).</param>
        /// <param name="maximumPulseDuration">The maximum angle's pulse duratino (in microseconds). Default is 2,000 (2.0 milleseconds).</param>
        /// <param name="frequency"></param>
        public ServoConfig(
            Angle? minimumAngle = null, Angle? maximumAngle = null,
            int minimumPulseDuration = 1000, int maximumPulseDuration = 2000,
            int frequency = 50)
        {
            MinimumAngle = minimumAngle?? new Angle(0, Angle.UnitType.Degrees);
            MaximumAngle = maximumAngle?? new Angle(180, Angle.UnitType.Degrees);
            MinimumPulseDuration = minimumPulseDuration;
            MaximumPulseDuration = maximumPulseDuration;
            Frequency = frequency;

        }
    }
}
