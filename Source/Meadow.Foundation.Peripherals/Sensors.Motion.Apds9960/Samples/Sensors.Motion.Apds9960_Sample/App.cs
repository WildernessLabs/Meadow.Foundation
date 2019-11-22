using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;

//This driver is untested and may not be working
namespace BasicSensors.Motion.Apds9960_Sample
{
    public class App : App<F7Micro, App>
    {
        Apds9960 sensor;

        public App()
        {
            InitHardware();
        }

        public void InitHardware()
        {
            Console.WriteLine("Creating Outputs...");

            sensor = new Apds9960(Device, Device.CreateI2cBus(), Device.Pins.D04);
            sensor.Enable(true);
        }
    }
}