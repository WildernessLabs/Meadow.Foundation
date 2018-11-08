using System;

namespace Meadow.Foundation.Servos
{
    public class ServoConfig
    {
        public int MinimumAngle { get; private set; }
        public int MaximumAngle { get; private set; } // -1 for continuous rotation
        public int MinimumPulseDuration { get; private set; }
        public int MaximumPulseDuration { get; private set; }

        public int Frequency { get; private set; } // almost always 50hz

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minimumAngle"></param>
        /// <param name="maximumAngle"></param>
        /// <param name="minimumPulseDuration">The minimum angle's pulse duration (in microseconds). Default is 1,000 (1.0 millisecond).</param>
        /// <param name="maximumPulseDuration">The maximum angle's pulse duratino (in microseconds). Default is 2,000 (2.0 milleseconds).</param>
        /// <param name="frequency"></param>
        public ServoConfig(int minimumAngle = 0, int maximumAngle = 180, int minimumPulseDuration = 1000, int maximumPulseDuration = 2000, int frequency = 50)
        {
            MinimumAngle = minimumAngle;
            MaximumAngle = maximumAngle;
            MinimumPulseDuration = minimumPulseDuration;
            MaximumPulseDuration = maximumPulseDuration;
            Frequency = frequency;
        }
    }
}
