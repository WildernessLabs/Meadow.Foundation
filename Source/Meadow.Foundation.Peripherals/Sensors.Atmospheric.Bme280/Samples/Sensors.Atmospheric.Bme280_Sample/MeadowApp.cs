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
        Bme280 bme280;

        IDigitalOutputPort trigger;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // create a trigger for the LA
            trigger = Device.CreateDigitalOutputPort(Device.Pins.D13);
            Console.WriteLine("Trigger on D02");
            trigger.State = true;

            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus();
            bme280 = new Bme280 (
                i2c,
                Bme280.I2cAddress.Adddress0x76 //default
                //Bme280.I2cAddress.Adddress0x77 //default
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
                        (result.New.Humidity.Value.Percent - old.Humidity.Value.Percent) > 0.05 // 5% humidity change
                        ); // returns true if > 0.5°C change.
                    }
                    return false;
                }
                // if you want to always get notified, pass null for the filter:
                //filter: null
                );
            bme280.Subscribe(consumer);

            //==== Events
            // classical .NET events can also be used:
            bme280.Updated += (object sender, IChangeResult<(Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)> e) => {
                Console.WriteLine($"  Temperature: {e.New.Temperature?.Celsius:N2}C");
                Console.WriteLine($"  Relative Humidity: {e.New.Humidity:N2}%");
                Console.WriteLine($"  Pressure: {e.New.Pressure?.Bar:N2}bar");
            };

            // just for funsies.
            Console.WriteLine($"ChipID: {bme280.GetChipID():X2}");

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            bme280.StartUpdating();
        }

        protected async Task ReadConditions()
        {
            var conditions = await bme280.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {conditions.Temperature?.Celsius:N2}C");
            Console.WriteLine($"  Pressure: {conditions.Pressure?.Bar:N2}hPa");
            Console.WriteLine($"  Relative Humidity: {conditions.Humidity?.Percent:N2}%");
        }
    }
}