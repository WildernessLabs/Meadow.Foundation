using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading;

namespace Sensors.Temperature.Mcp9808_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mcp9808 mcp9808;

        public MeadowApp()
        {
            mcp9808 = new Mcp9808(Device.CreateI2cBus());

            TestUpdating();
            //TestRead();
        }

        void TestUpdating()
        {
            Console.WriteLine("TestFC28Updating...");

            var consumer = Mcp9808.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Temperature New Value { result.New.Celsius}");
                    Console.WriteLine($"Temperature Old Value { result.Old?.Celsius}");
                    //Console.WriteLine($"Temperature Delta Value { result.Delta?.Celsius}");
                },
                filter: null
            );
            mcp9808.Subscribe(consumer);

            mcp9808.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Console.WriteLine($"Moisture Updated: {e.New.Value}");
            };

            mcp9808.StartUpdating();
        }

        void TestRead()
        {
            Console.WriteLine("TestFC28Sensor...");

            while (true)
            {
                var temp = mcp9808.Read();

                Console.WriteLine($"Temperature New Value { temp.Celsius}");
                Thread.Sleep(1000);
            }
        }
    }
}