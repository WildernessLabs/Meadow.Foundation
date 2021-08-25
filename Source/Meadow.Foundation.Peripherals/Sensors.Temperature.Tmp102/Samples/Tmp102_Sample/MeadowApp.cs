using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;

namespace Sensors.Temperature.Tmp102_Sample
{
    // TODO: This sample needs a rewrite. See the other atmospheric samples for
    // an example of the sample pattern.

    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //<!—SNIP—>

        Tmp102 tmp102;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            tmp102 = new Tmp102(Device.CreateI2cBus());

            var consumer = Tmp102.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Temperature New Value { result.New.Celsius}C");
                    Console.WriteLine($"Temperature Old Value { result.Old?.Celsius}C");
                },
                filter: null
            );
            tmp102.Subscribe(consumer);

            tmp102.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Console.WriteLine($"Temperature Updated: {e.New.Celsius:N2}C");
            };

            tmp102.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!—SNOP—>

        void TestRead()
        {
            Console.WriteLine("TestTmp102Sensor...");

            while (true)
            {
                var temp = tmp102.Read().Result;

                Console.WriteLine($"Temperature New Value { temp.Celsius}C");
                Thread.Sleep(1000);
            }
        }
    }
}