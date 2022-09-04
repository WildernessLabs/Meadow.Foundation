using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Sw18AB_ADC_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Sw18AB serialWombat;
        private IAnalogInputPort analogInputPort;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            try
            {
                Resolver.Log.Info(" creating Wombat...");
                serialWombat = new Sw18AB(Device.CreateI2cBus(), logger: Resolver.Log);
                Resolver.Log.Info(" creating ADC...");
                analogInputPort = serialWombat.CreateAnalogInputPort(serialWombat.Pins.WP0);
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

            Resolver.Log.Info($"ADC: Reference voltage = {analogInputPort.ReferenceVoltage.Volts:0.0}V");
            for (int i = 1; i < 1000; i += 10)
            {
                var v = await analogInputPort.Read();
                Resolver.Log.Info($"ADC: {analogInputPort.Voltage.Volts:0.0}V");
                await Task.Delay(2000);
            }
        }

        //<!=SNOP=>
    }
}