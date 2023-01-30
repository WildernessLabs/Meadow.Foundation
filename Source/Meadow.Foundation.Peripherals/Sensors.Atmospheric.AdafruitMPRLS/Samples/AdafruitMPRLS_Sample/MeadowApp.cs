using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace AdafruitMPRLSSensorExample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        AdafruitMPRLS sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            sensor = new AdafruitMPRLS(Device.CreateI2cBus());
            sensor.Updated += PressureSensor_Updated;

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            sensor.StartUpdating(TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        void PressureSensor_Updated(object sender, IChangeResult<(Pressure? Pressure, Pressure? RawPsiMeasurement)> result)
        {
            Resolver.Log.Info($"New pressure PSI: {result.New.Pressure?.Psi}, Old pressure PSI: {result.Old?.Pressure?.Psi}");

            Resolver.Log.Info($"Pressure in Pascal: {result.New.Pressure?.Pascal}");

            Resolver.Log.Info($"Raw sensor value: {result.New.RawPsiMeasurement?.Psi}");
        }

        //<!=SNOP=>
    }
}