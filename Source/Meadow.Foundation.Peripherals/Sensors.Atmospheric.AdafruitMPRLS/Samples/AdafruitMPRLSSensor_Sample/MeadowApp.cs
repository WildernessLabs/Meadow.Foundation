using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;

namespace AdafruitMPRLSSensorExample
{
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

            PressureSensor.StartUpdating();

            PressureSensor.Updated += PressureSensor_Updated;
        }

        private void PressureSensor_Updated(object sender, CompositeChangeResult<Meadow.Units.Pressure> e)
        {
            Console.WriteLine($"New pressure PSI: {e.New.Psi}, Old pressure PSI: {e.Old.Psi}");

            Console.WriteLine($"Pressure in Pascal: {e.New.Pascal}");

            Console.WriteLine($"Raw sensor value: {PressureSensor.RawPSIMeasurement}");
        }
    }
}