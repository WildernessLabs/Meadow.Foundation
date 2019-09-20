using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio.Radio;
using Meadow.Hardware;

namespace BasicTEA5767
{
    public class TEA5767App : App<F7Micro, TEA5767App>
    {
        public TEA5767App()
        {
            Console.WriteLine("Initialize App");

            Console.WriteLine("Create I2C bus");
            var i2cBus = Device.CreateI2cBus();

            Console.WriteLine("Create TEA5767 instance");
            var radio = new TEA5767(i2cBus);
            

            Console.WriteLine($"Current frequency: {radio.GetFrequency()}");

            for (int i = 0; i < 8; i++)
            {
                Thread.Sleep(1000);

                radio.SearchNextSilent();

                Console.WriteLine($"Current frequency: {radio.GetFrequency()}");
            }

            radio.SelectFrequency(94.5f);
        }
    }
}