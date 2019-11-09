using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Peripherals.Sensors.Atmospheric;
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

            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree, and humidty by 5%.
            // (blowing hot breath on the sensor should trigger)
            bme280.Subscribe(new FilterableObserver<AtmosphericConditionChangeResult, AtmosphericConditions>(
                h => {
                    Console.WriteLine($"Temp and pressure changed by threshold; new temp: {h.New.Temperature}, old: {h.Old.Temperature}");
                },
                e => {
                    return (
                        (Math.Abs(e.Delta.Temperature) > 1)
                        &&
                        (Math.Abs(e.Delta.Pressure) > 5)
                        );
                }
                ));

            // classical .NET events can also be used:
            bme280.Updated += (object sender, AtmosphericConditionChangeResult e) => {
                Console.WriteLine($"  Temperature: {e.New.Temperature}ºC");
                Console.WriteLine($"  Pressure: {e.New.Pressure}hPa");
                Console.WriteLine($"  Relative Humidity: {e.New.Humidity}%");
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