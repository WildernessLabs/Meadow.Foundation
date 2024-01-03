using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Motors.Stepper;
using Meadow.Peripherals;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private A4988 a4988;

        public override Task Initialize()
        {
            a4988 = new A4988(
                step: Device.Pins.D01,
                direction: Device.Pins.D00,
                ms1Pin: Device.Pins.D04,
                ms2Pin: Device.Pins.D03,
                ms3Pin: Device.Pins.D02);

            return base.Initialize();
        }

        public override Task Run()
        {
            var stepDivisors = (StepDivisor[])Enum.GetValues(typeof(StepDivisor));
            while (true)
            {
                foreach (var step in stepDivisors)
                {
                    for (var d = 2; d < 5; d++)
                    {
                        Resolver.Log.Info($"180 degrees..Speed divisor = {d}..1/{(int)step} Steps..{a4988.Direction}...");
                        a4988.RotationSpeedDivisor = d;
                        a4988.StepDivisor = step;
                        a4988.Rotate(180);

                        Thread.Sleep(500);
                    }
                }
                a4988.Direction = (a4988.Direction == RotationDirection.Clockwise) ? RotationDirection.CounterClockwise : RotationDirection.Clockwise;
            }
        }

        //<!=SNOP=>

        public void StepperSample_Divisors()
        {
            var a = new A4988(
                step: Device.Pins.D01,
                direction: Device.Pins.D00,
                ms1Pin: Device.Pins.D04,
                ms2Pin: Device.Pins.D03,
                ms3Pin: Device.Pins.D02);

            var s = (StepDivisor[])Enum.GetValues(typeof(StepDivisor));
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
    }
}