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

            sensor.Updated += Sensor_Updated;

            sensor.StartUpdating(500);

        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Motion.AccelerationConditionChangeResult e)
        {
            Console.WriteLine($"X: {e.New.XAcceleration}, Y: {e.New.YAcceleration}, Z: {e.New.ZAcceleration}");
        }

        private void Sensor_AccelerationChanged(object sender, Meadow.Foundation.Sensors.SensorVectorEventArgs e)
        {
            
        }
    }
}