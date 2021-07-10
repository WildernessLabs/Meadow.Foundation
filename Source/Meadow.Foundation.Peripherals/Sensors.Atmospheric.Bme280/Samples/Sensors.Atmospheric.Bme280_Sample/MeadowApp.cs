using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;
using Meadow.Units;

namespace Sensors.Atmospheric.BME280_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Bme280 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus();
            sensor = new Bme280 (
                i2c,
                Bme280.I2cAddress.Adddress0x76 // SDA pulled up
                //Bme280.I2cAddress.Adddress0x77 // SDA pulled down
            );

            // TODO: SPI version

            //==== IObservable 
            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree, and humidty by 5%.
            // (blowing hot breath on the sensor should trigger)
            var consumer = Bme280.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },
                // only notify if the change is greater than 0.5°C
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return (
                        (result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5 // returns true if > 0.5°C change.
                        &&
                        (result.New.Humidity.Value - old.Humidity.Value).Percent > 0.05 // 5% humidity change
                        ); // returns true if > 0.5°C change.
                    }
                    return false;
                }
                // if you want to always get notified, pass null for the filter:
                //filter: null
                );
            sensor.Subscribe(consumer);

            //==== Events
            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => {
                Console.WriteLine($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
                Console.WriteLine($"  Relative Humidity: {result.New.Humidity:N2}%");
                Console.WriteLine($"  Pressure: {result.New.Pressure?.Millibar:N2}mbar ({result.New.Pressure?.Pascal:N2}Pa)");
            };

            // just for funsies.
            Console.WriteLine($"ChipID: {sensor.GetChipID():X2}");

            //==== one-off read
            ReadConditions().Wait();

            // start updating continuously
            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        protected async Task ReadConditions()
        {
            var conditions = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {conditions.Temperature?.Celsius:N2}C");
            Console.WriteLine($"  Pressure: {conditions.Pressure?.Bar:N2}hPa");
            Console.WriteLine($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%");
        }
    }
}