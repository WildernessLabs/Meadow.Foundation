using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading;

namespace Ms5611_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public MeadowApp()
        {
            Console.WriteLine("Ms5611App");

            Ms5611I2cTest();
         //   Ms5611SpiTest();
        }

        private void Ms5611I2cTest()
        {
            Console.WriteLine("MS5611 I2C Test");

            var i2c = Device.CreateI2cBus();

            var sensor = new Ms5611(i2c);

            while (true)
            {
                Thread.Sleep(1000);

                Console.WriteLine(" Reading Temp...");

                sensor.ReadTemperature();

                Console.WriteLine(" Reading Pressure...");

                sensor.ReadPressure();
                
                Thread.Sleep(1000);
            }
        }

        private void Ms5611SpiTest()
        {
            Console.WriteLine("MS5611 SPI Test");

            var spi = Device.CreateSpiBus();

            var sensor = new Ms5611(spi, Device.Pins.D00);

            while (true)
            {
                Console.WriteLine(" Reading...");

                Thread.Sleep(2000);
            }
        }
    }
}
