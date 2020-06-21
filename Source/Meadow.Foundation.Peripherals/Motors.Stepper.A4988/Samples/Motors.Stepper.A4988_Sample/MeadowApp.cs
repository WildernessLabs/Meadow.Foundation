using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Motors.Stepper;
using System;
using System.Threading;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public MeadowApp()
        {
            StepperSample();
        }

        public void StepperSample_Divisors()
        {
            var a = new A4988(Device, Device.Pins.D01, Device.Pins.D00, Device.Pins.D04, Device.Pins.D03, Device.Pins.D02);

            var s = (StepDivisor[])Enum.GetValues(typeof(StepDivisor));
            //var s = new StepDivisor[] { StepDivisor.Divisor_2 };
            while (true)
            {
                foreach (var sd in s)
                {
                    a.StepDivisor = sd;
                    a.Rotate(360);

                    Thread.Sleep(2000);
                }
            }
        }

        public void StepperSample()
        {
            var a = new A4988(Device, Device.Pins.D01, Device.Pins.D00, Device.Pins.D04, Device.Pins.D03, Device.Pins.D02);

            var s = (StepDivisor[])Enum.GetValues(typeof(StepDivisor));
            while (true)
            {
                foreach (var sd in s)
                {
                    for (var d = 2; d < 5; d++)
                    {
                        Console.WriteLine($"180 degrees..Speed divisor = {d}..1/{(int)sd} Steps..{a.Direction}...");
                        a.RotationSpeedDivisor = d;
                        a.StepDivisor = sd;
                        a.Rotate(180);

                        Thread.Sleep(500);
                    }
                }
                a.Direction = (a.Direction == RotationDirection.Clockwise) ? RotationDirection.Counterclockwise : RotationDirection.Clockwise;
            }
        }
    }
}