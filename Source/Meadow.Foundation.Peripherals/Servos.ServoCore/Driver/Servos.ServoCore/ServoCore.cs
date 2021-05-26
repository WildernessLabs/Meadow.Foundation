using Meadow.Hardware;

namespace Meadow.Foundation.Servos
{
    public class Servo : ServoBase
    {
        public Servo(IPwmOutputController device, IPin pwm, ServoConfig config) :
            this(device.CreatePwmPort(pwm), config) { }

        public Servo(IPwmPort pwm, ServoConfig config) : 
            base(pwm, config) { }
    }
}