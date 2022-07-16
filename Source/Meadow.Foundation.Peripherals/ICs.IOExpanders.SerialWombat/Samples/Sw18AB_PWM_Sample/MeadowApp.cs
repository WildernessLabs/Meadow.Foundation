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
        private IPwmPort _pwm;

        public MeadowApp()
        {
        }

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            try
            {
                Resolver.Log.Info(" creating Wombat...");
                _wombat = new Sw18AB(Device.CreateI2cBus());
                Resolver.Log.Info(" creating PWM...");
                _pwm = _wombat.CreatePwmPort(_wombat.Pins.WP0, new Meadow.Units.Frequency(1, Meadow.Units.Frequency.UnitType.Hertz));
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

            _pwm.Start();

            for (int i = 1; i < 1000; i += 10)
            {
                Resolver.Log.Info($"{i}Hz");
                _pwm.Frequency = new Meadow.Units.Frequency(i, Meadow.Units.Frequency.UnitType.Hertz);
                await Task.Delay(2000);
            }

            _pwm.Stop();
        }

        //<!=SNOP=>
    }
}