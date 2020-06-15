using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;
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

            Console.WriteLine("Initialize wing...");
            servoWing.Initialize();
        }

        void Run()
        {
            Console.WriteLine("Run...");

            var servo = servoWing.GetServo(0, Meadow.Foundation.Servos.NamedServoConfigs.SG90);

            while (true)
            {
                Console.WriteLine("0");
                servo.RotateTo(0);
                Thread.Sleep(1000);

                Console.WriteLine("45");
                servo.RotateTo(45);
                Thread.Sleep(1000);

                Console.WriteLine("90");
                servo.RotateTo(90);
                Thread.Sleep(1000);

                Console.WriteLine("135");
                servo.RotateTo(135);
                Thread.Sleep(1000);
            }
        }
    }
}