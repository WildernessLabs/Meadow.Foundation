using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;

namespace Sensors.Temperature.TMP102_Sample
{
    public class App : App<F7Micro, App>
    {
        TMP102 sensor;

        public App()
        {
            InitHardware();

            sensor.StartUpdating();
        }

        public void InitHardware()
        {
            Console.WriteLine("Creating output ports...");

            sensor = new TMP102(Device.CreateI2cBus());
            sensor.Updated += Sensor_Updated;
        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditionChangeResult e)
        {
            Console.WriteLine($"Temp: {e.New.Temperature}");
        }
    }
}