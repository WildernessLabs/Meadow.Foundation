using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;

namespace Sensors.Atmospheric.BME280_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        BME280 bme280;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus();
            bme280 = new BME280 (
                i2c,
                BME280.I2cAddress.Adddress0x77 //default
            );

            // TODO: SPI version


            // classical .NET events can also be used:
            bme280.TemperatureChanged += (object sender, FloatChangeResult e) => {
                Console.WriteLine($"Temp Changed, temp: {e.New}ºC");
            };
            bme280.PressureChanged += (object sender, FloatChangeResult e) => {
                Console.WriteLine($"Presure Changed, pressure: {e.New}hPa");
            };
            bme280.HumidityChanged += (object sender, FloatChangeResult e) => {
                Console.WriteLine($"Humidty Changed, temp: {e.New}%");
            };


            // just for funsies.
            Console.WriteLine($"ChipID: {bme280.GetChipID():X2}");
            //Thread.Sleep(1000);

            //// is this necessary? if so, it should probably be tucked into the driver
            //Console.WriteLine("Reset");
            //bme280.Reset();

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            bme280.StartUpdating();

            Console.WriteLine("Feeling cute, might delete later.");
        }

        protected async Task ReadConditions()
        {
            var conditions = await bme280.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {conditions.Temperature}ºC");
            Console.WriteLine($"  Pressure: {conditions.Pressure}hPa");
            Console.WriteLine($"  Relative Humidity: {conditions.Humidity}%");
        }

    }
}