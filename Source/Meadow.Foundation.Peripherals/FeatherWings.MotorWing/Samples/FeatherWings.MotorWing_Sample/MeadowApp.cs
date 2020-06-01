using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Leds;
using Meadow.Hardware;

namespace FeatherWings.MotorWing_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        MotorWing motorWing;

        public MeadowApp()
        {
            Initialize();
            Run();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            var i2CBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            motorWing = new MotorWing(i2CBus, 0x61);
            motorWing.Initialize();
        }

        void Run()
        {
            //Get DC motor 1
            var dcMotor1 = motorWing.GetMotor(1);

            //Get DC motor 2
            var dcMotor2 = motorWing.GetMotor(2);

            //Get Stepper motor number 2
            var stepper = motorWing.GetStepper(2, 200);

            dcMotor1.Run(Commmand.FORWARD);
            dcMotor2.Run(Commmand.BACKWARD);

            while (true)
            {
                Console.WriteLine("Speed up");
                for (short i = 0; i <= 255; i++)
                {
                    dcMotor1.SetSpeed(i);
                    dcMotor2.SetSpeed(i);
                    Thread.Sleep(10);
                }

                stepper.Step(50);

                Thread.Sleep(500);

                Console.WriteLine("Slow down");
                for (short i = 255; i >= 0; i--)
                {
                    dcMotor1.SetSpeed(i);
                    dcMotor2.SetSpeed(i);
                    Thread.Sleep(10);
                }

                stepper.Step(-50);

                Thread.Sleep(500);
            }

        }
    }
}
