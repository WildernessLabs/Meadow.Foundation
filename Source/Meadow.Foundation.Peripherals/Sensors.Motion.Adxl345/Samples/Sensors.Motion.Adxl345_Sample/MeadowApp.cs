using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Adxl345 sensor;

        public MeadowApp()
        {
            InitHardware();
        }

        public void InitHardware()
        {
            Console.WriteLine("Creating output ports...");

            //polling
            sensor = new Adxl345(Device.CreateI2cBus(), 83);

            sensor.SetPowerState(false, false, true, false, Adxl345.Frequency.EightHz);

            sensor.Updated += Sensor_Updated;

            sensor.StartUpdating(1000);
        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Motion.AccelerationConditionChangeResult e)
        {
            Console.WriteLine($"X: {e.New.XAcceleration}, Y: {e.New.YAcceleration}, Z: {e.New.ZAcceleration}");
        }
    }
}