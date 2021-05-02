using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading;

namespace Sensors.Temperature.Lm75_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Lm75 lm75;

        public MeadowApp()
        {
            lm75 = new Lm75(Device.CreateI2cBus());

            TestUpdating();
            //TestRead();
        }

        void TestUpdating()
        {
            Console.WriteLine("TestFC28Updating...");

            var consumer = Lm75.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Temperature New Value { result.New.Celsius}");
                    Console.WriteLine($"Temperature Old Value { result.Old.Celsius}");
                    Console.WriteLine($"Temperature Delta Value { result.Delta.Celsius}");
                },
                filter: null
            );
            lm75.Subscribe(consumer);

            lm75.TemperatureUpdated += (object sender, CompositeChangeResult<Meadow.Units.Temperature> e) =>
            {
                Console.WriteLine($"Moisture Updated: {e.New.Value}");
            };

            lm75.StartUpdating();
        }

        void TestRead()
        {
            Console.WriteLine("TestFC28Sensor...");

            while (true)
            {
                var temp = lm75.Read();

                Console.WriteLine($"Temperature New Value { temp.Celsius}");                
                Thread.Sleep(1000);
            }
        }
    }
}