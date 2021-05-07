using System;
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
            InitHardware();
        }

        public void InitHardware()
        {
            Console.WriteLine("Initialize...");

            sensor = new Adxl362(Device, Device.CreateSpiBus(), Device.Pins.D00);

            var observer = Adxl362.CreateObserver(e =>
            {
                Console.WriteLine($"X: {e.New.X.Gravity}g, Y: {e.New.Y.Gravity}g, Z: {e.New.Z.Gravity}g");
            });

            sensor.Subscribe(observer);

           // sensor.Updated += Sensor_Updated;

            sensor.StartUpdating(500);
        }

        private void Sensor_Updated(object sender, ChangeResult<Meadow.Units.Acceleration3d> e)
        {
            Console.WriteLine($"X: {e.New.X}, Y: {e.New.Y}, Z: {e.New.Z}");
        }
    }
}