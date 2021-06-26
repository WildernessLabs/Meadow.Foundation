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
            Console.WriteLine("TestUpdating...");

            var consumer = Mcp9808.CreateObserver(
                handler: result =>
                {
                    Console.WriteLine($"Temperature New Value { result.New.Celsius}C");
                    Console.WriteLine($"Temperature Old Value { result.Old?.Celsius}C");
                },
                filter: null
            );
            mcp9808.Subscribe(consumer);

            mcp9808.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Console.WriteLine($"Temperature Updated: {e.New.Celsius:N2}C");
            };

            mcp9808.StartUpdating(TimeSpan.FromSeconds(1));
        }

        void TestRead()
        {
            Console.WriteLine("TestMcp9808Sensor...");

            while (true)
            {
                var temp = mcp9808.Read().Result;

                Console.WriteLine($"Temperature New Value {temp.Celsius}C");
                Thread.Sleep(1000);
            }
        }
    }
}