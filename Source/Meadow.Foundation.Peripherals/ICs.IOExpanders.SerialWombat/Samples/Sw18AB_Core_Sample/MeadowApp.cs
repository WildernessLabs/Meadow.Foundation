using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using System;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Sw18AB_Samples
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Sw18AB _wombat;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            try
            {
                _wombat = new Sw18AB(Device.CreateI2cBus());
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"error: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("getting version...");

            var version = _wombat.Version; // this doesn't change, so read once
            var info = _wombat.Info;

            Resolver.Log.Info($"Version   : {version.Version}");
            Resolver.Log.Info($"Identifier: {info.Identifier}");
            Resolver.Log.Info($"Revision  : {info.Revision}");
            Resolver.Log.Info($"UUID      : {_wombat.Uuid}");

            while (true)
            {
                Resolver.Log.Info($"Temperature   : {_wombat.GetTemperature().Fahrenheit}F");
                Resolver.Log.Info($"Supply Voltage: {_wombat.GetSupplyVoltage().Volts}V");

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}