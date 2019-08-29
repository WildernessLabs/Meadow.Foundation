using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Barometric;
using System;
using System.Threading;

namespace GY63_Sample
{
    public class GY63App : App<F7Micro, GY63App>
    {
        public GY63App()
        {
            Console.WriteLine("+GY63App");

            GY63TestI2C();
//            GY63TestSPI();
        }

        private void GY63TestI2C()
        {
            Console.WriteLine("+GY63 I2C Test");

            var i2c = Device.CreateI2cBus();

            var sensor = new GY63(i2c);

            while (true)
            {
                Console.WriteLine(" Reading...");

                Thread.Sleep(2000);
            }
        }

        private void GY63TestSPI()
        {
            Console.WriteLine("+GY63 SPI Test");

            var spi = Device.CreateSpiBus();

            var sensor = new GY63(spi);

            while (true)
            {
                Console.WriteLine(" Reading...");

                Thread.Sleep(2000);
            }
        }
    }
}
