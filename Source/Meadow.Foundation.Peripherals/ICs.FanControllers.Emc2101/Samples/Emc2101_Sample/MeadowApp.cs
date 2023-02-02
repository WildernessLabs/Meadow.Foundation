using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.FanControllers;
using System;
using System.Threading.Tasks;

namespace Emc2101_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Emc2101 fanController;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            fanController = new Emc2101(i2cBus: Device.CreateI2cBus());

            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run ...");

            return base.Run();
        }

        //<!=SNOP=>
    }
}