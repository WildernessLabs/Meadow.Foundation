using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace FeatherWings.ServoWing_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        II2cBus _i2CBus;
        ServoWing _servoWing;
        public MeadowApp()
        {
            Initialize();
            Run();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            _i2CBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            _servoWing = new ServoWing(_i2CBus);
            _servoWing.Initialize();
        }

        void Run()
        {
            var servo = _servoWing.GetServo(0, new Meadow.Foundation.Servos.ServoConfig());

            while (true)
            {
                servo.RotateTo(0);
                Thread.Sleep(2000);

                servo.RotateTo(90);
                Thread.Sleep(2000);

                servo.RotateTo(180);
                Thread.Sleep(2000);
            }

        }

    }
}
