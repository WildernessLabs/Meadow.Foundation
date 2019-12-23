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

            //poling
            sensor = new Adxl345(Device.CreateI2cBus(), 83, 0);

            sensor.SetPowerState(false, false, true, false, Adxl345.Frequency.EightHz);

            while (true)
            {
                sensor.Update();

                Console.WriteLine($"{sensor.X}, {sensor.Y}, {sensor.Z}");

                Thread.Sleep(500);
            } 

            //event
            /*sensor = new Adxl345(Device.CreateI2cBus(), 83);

            sensor.SetPowerState(false, false, true, false, Adxl345.Frequency.EightHz);

            sensor.AccelerationChanged += Sensor_AccelerationChanged;
            */
        }

        private void Sensor_AccelerationChanged(object sender, Meadow.Foundation.Sensors.SensorVectorEventArgs e)
        {
            Console.WriteLine($"Accel change {e.CurrentValue}");
        }
    }
}