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

        private void Sensor_Updated(object sender, CompositeChangeResult<Meadow.Units.Temperature, Meadow.Units.RelativeHumidity> e)
        {
            Console.WriteLine($"Temp: {e.New.Value.Unit1.Celsius}");
            Console.WriteLine($"Humidity: {e.New.Value.Unit2.Value}");
        }
    }
}