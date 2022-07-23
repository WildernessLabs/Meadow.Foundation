using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Sw18AB_Samples
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Sw18AB _wombat;
        private IRangeFinder _sensor;

        public MeadowApp()
        {
        }

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            try
            {
                Resolver.Log.Info(" creating Wombat...");
                _wombat = new Sw18AB(Device.CreateI2cBus(), logger: Resolver.Log);

                Resolver.Log.Info($"  rev : {_wombat.Version.Firmware}");
                Resolver.Log.Info($"  core: {_wombat.GetSupplyVoltage().Volts:0.0}V");

                Resolver.Log.Info(" creating Sensor...");
                _sensor = _wombat.CreateDistanceSensor(_wombat.Pins.WP11, _wombat.Pins.WP10, TimeSpan.FromSeconds(1));
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"error: {ex.Message}");
            }

            await Task.Delay(1000);
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Running...");

            while (true)
            {
                await Task.Delay(2000);

                _sensor.MeasureDistance();
                Resolver.Log.Info($"Distance: {_sensor.Distance.Value.Centimeters:0.0}cm");
            }
        }

        //<!=SNOP=>
    }
}