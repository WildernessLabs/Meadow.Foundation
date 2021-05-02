using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Peripherals.Sensors.Atmospheric;
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
            var observer = Bmp085.CreateObserver(h =>
            {
                Console.WriteLine($"Temp and pressure changed by threshold; new temp: {h.New.Value.Unit1}, old: {h.Old.Value.Unit1}");
            },
                e =>
                {
                    return ((Math.Abs(e.Delta.Value.Unit1.Value) > 1) &&
                            (Math.Abs(e.Delta.Value.Unit2.Value) > 5));
                });


            bmp085.Subscribe(observer);

            bmp085.Updated += Bmp085_Updated;

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            bmp085.StartUpdating();
        }

        private void Bmp085_Updated(object sender, CompositeChangeResult<Meadow.Units.Temperature, Meadow.Units.Pressure> e)
        {
            Console.WriteLine($"Temperature: {e.New.Value.Unit1.Celsius}°C, Pressure: {e.New.Value.Unit2.Pascal}Pa");
        }

        protected async Task ReadConditions()
        {
            var conditions = await bmp085.Read();
            Console.WriteLine($"Temperature: {conditions.Temperature.Celsius}°C, Pressure: {conditions.Pressure.Pascal}Pa");
        }
    }
}