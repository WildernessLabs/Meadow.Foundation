using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Sw18AB_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Sw18AB serialWombat;
        private IDigitalOutputPort digitalOutputPort;
        private IDigitalInputPort digitalInputPort;
        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize...");

            try
            {
                serialWombat = new Sw18AB(Device.CreateI2cBus());
                digitalOutputPort = serialWombat.CreateDigitalOutputPort(serialWombat.Pins.WP0);
                digitalInputPort = serialWombat.CreateDigitalInputPort(serialWombat.Pins.WP1);
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"error: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Running...");

            bool state = false;

            while (true)
            {
                Resolver.Log.Info($"WP0 = {(state ? "high" : "low")}");
                digitalOutputPort.State = state;
                Resolver.Log.Info($"WP1 = {(digitalInputPort.State ? "high" : "low")}");
                state = !state;

                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}