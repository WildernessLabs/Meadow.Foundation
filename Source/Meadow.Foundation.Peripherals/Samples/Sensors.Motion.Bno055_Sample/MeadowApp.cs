using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Bno055 sensor;

        public MeadowApp()
        {
            InitHardware();

            while (true)
            {
                sensor.Read();

                Console.WriteLine($"{sensor.Temperature}");

                Thread.Sleep(1000);
            }
        }

        public void InitHardware()
        {
            Console.WriteLine("Creating output ports...");

            sensor = new Bno055(Device.CreateI2cBus(), 0x28);

            sensor.DisplayRegisters();
            sensor.PowerMode = Bno055.PowerModes.Normal;
            sensor.OperatingMode = Bno055.OperatingModes.ConfigurationMode;
            sensor.OperatingMode = Bno055.OperatingModes.InertialMeasurementUnit;
            sensor.DisplayRegisters();
        }
    }
}