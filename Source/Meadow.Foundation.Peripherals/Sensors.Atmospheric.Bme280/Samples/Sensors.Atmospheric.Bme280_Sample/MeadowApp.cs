using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Peripherals.Sensors.Atmospheric;
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

            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree, and humidty by 5%.
            // (blowing hot breath on the sensor should trigger)
            var consumer = Bme280.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Temp and pressure changed by threshold; new temp: {result.New.Value.Unit1.Celsius}, old: {result.Old.Value.Unit1.Celsius}");
                },
                filter: result => {
                    return true;
                    //return (
                    //    (Math.Abs(result.Delta.Value.Unit1.Celsius) > 1)
                    //    &&
                    //    (Math.Abs(result.Delta.Value.Unit3.Bar) > 5)
                    //    );
                } );
            bme280.Subscribe(consumer);


            // classical .NET events can also be used:
            bme280.Updated += (object sender, CompositeChangeResult<Temperature, RelativeHumidity, Pressure> e) => {
                Console.WriteLine($"  Temperature: {e.New.Value.Unit1.Celsius}°C");
                Console.WriteLine($"  Relative Humidity: {e.New.Value.Unit2.Value}%");
                Console.WriteLine($"  Pressure: {e.New.Value.Unit3.Pascal}hPa");
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
            Console.WriteLine($"  Temperature: {conditions.Temperature}°C");
            Console.WriteLine($"  Pressure: {conditions.Pressure}hPa");
            Console.WriteLine($"  Relative Humidity: {conditions.Humidity}%");
        }

    }
}