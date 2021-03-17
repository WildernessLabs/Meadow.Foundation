using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;

namespace Sensors.Temperature.TMP102_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mcp9808 sensor;

        public MeadowApp()
        {
            InitHardware();

            sensor.StartUpdating();
        }

        public void InitHardware()
        {
            Console.WriteLine("Init Mcp9808...");

            sensor = new Mcp9808(Device.CreateI2cBus());

            Console.WriteLine("Mcp9808 created");

            sensor.Updated += Sensor_Updated;

            Console.WriteLine("Start reading temperature data");
            sensor.StartUpdating();
        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditionChangeResult e)
        {
            Console.WriteLine($"Temp: {e.New.Temperature}°C");
        }
    }
}