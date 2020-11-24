using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Sensors.Light;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Tcs3472x sensor;
    
        public MeadowApp()
        {
            Initialize();

            var t = ReadColor();
        }

        public void Initialize()
        {
            Console.WriteLine("Init...");
            sensor = new Tcs3472x(Device.CreateI2cBus());

            Thread.Sleep(1000);
        }

        async Task ReadColor()
        {
            Color color;

            while(true)
            {
                color = await sensor.GetColor();

                Console.WriteLine($"Color: {color}");

                await Task.Delay(2500);
            }

        }
    }
}