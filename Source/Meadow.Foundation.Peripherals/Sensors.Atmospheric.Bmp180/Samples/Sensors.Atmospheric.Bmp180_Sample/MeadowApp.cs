using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
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
            var observer = Bmp180.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Temp and pressure changed by threshold; new temp: {result.New.Item1}, old: {result.Old?.Item1}");
                },
                filter: null);
                //e =>
                //{
                //    return ((Math.Abs(e.Delta.Value.Unit1.Value) > 1) &&
                //            (Math.Abs(e.Delta.Value.Unit2.Value) > 5));
                //});

            bmp180.Subscribe(observer);

            bmp180.Updated += Bmp180_Updated;

            // get an initial reading
            ReadConditions().Wait();

            // start updating continuously
            bmp180.StartUpdating();
        }

        private void Bmp180_Updated(object sender, IChangeResult<(Meadow.Units.Temperature Temperature, Meadow.Units.Pressure Pressure)> result)
        {
            Console.WriteLine($"Temperature: {result.New.Temperature.Celsius}°C, Pressure: {result.New.Pressure.Pascal}Pa");
        }

        protected async Task ReadConditions()
        {
            var conditions = await bmp180.Read();
            Console.WriteLine($"Temperature: {conditions.Temperature.Celsius}°C, Pressure: {conditions.Pressure.Pascal}Pa");
        }
    }
}