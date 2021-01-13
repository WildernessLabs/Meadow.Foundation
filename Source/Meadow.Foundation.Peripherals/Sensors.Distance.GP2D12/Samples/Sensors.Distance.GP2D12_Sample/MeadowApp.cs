using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Gp2D12 sensor;

        public MeadowApp()
        {
            Initalize();

            sensor.DistanceDetected += Sensor_DistanceDetected;
        }

        private void Sensor_DistanceDetected(object sender, Meadow.Peripherals.Sensors.Distance.DistanceEventArgs e)
        {
            Console.WriteLine($"Distance: {e.Distance}");
        }

        public void Initalize()
        {
            Console.WriteLine("Initialize...");

            sensor = new Gp2D12(Device, Device.Pins.A01);
        }
    }
}