using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Adxl335 sensor;

        public MeadowApp()
        {
            InitHardware();
        }

        public void InitHardware()
        {
            Console.WriteLine("Initialize...");

            sensor = new Adxl335(Device, Device.Pins.A00, Device.Pins.A01, Device.Pins.A02);

            var observer = Adxl335.CreateObserver(e =>
            {
                Console.WriteLine($"X: {e.New.AccelerationX.Gravity}g, Y: {e.New.AccelerationY.Gravity}g, Z: {e.New.AccelerationZ.Gravity}g");
            });

            sensor.Subscribe(observer);

           // sensor.Updated += Sensor_Updated;

            sensor.StartUpdating(500);
        }

        private void Sensor_Updated(object sender, CompositeChangeResult<Meadow.Units.Acceleration3d> e)
        {
            Console.WriteLine($"X: {e.New.AccelerationX}, Y: {e.New.AccelerationY}, Z: {e.New.AccelerationZ}");
        }
    }
}