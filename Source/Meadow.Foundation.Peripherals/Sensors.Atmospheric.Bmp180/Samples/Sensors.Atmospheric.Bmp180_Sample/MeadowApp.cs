using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Atmospheric;

namespace Sensors.Atmospheric.Bmp180_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Bmp180 bmp180;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus();
            bmp180 = new Bmp180(i2c);

            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree, and humidty by 5%.
            // (blowing hot breath on the sensor should trigger)
            bmp180.Subscribe(new FilterableChangeObserver<AtmosphericConditionChangeResult, AtmosphericConditions>(
                h => {
                    Console.WriteLine($"Temp and pressure changed by threshold; new temp: {h.New.Temperature}, old: {h.Old.Temperature}");
                },
                e => {
                    return (
                        (Math.Abs(e.Delta.Temperature.Value) > 1)
                        &&
                        (Math.Abs(e.Delta.Pressure.Value) > 5)
                        );
                }
                ));

            // classical .NET events can also be used:
            bmp180.Updated += (object sender, AtmosphericConditionChangeResult e) => {
                Console.WriteLine($"Temperature: {e.New.Temperature}°C, Pressure: {e.New.Pressure}hPa");
            };

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            bmp180.StartUpdating();
        }

        protected async Task ReadConditions()
        {
            var conditions = await bmp180.Read();
            Console.WriteLine($"Temperature: {conditions.Temperature}°C, Pressure: {conditions.Pressure}hPa");
        }
    }
}