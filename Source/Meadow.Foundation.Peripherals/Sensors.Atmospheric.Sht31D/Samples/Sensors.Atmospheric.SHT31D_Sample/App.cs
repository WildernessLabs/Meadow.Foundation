using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;

namespace BasicSensors.Atmospheric.SHT31D_Sample
{
    public class App : App<F7Micro, App>
    {
        SHT31D sensor;

        public App()
        {
            InitHardware();

            Console.WriteLine($"Temp: {sensor.Temperature}");
            Console.WriteLine($"Humidity: {sensor.Humidity}");

            sensor.StartUpdating();
        }

        public void InitHardware()
        {
            Console.WriteLine("Init sensor...");

            sensor = new SHT31D(Device.CreateI2cBus());
            sensor.Updated += Sensor_Updated;

        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditionChangeResult e)
        {
            Console.WriteLine($"Temp: {e.New.Temperature}");
            Console.WriteLine($"Humidity: {e.New.Humidity}");
        }
    }
}