using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;

namespace Sensors.Atmospheric.Bmp085_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Bmp085 bmp085;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus();
            bmp085 = new Bmp085(i2c);   

            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree, and humidty by 5%.
            // (blowing hot breath on the sensor should trigger)
            var observer = Bmp085.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Temp and pressure changed by threshold; new temp: {result.New.Item1}, old: {result.Old?.Item1}");
                },
                filter: null
                //result =>
                //{
                //    return ((Math.Abs(result.Delta.Value.Item1.Value) > 1) &&
                //            (Math.Abs(result.Delta.Value.Item2.Value) > 5));
                //}
                );


            bmp085.Subscribe(observer);

            bmp085.Updated += Bmp085_Updated;

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            bmp085.StartUpdating();
        }

        private void Bmp085_Updated(object sender, IChangeResult<(Meadow.Units.Temperature Temperature, Meadow.Units.Pressure Pressure)> e)
        {
            Console.WriteLine($"Temperature: {e.New.Temperature.Celsius}°C, Pressure: {e.New.Pressure.Pascal}Pa");
        }

        protected async Task ReadConditions()
        {
            var conditions = await bmp085.Read();
            Console.WriteLine($"Temperature: {conditions.Temperature.Celsius}°C, Pressure: {conditions.Pressure.Pascal}Pa");
        }
    }
}