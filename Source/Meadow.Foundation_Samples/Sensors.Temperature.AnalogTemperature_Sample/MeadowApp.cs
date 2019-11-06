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


            analogTemperature.Subscribe(new FilterableObserver<FloatChangeResult, float>(
                h => {
                    Console.WriteLine($"Temp changed by a degree; new: {h.New}, old: {h.Old}");
                },
                e => {
                    return (Math.Abs(e.Delta) > 1);
                }
                ));

            //ReadTemp().Result;

            analogTemperature.StartUpdating();
        }

        protected async Task ReadTemp()
        {
            var temp = await analogTemperature.Read();
            Console.WriteLine($"Initial temp: { temp }");
        }
    }
}