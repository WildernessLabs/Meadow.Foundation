using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;

namespace Sensors.Temperature.TMP102_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Lm75 sensor;

        public MeadowApp()
        {
            InitHardware();

            sensor.StartUpdating();
        }

        public void InitHardware()
        {
            Console.WriteLine("Initialize...");

            sensor = new Lm75(Device.CreateI2cBus());
            sensor.Updated += Sensor_Updated;
        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditionChangeResult e)
        {
            Console.WriteLine($"Temp: {e.New.Temperature}°C");
        }
    }
}