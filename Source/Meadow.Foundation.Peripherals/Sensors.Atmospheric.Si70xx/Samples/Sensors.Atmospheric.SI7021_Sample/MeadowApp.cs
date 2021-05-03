using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System.Threading.Tasks;
using Meadow.Units;

namespace BasicSensors.Atmospheric.SI7021_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Si70xx si7021;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our SI7021 on the I2C Bus
            var i2cBus = Device.CreateI2cBus();

            si7021 = new Si70xx(i2cBus);

            Console.WriteLine($"Chip Serial: {si7021.SerialNumber}");

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            si7021.StartUpdating();

            //==== Events
            // classical .NET events can also be used:
            si7021.Updated += (object sender, CompositeChangeResult<Temperature, RelativeHumidity> result) => {
                Console.WriteLine($"  Temperature: {result.New?.Unit1.Celsius:F1}°C");
                Console.WriteLine($"  Relative Humidity: {result.New?.Unit2.Value:F1}%");
            };

            //==== IObservable
            var consumer = Si70xx.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer triggered; new temp: {result.New?.Unit1.Celsius}, old: {result.Old?.Unit1.Celsius}");
                },
                filter: result => {
                    return true;
                    //return (
                    //    (Math.Abs(result.Delta.Value.Unit1.Celsius) > 1)
                    //    &&
                    //    (Math.Abs(result.Delta.Value.Unit3.Bar) > 5)
                    //    );
                });
            si7021.Subscribe(consumer);
        }

        protected async Task ReadConditions()
        {
            var result = await si7021.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"  Temperature: {result.Temperature.Celsius:F1}°C");
            Console.WriteLine($"  Relative Humidity: {result.Humidity:F1}%");
        }

        //protected void ShowConditions()
        //{
        //    Console.WriteLine($"  T: {si7021.Conditions.Temperature:F1}C    RH: {si7021.Conditions.Humidity:F1}%");
        //}
    }
}