using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Motors.Stepper;
using Meadow.Hardware;
using Meadow.Peripherals;
using Meadow.Peripherals.Motors;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>
        private IStepperMotor stepper;

        public override Task Initialize()
        {
            stepper = new Em542s(
                Device.Pins.D15.CreateDigitalOutputPort(),
                Device.Pins.D14.CreateDigitalOutputPort(),
                inverseLogic: true);

            return base.Initialize();
        }

        public override Task Run()
        {
            var direction = RotationDirection.Clockwise;

            // max rate for this drive
            var rate = new Meadow.Units.Frequency(200, Meadow.Units.Frequency.UnitType.Kilohertz);

            while (true)
            {
                Resolver.Log.Info($"{direction}");

                stepper.Rotate(360f, direction, rate);
                Thread.Sleep(1000);

                direction = direction switch
                {
                    RotationDirection.CounterClockwise => RotationDirection.Clockwise,
                    _ => RotationDirection.CounterClockwise
                };
            }
        }

        //<!=SNOP=>

    }
}