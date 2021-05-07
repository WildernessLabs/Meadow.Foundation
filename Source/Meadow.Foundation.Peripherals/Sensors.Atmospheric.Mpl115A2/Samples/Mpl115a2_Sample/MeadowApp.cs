using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        RgbPwmLed onboardLed;
        Mpl115a2 sensor;

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
            sensor = new Mpl115a2(i2cBus);

            Console.WriteLine("Sensor initialized.");

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            sensor.StartUpdating();

            //==== Events
            // classical .NET events can also be used:
            sensor.Updated += (object sender, IChangeResult<(Temperature Temperature, Pressure Pressure)> result) => {
                Console.WriteLine($"  Temperature: {result.New.Temperature.Celsius:F1}°C");
                Console.WriteLine($"  Pressure: {result.New.Pressure.Pascal:F1}hpa");
            };

            //==== IObservable
            var consumer = Mpl115a2.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer triggered; new temp: {result.New.Item1.Celsius}, old: {result.Old?.Item1.Celsius}");
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