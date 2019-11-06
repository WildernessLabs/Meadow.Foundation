using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;

namespace Sensors.Temperature.AnalogTemperature_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        AnalogTemperature analogTemperature;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            analogTemperature = new AnalogTemperature
            (
                device: Device,
                analogPin: Device.Pins.A00,
                sensorType: AnalogTemperature.KnownSensorType.LM35
            );

            //TestAnalogTemperature();


            analogTemperature.Subscribe(new FilterableObserver<FloatChangeResult>(
                h => {
                    Console.WriteLine($"Temp changed by a degree; new: {h.New}, old: {h.Old}");
                },
                e => {
                    return (e.Delta > 1);
                }
                ));

            ReadTemp();

            analogTemperature.StartUpdating();
        }

        protected void ReadTemp()
        {
            Task t = new Task(async () => {
                Console.WriteLine($"Initial temp: { await analogTemperature.Read()}");
            });
            t.Start();
        }

        //protected void TestAnalogTemperature()
        //{
        //    Console.WriteLine("TestAnalogTemperature...");

        //    // Before update;
        //    analogTemperature.Update();

        //    while (true)
        //    {
        //        Console.WriteLine(analogTemperature.Temperature);
        //        Thread.Sleep(1000);
        //    }

        //    // Connect an interrupt handler.
        //    analogTemperature.TemperatureChanged += (s, e) =>
        //    {
        //        Console.WriteLine("Temperature: " + e.CurrentValue.ToString("f2"));
        //    };
        //}
    }
}