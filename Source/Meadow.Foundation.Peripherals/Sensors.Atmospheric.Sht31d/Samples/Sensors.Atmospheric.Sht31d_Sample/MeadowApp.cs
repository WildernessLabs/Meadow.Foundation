using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;

namespace BasicSensors.Atmospheric.SHT31D_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Sht31d sensor;

        public MeadowApp()
        {
            InitHardware();

            Console.WriteLine($"Temp: {sensor.Temperature}");
            Console.WriteLine($"Humidity: {sensor.Humidity}");

            sensor.StartUpdating();
        }

        public void InitHardware()
        {
            Console.WriteLine("Init sensor...");

            sensor = new Sht31d(Device.CreateI2cBus());
            sensor.Updated += Sensor_Updated;

        }

        private void Sensor_Updated( object sender,
            IChangeResult<(Meadow.Units.Temperature Temperature, Meadow.Units.RelativeHumidity Humidity)> e)
        {
            Console.WriteLine($"Temp: {e.New.Temperature.Celsius}");
            Console.WriteLine($"Humidity: {e.New.Humidity.Percent}");
        }
    }
}