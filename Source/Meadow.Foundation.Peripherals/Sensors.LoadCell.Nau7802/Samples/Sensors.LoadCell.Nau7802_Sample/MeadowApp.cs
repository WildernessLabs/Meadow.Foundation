using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace Sensors.LoadCell.Nau7802_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private Nau7802 _loadSensor;

        public MeadowApp()
        {
            Console.WriteLine($"Creating I2C Bus...");
            var bus = Device.CreateI2cBus();
            Console.WriteLine($"Creating Sensor...");
            using (_loadSensor = new Nau7802(bus))
            {
                _loadSensor.Tare();

                while (true)
                {
                    var c = _loadSensor.GetWeight();
                    Console.WriteLine($"Conversion returned {c}");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}