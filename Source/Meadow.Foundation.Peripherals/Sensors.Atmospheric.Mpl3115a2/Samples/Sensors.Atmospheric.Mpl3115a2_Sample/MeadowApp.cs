using System;
using Meadow;
using Meadow.Devices;
using Meadow.Peripherals.Sensors.Atmospheric;
using System.Threading.Tasks;
using Meadow.Foundation.Sensors.Atmospheric;

namespace Sensors.Atmospheric.Mpl3115A2_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mpl3115a2 mpl3115A2;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus();
            mpl3115A2 = new Mpl3115a2(i2c);

            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree, and humidty by 5%.
            // (blowing hot breath on the sensor should trigger)
            mpl3115A2.Subscribe(new FilterableChangeObserver<AtmosphericConditionChangeResult, AtmosphericConditions>(
                h => {
                    Console.WriteLine($"Temp and pressure changed by threshold; new temp: {h.New.Temperature}, old: {h.Old.Temperature}");
                },
                e =>
                {
                    return
                    (
                        (Math.Abs(e.Delta.Temperature.Value) > 1) &&
                        (Math.Abs(e.Delta.Pressure.Value) > 5)
                    );
                }
            ));

            // classical .NET events can also be used:
            mpl3115A2.Updated += (object sender, AtmosphericConditionChangeResult e) =>
            {
                Console.WriteLine($"Temperature: {e.New.Temperature}C, Pressure: {e.New.Pressure}hPa");
            }; 

            // get an initial reading
            ReadConditions().Wait();

            Console.WriteLine("Begin updates");

            // start updating continuously
            mpl3115A2.StartUpdating();
        }

        protected async Task ReadConditions()
        {
            var conditions = await mpl3115A2.Read();
            Console.WriteLine($"Temperature: {conditions.Temperature}C, Pressure: {conditions.Pressure}hPa");
        }
    }
}