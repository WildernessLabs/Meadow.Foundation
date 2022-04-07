using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Units;
using System;

namespace AdafruitMPRLSSensorExample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var PressureSensor = new AdafruitMPRLS(Device.CreateI2cBus());

            PressureSensor.StartUpdating(TimeSpan.FromSeconds(1));

            PressureSensor.Updated += PressureSensor_Updated;
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