using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors;

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


            Thread.Sleep(-1);
        }

        public void InitHardware()
        {
            Console.WriteLine("Init sensor...");

            sensor = new SHT31D(Device.CreateI2cBus());
            sensor.TemperatureChanged += Sensor_TemperatureChanged;
            sensor.HumidityChanged += Sensor_HumidityChanged;
        }

        private void Sensor_HumidityChanged(object sender, SensorFloatEventArgs e)
        {
            Console.WriteLine($"Humidity: {e.CurrentValue}");
        }

        private void Sensor_TemperatureChanged(object sender, SensorFloatEventArgs e)
        {
            Console.WriteLine($"Temp: {e.CurrentValue}");
        }
    }
}