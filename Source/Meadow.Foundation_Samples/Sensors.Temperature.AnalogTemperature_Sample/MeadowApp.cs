using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;

namespace Sensors.Temperature.AnalogTemperature_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        AnalogTemperature analogTemperature;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            analogTemperature = new AnalogTemperature
            (
                device: Device,
                analogPin: Device.Pins.A00,
                sensorType: AnalogTemperature.KnownSensorType.LM35,
                updateInterval: 1000,
                temperatureChangeNotificationThreshold: 0.1F
            );

            TestAnalogTemperature();
        }

        protected void TestAnalogTemperature()
        {
            Console.WriteLine("TestAnalogTemperature...");

            // Before update;
            analogTemperature.Update();

            while (true)
            {
                Console.WriteLine(analogTemperature.Temperature);
                Thread.Sleep(1000);
            }

            // Connect an interrupt handler.
            analogTemperature.TemperatureChanged += (s, e) =>
            {
                Console.WriteLine("Temperature: " + e.CurrentValue.ToString("f2"));
            };
        }
    }
}