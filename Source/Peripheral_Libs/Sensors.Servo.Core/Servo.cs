using Meadow.Hardware;

namespace Meadow.Foundation.Servos
{
    public class Servo : ServoBase
    {
        public Servo(IPWMPort pwm, ServoConfig config) : base(pwm, config)
        {

        }
    }
}