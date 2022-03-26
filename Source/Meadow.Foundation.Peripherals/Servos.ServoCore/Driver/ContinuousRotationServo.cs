using Meadow.Hardware;

namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Represents a servo that can rotate continuously.
    /// </summary>
    public class ContinuousRotationServo : ContinuousRotationServoBase
    {

        /// <summary>
        /// Instantiates a new continuous rotation servo on the specified pin, with the specified configuration.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="config"></param>
        public ContinuousRotationServo(IPwmPort pwm, ServoConfig config) : base (pwm, config)
        {
        }
    }
}