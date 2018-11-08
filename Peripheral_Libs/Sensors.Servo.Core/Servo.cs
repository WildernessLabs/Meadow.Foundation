using Meadow.Hardware;

namespace Meadow.Foundation.Servos
{
    public class Servo : ServoBase
    {
        public Servo(Cpu.PWMChannel pin, ServoConfig config) : base(pin, config)
        {

        }
    }
}