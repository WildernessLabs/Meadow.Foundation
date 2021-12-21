using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Motors.Stepper;

namespace MeadowApp
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        public MeadowApp()
        {
            var stepperController = new Uln2003(
                device: Device, 
                pin1: Device.Pins.D01, 
                pin2: Device.Pins.D02, 
                pin3: Device.Pins.D03, 
                pin4: Device.Pins.D04);

            stepperController.Step(1024);

            for (int i = 0; i < 100; i++)
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
            } 
        }
        //<!—SNOP—>
    }
}