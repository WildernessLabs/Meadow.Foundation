using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foudnation.Motors.Stepper;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Uln2003 stepperController;

        public MeadowApp()
        {
            Initialize();

            stepperController.Step(1024);

         /*   for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Step forward {i}");

                stepperController.Step(50);

                Thread.Sleep(10);
            }

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"Step backwards {i}");

                stepperController.Step(-50);

                Thread.Sleep(10);
            } */
        }

        public void Initialize()
        {
            Console.WriteLine("Init ULN2003...");

            stepperController = new Uln2003(Device, Device.Pins.D01, Device.Pins.D02, Device.Pins.D03, Device.Pins.D04);
        }
    }
}