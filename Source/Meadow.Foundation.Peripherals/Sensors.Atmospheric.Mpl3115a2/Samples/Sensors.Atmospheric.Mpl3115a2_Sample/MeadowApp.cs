using System;
using Meadow;
using Meadow.Devices;
using Meadow.Peripherals.Sensors.Atmospheric;
using System.Threading.Tasks;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;
using Meadow.Units;

namespace Sensors.Atmospheric.Mpl3115A2_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mpl3115a2 sensor;

        //public MeadowApp()
        //{
        //    Console.WriteLine("Initializing...");

        //    // configure our BME280 on the I2C Bus
        //    var i2c = Device.CreateI2cBus();
        //    sensor = new Mpl3115a2(i2c);

        //    // Example that uses an IObersvable subscription to only be notified
        //    // when the temperature changes by at least a degree, and humidty by 5%.
        //    // (blowing hot breath on the sensor should trigger)
        //    sensor.Subscribe(new FilterableChangeObserver<AtmosphericConditionChangeResult, AtmosphericConditions>(
        //        h => {
        //            Console.WriteLine($"Temp and pressure changed by threshold; new temp: {h.New.Temperature}, old: {h.Old.Temperature}");
        //        },
        //        e =>
        //        {
        //            return
        //            (
        //                (Math.Abs(e.Delta.Temperature.Value) > 1) &&
        //                (Math.Abs(e.Delta.Pressure.Value) > 5)
        //            );
        //        }
        //    ));

        //    // classical .NET events can also be used:
        //    sensor.Updated += (object sender, AtmosphericConditionChangeResult e) =>
        //    {
        //        Console.WriteLine($"Temperature: {e.New.Temperature}C, Pressure: {e.New.Pressure}hPa");
        //    }; 

        //    // get an initial reading
        //    ReadConditions().Wait();

        //    Console.WriteLine("Begin updates");

        //    // start updating continuously
        //    sensor.StartUpdating();
        //}

        public MeadowApp()
        {
            Initialize();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            // create the I2C Bus
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Standard);

            Console.WriteLine("Created I2C Bus");

            // create our device
            sensor = new Mpl3115a2(i2cBus);

            Console.WriteLine("Sensor initialized.");

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            sensor.StartUpdating();

            //==== Events
            // classical .NET events can also be used:
            sensor.Updated += (object sender, CompositeChangeResult<Temperature, Pressure> result) => {
                Console.WriteLine($"  Temperature: {result.New.Value.Unit1.Celsius:F1}°C");
                Console.WriteLine($"  Pressure: {result.New.Value.Unit2.Pascal:F1}hpa");
            };

            //==== IObservable
            var consumer = Mpl3115a2.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer triggered; new temp: {result.New.Value.Unit1.Celsius}, old: {result.Old.Value.Unit1.Celsius}");
                },
                filter: result => {
                    return true;
                    //return (
                    //    (Math.Abs(result.Delta.Value.Unit1.Celsius) > 1)
                    //    &&
                    //    (Math.Abs(result.Delta.Value.Unit3.Bar) > 5)
                    //    );
                });
            sensor.Subscribe(consumer);

            Console.WriteLine("Hardware initialized.");
        }

        protected async Task ReadConditions()
        {
            var result = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {result.Temperature.Celsius:F1}°C");
            Console.WriteLine($"  Pressure: {result.Pressure:F1}hpa");
        }

    }
}