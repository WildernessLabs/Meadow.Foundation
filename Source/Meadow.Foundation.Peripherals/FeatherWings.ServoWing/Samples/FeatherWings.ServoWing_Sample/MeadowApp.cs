using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;
using Meadow.Hardware;
using Meadow.Foundation.Servos;
using Meadow.Units;
using AU = Meadow.Units.Angle.UnitType;

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

            var servo = servoWing.GetServo(0, NamedServoConfigs.SG90);

            while (true)
            {
                Console.WriteLine("0");
                servo.RotateTo(new Angle(0, AU.Degrees));
                Thread.Sleep(1000);

                Console.WriteLine("45");
                servo.RotateTo(new Angle(45, AU.Degrees));
                Thread.Sleep(1000);

                Console.WriteLine("90");
                servo.RotateTo(new Angle(90, AU.Degrees));
                Thread.Sleep(1000);

                Console.WriteLine("135");
                servo.RotateTo(new Angle(135, AU.Degrees));
                Thread.Sleep(1000);
            }
        }
    }
}