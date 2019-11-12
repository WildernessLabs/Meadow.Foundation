using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Atmospheric;
using System.Threading.Tasks;

namespace BasicSensors.Atmospheric.SI7021_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Si70xx si7021;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus();
            si7021 = new Si70xx(
                i2c
            );

            // just for funsies.
            Console.WriteLine($"Chip Serial: {si7021.SerialNumber}");
            //Thread.Sleep(1000);


            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            si7021.StartUpdating();

        }

        protected async Task ReadConditions()
        {
            var conditions = await si7021.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {conditions.Temperature}ºC");
            Console.WriteLine($"  Pressure: {conditions.Pressure}hPa");
            Console.WriteLine($"  Relative Humidity: {conditions.Humidity}%");
        }

    }
}
