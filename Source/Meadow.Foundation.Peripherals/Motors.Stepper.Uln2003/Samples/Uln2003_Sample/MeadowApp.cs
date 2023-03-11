using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Motors.Stepper;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Uln2003 stepperController;

        public override Task Initialize(string[]? args)
        {
            stepperController = new Uln2003(
                pin1: Device.Pins.D01,
                pin2: Device.Pins.D02,
                pin3: Device.Pins.D03,
                pin4: Device.Pins.D04);

            return base.Initialize(args);
        }

        public override Task Run()
        {
            stepperController.Step(1024);

            for (int i = 0; i < 100; i++)
            {
                Resolver.Log.Info($"Step forward {i}");
                stepperController.Step(50);
                Thread.Sleep(10);
            }

            for (int i = 0; i < 100; i++)
            {
                Resolver.Log.Info($"Step backwards {i}");
                stepperController.Step(-50);
                Thread.Sleep(10);
            }

            return base.Run();
        }

        //<!=SNOP=>
    }
}