using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {

        Hih6130 sensor;

        public MeadowApp()
        {
            Initialize();

            sensor.StartUpdating();

            sensor.Updated += Sensor_Updated;
        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditionChangeResult e)
        {
            Console.WriteLine($"Humidity: {e.New.Humidity}, Temperature: {e.New.Temperature}");
        }

        public void Initialize()
        {
            Console.WriteLine("Init...");

            sensor = new Hih6130(Device.CreateI2cBus());
        }
    }
}