using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading;

namespace BME280_Sample
{
    class BME280App : App<F7Micro, BME280App>
    {
        private BME280 _bme280;

        public BME280App()
        {
            Console.WriteLine("+BME280App");

            BME280TestI2C(true);
        }

        private void BME280TestI2C(bool pollMode)
        {
            Console.WriteLine(" BME280TestI2C");

            var i2c = Device.CreateI2cBus();

            if (pollMode)
            {
                _bme280 = new BME280(i2c, BME280.I2cAddress.Adddress0x77, updateInterval: 0);
                var i = 0;

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
