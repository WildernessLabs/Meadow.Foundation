using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Foundation.Sensors.Atmospheric;

namespace Sensors.Atmospheric.BME680_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Bme680 bme680;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus();
            bme680 = new Bme680 (
                i2c
                //Bme680.I2cAddress.Adddress0x77 //Might need this enum
            );

            //// Example that uses an IObersvable subscription to only be notified
            //// when the temperature changes by at least a degree, and humidty by 5%.
            //// (blowing hot breath on the sensor should trigger)
            //bme680.Subscribe(new FilterableObserver<AtmosphericConditionChangeResult, AtmosphericConditions>(
            //    h => {
            //        Console.WriteLine($"Temp and pressure changed by threshold; new temp: {h.New.Temperature}, old: {h.Old.Temperature}");
            //    },
            //    e => {
            //        return (
            //            (Math.Abs(e.Delta.Temperature) > 1)
            //            &&
            //            (Math.Abs(e.Delta.Pressure) > 5)
            //            );
            //    }
            //    ));

            //// classical .NET events can also be used:
            //bme680.Updated += (object sender, AtmosphericConditionChangeResult e) => {
            //    Console.WriteLine($"  Temperature: {e.New.Temperature}°C");
            //    Console.WriteLine($"  Pressure: {e.New.Pressure}hPa");
            //    Console.WriteLine($"  Relative Humidity: {e.New.Humidity}%");
            //};

            //// just for funsies.
            //Console.WriteLine($"ChipID: {bme680.GetChipID():X2}");
            ////Thread.Sleep(1000);

            // get an initial reading
            ReadConditions().Wait();

            //// start updating continuously
            //bme680.StartUpdating();
        }

        protected async Task ReadConditions()
        {
            //var conditions = await bme680.Read();
            //Console.WriteLine("Initial Readings:");
            //Console.WriteLine($"  Temperature: {conditions.Temperature}°C");
            //Console.WriteLine($"  Pressure: {conditions.Pressure}hPa");
            //Console.WriteLine($"  Relative Humidity: {conditions.Humidity}%");
        }

    }
}