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
    }
}