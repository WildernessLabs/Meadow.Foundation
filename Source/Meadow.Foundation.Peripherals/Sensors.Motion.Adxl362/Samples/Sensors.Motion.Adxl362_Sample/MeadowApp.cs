using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Adxl362 sensor;

        public MeadowApp()
        {
            Initialize();

            sensor.Updated += Sensor_Updated;
            sensor.Start();
        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Motion.AccelerationConditionChangeResult e)
        {
            Console.WriteLine($"X: {e.New.XAcceleration}, Y: {e.New.YAcceleration}, Z: {e.New.ZAcceleration}");
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            sensor = new Adxl362(Device, Device.CreateSpiBus(), Device.Pins.D00);
        }
    }
}