using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Atmospheric;

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
        AdafruitMPRLSSensor PressureSensor;

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            PressureSensor = new AdafruitMPRLSSensor(Device.CreateI2cBus());

            PressureSensor.StartUpdating();

            PressureSensor.Updated += PressureSensor_Updated;
        }

        private void PressureSensor_Updated(object sender, Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditionChangeResult e)
        {
            Console.WriteLine($"new pressure PSI: {e.New.Pressure}, old pressure PSI: {e.Old.Pressure}");

            Console.WriteLine($"pressure in hPA: {PressureSensor.CalculatedhPAMeasurement}");

            Console.WriteLine($"raw sensor value: {PressureSensor.RawPSIMeasurement}");
        }
    }
}