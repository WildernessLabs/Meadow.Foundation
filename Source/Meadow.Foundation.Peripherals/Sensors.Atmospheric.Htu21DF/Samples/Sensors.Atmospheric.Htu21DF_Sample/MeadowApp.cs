using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Htu21d sensor;

        public MeadowApp()
        {
            InitHardware();

            sensor.StartUpdating(1000);
        }

        public void InitHardware()
        {
            Console.WriteLine("Init hardware...");

            sensor = new Htu21d(Device.CreateI2cBus(400));

            sensor.Updated += Sensor_Updated;
        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Atmospheric.AtmosphericConditionChangeResult e)
        {
            Console.WriteLine($"Temp {e.New.Temperature}");
            Console.WriteLine($"Hum {e.New.Humidity}");
        }
    }
}