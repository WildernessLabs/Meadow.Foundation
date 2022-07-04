using Meadow.Hardware;

namespace Meadow.Foundation.Servos
{
    public class Servo : AngularServoBase
    {
        public Servo(IPwmOutputController device, IPin pwm, ServoConfig config) :
            this(device.CreatePwmPort(pwm, new Units.Frequency(IPwmOutputController.DefaultPwmFrequency, Units.Frequency.UnitType.Hertz)), config) { }

        public Servo(IPwmPort pwm, ServoConfig config) : 
            base(pwm, config) { }
    }
}