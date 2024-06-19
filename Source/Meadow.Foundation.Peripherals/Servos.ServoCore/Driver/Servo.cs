using Meadow.Hardware;

namespace Meadow.Foundation.Servos
{
    /// <summary>
    /// Class to represent a generic servo motor
    /// </summary>
    public class Servo : AngularServoBase
    {
        /// <summary>
        /// Create a new Servo object
        /// </summary>
        /// <param name="pwmPin">The PWM pin</param>
        /// <param name="config">The servo configuration</param>
        public Servo(IPin pwmPin, ServoConfig config) :
            this(pwmPin.CreatePwmPort(new Units.Frequency(IPwmOutputController.DefaultPwmFrequency, Units.Frequency.UnitType.Hertz)), config)
        { }

        /// <summary>
        /// Create a new Servo object
        /// </summary>
        /// <param name="pwmPort">The port for the PWM pin</param>
        /// <param name="config">The servo configuration</param>
        public Servo(IPwmPort pwmPort, ServoConfig config) :
            base(pwmPort, config)
        { }
    }
}