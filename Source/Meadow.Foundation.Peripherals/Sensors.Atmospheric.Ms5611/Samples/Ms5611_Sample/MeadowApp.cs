using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading;

namespace Ms5611_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!—SNIP—>

        Ms5611 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            //CreateSpiSensor();
            CreateI2CSensor();

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

        void CreateI2CSensor()
        {
            Console.WriteLine("MS5611 I2C Test");

            sensor = new Ms5611(Device.CreateI2cBus());
        }

        void CreateSpiSensor()
        {
            Console.WriteLine("MS5611 SPI Test");

            sensor = new Ms5611(Device.CreateSpiBus(), Device.Pins.D00);
        }

        //<!—SNOP—>
    }
}
