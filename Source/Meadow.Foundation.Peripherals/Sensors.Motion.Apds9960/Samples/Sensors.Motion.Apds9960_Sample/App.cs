using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

//This driver is untested and may not be working
namespace BasicSensors.Motion.Apds9960_Sample
{
    public class App : App<F7Micro, App>
    {
        Apds9960 sensor;

        public App()
        {
            TestNewDriver();
        }

        void TestOldDriver()
        {
            var sensor = new Apds9960O(Device, Device.CreateI2cBus(), Device.Pins.D04);

            sensor.Enable(true);

            

        }

        void TestNewDriver()
        { 
            InitHardware();

            while (true)
            {
                if(sensor.IsGestureAvailable())
                {
                    Console.WriteLine($"Gesture: {sensor.ReadGesture()}");
                }
                else
                {
                    Console.WriteLine("No gesture detected");
                }

                Thread.Sleep(5000);
            }
        }

        public void InitHardware()
        {
            Console.WriteLine("Creating Outputs...");

            sensor = new Apds9960(Device, Device.CreateI2cBus(), Device.Pins.D04);

            Console.WriteLine("EnabledGestureSensor");
            sensor.EnableGestureSensor(false);
        }
    }
}