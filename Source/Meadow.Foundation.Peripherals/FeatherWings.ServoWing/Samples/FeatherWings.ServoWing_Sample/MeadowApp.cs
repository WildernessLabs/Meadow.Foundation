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
        ServoWing servoWing;

        public MeadowApp()
        {
            Initialize();
            Run();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            var i2CBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            servoWing = new ServoWing(i2CBus);
            servoWing.Initialize();
        }

        void Run()
        {
            var servo = servoWing.GetServo(0, new Meadow.Foundation.Servos.ServoConfig());

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