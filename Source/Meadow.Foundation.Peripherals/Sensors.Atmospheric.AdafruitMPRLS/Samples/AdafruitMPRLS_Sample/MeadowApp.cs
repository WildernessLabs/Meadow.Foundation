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
            Console.WriteLine("Initializing...");

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
            Console.WriteLine($"New pressure PSI: {result.New.Pressure?.Psi}, Old pressure PSI: {result.Old?.Pressure?.Psi}");

            Console.WriteLine($"Pressure in Pascal: {result.New.Pressure?.Pascal}");

            Console.WriteLine($"Raw sensor value: {result.New.RawPsiMeasurement?.Psi}");
        }

        //<!=SNOP=>
    }
}