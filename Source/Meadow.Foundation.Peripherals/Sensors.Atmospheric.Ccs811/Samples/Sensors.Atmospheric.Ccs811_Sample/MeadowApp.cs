using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;

namespace Sensors.AirQuality.Ccs811_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Ccs811 sensor;

        public MeadowApp()
        {
            // configure our BME280 on the I2C Bus
            var i2c = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Fast);
            sensor = new Ccs811(i2c);

            sensor.StartUpdating();

            var bl = sensor.GetBaseline();
            Console.WriteLine($"Baseline A: 0x{bl:x4}");
            sensor.SetBaseline(0x847b);
            bl = sensor.GetBaseline();
            Console.WriteLine($"Baseline B: 0x{bl:x4}");
            sensor.SetMeasurementMode(Ccs811.MeasurementMode.ConstantPower250ms);
            /*
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
            */
            // get an initial reading
            ReadConditions().Wait();

            /*
            // start updating continuously
            bmp180.StartUpdating();
            */
        }

        protected async Task ReadConditions()
        {
            var conditions = await sensor.Read();
            Console.WriteLine($"CO2: {conditions.Co2.PartsPerMillion:n1}ppm, VOC: {conditions.Voc.PartsPerMillion:n1}ppm");
        }
    }
}