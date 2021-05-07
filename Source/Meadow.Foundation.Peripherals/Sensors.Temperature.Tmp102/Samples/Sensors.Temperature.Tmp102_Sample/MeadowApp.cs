using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;

namespace Sensors.Temperature.Tmp102_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Tmp102 tmp102;

        public MeadowApp()
        {
            tmp102 = new Tmp102(Device.CreateI2cBus());

            TestUpdating();
            //TestRead();
        }

        void TestUpdating()
        {
            Console.WriteLine("TestFC28Updating...");

            var consumer = Tmp102.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Temperature New Value { result.New.Celsius}");
                    Console.WriteLine($"Temperature Old Value { result.Old?.Celsius}");
                    //Console.WriteLine($"Temperature Delta Value { result.Delta?.Celsius}");
                },
                filter: null
            );
            tmp102.Subscribe(consumer);

            tmp102.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Console.WriteLine($"Moisture Updated: {e.New.Value}");
            };

            tmp102.StartUpdating();
        }

        void TestRead()
        {
            Console.WriteLine("TestFC28Sensor...");

            while (true)
            {
                var temp = tmp102.Read();

                Console.WriteLine($"Temperature New Value { temp.Celsius}");
                Thread.Sleep(1000);
            }
        }
    }
}