using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using Meadow.Hardware;

namespace Sensors.Atmospheric.BME280_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        BME280 _bme280;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            //TestI2cBME280(true);
            TestSpiBME280(true);
        }

        void TestI2cBME280(bool pollMode)
        {
            Console.WriteLine("TestI2cBME280...");

            var i2c = Device.CreateI2cBus();

            if (pollMode)
            {
                _bme280 = new BME280
                (
                    i2c, 
                    BME280.I2cAddress.Adddress0x77, 
                    updateInterval: 0
                );

                Console.WriteLine($"ChipID: {_bme280.GetChipID():X2}");
                Thread.Sleep(1000);
                
                Console.WriteLine("Reset");
                _bme280.Reset();

                while (true)
                {
                    _bme280.Update();
                    Console.WriteLine($"T: { _bme280.Temperature}  H: {_bme280.Humidity}  P: {_bme280.Pressure}");
                    Thread.Sleep(1000);
                }
            }
            else
            {
                // TODO:
            }
        }

        void TestSpiBME280(bool pollMode)
        {
            Console.WriteLine("TestSpiBME280...");

            var spi = Device.CreateSpiBus();

            if (pollMode)
            {
                // for now we're just tying the CS to VCC
                IDigitalOutputPort chipSelect = Device.CreateDigitalOutputPort(Device.Pins.D04);

                _bme280 = new BME280
                (
                    spi, 
                    chipSelect, 
                    updateInterval: 0
                );

                Console.WriteLine($"ChipID: {_bme280.GetChipID():X2}");
                Thread.Sleep(1000);
                
                Console.WriteLine("Reset");
                _bme280.Reset();

                while (true)
                {
                    _bme280.Update();
                    Console.WriteLine($"T: { _bme280.Temperature}  H: {_bme280.Humidity}  P: {_bme280.Pressure}");
                    Thread.Sleep(1000);
                }
            }
            else
            {
                // TODO:
            }
        }
    }
}