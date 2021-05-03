using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric.Dhtxx;

namespace Sensors.Atmospheric.Dht12_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Dht12 sensor;

        public MeadowApp()
        {
            Initialize();

            sensor.StartUpdating();

            sensor.Updated += Sensor_Updated;
        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditionChangeResult e)
        {
            Console.WriteLine($"Humidity: {e.New.Humidity}%, Temperature: {e.New.Temperature}°C");
        }

        public void Initialize()
        {
            Console.WriteLine("Init...");

            sensor = new Dht12(Device.CreateI2cBus());
        }
    }
}