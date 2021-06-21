using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Units;
using System;

namespace AdafruitMPRLSSensorExample
{
    // TODO: this app needs rewritten
    /// <summary>
    /// Connect VIN to 3.3v
    /// Connect GND to ground
    /// Connect SCL to pin D08
    /// Connect SDA to pin D07
    /// </summary>
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        AdafruitMPRLS PressureSensor;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            PressureSensor = new AdafruitMPRLS(Device.CreateI2cBus());

            PressureSensor.StartUpdating(TimeSpan.FromSeconds(1));

            PressureSensor.Updated += PressureSensor_Updated;
        }

        private void PressureSensor_Updated(object sender, IChangeResult<(Pressure? Pressure, Pressure? RawPsiMeasurement)> result)
        {
            Console.WriteLine($"New pressure PSI: {result.New.Pressure?.Psi}, Old pressure PSI: {result.Old?.Pressure?.Psi}");

            Console.WriteLine($"Pressure in Pascal: {result.New.Pressure?.Pascal}");

            Console.WriteLine($"Raw sensor value: {result.New.RawPsiMeasurement?.Psi}");
        }
    }
}