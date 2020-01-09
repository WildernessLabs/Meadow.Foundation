using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Light;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Bh1750 sensor;

        public MeadowApp()
        {
            Init();

            Task.Run(async () =>
            {
                while(true)
                {
                    Console.WriteLine($"Illuminance: {sensor.GetIlluminance()}lux");
                    await Task.Delay(1000);
                }
            });
        }

        public void Init()
        {
            Console.WriteLine("Init...");

            sensor = new Bh1750(Device.CreateI2cBus(), Bh1750.I2cAddressLow);
        }
    }
}