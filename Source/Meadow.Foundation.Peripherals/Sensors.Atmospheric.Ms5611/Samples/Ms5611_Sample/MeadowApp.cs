using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ms5611_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ms5611 sensor;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing...");

            //CreateSpiSensor();
            CreateI2CSensor();

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            while (true)
            {
                Thread.Sleep(1000);

                Console.WriteLine(" Reading Temp...");

                sensor.ReadTemperature();

                Console.WriteLine(" Reading Pressure...");

                sensor.ReadPressure();

                Thread.Sleep(1000);
            }

            return Task.CompletedTask;
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

        //<!=SNOP=>
    }
}
