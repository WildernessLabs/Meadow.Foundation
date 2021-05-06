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

        private void Sensor_Updated(object sender, ChangeResult<(Meadow.Units.Temperature Temperature, Meadow.Units.RelativeHumidity Humidity)> result)
        {
            Console.WriteLine($"Humidity: {result.New.Humidity.Value*100}%, Temperature: {result.New.Temperature.Celsius}°C");
        }

        public void Initialize()
        {
            Console.WriteLine("Init...");

            sensor = new Dht12(Device.CreateI2cBus());
        }
    }
}