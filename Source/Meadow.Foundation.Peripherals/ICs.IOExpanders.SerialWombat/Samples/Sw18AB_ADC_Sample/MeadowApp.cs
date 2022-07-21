using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Sw18AB_Samples
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Sw18AB _wombat;
        private IAnalogInputPort _adc;

        public MeadowApp()
        {
        }

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            try
            {
                Resolver.Log.Info(" creating Wombat...");
                _wombat = new Sw18AB(Device.CreateI2cBus(), logger: Resolver.Log);
                Resolver.Log.Info(" creating ADC...");
                _adc = _wombat.CreateAnalogInputPort(_wombat.Pins.WP0);
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

            Resolver.Log.Info($"ADC: Reference voltage = {_adc.ReferenceVoltage.Volts:0.0}V");
            for (int i = 1; i < 1000; i += 10)
            {
                var v = await _adc.Read();
                Resolver.Log.Info($"ADC: {_adc.Voltage.Volts:0.0}V");
                await Task.Delay(2000);
            }
        }

        //<!=SNOP=>
    }
}