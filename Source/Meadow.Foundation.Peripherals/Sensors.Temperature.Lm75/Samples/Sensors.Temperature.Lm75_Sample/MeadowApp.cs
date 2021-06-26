using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sensors.Temperature.Lm75_Sample
{
    // TODO: this sample needs to be updated.
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Lm75 lm75;

        public MeadowApp()
        {
            lm75 = new Lm75(Device.CreateI2cBus());

            TestRead().Wait();

            TestUpdating();
        }

        void TestUpdating()
        {
            Console.WriteLine("TestUpdating...");

            var consumer = Lm75.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Temperature New Value { result.New.Celsius}C");
                    Console.WriteLine($"Temperature Old Value { result.Old?.Celsius}C");
                    //Console.WriteLine($"Temperature Delta Value { result.Delta?.Celsius}");
                },
                filter: null
            );
            lm75.Subscribe(consumer);

            lm75.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Console.WriteLine($"Temperature Updated: {e.New.Celsius:n2}C");
            };

            lm75.StartUpdating();
        }

        async Task TestRead()
        {
            Console.WriteLine("TestRead...");

            var temp = await lm75.Read();

            Console.WriteLine($"Temperature New Value { temp.Celsius}");                
            Thread.Sleep(1000);
        }
    }
}