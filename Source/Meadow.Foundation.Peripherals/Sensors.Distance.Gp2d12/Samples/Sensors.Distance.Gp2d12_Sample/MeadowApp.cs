using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Gp2d12 sensor;

        public MeadowApp()
        {
            Initalize();

            sensor.DistanceUpdated += Sensor_DistanceUpdated;
        }

        private void Sensor_DistanceUpdated(object sender, ChangeResult<Meadow.Units.Length> e)
        {
            Console.WriteLine($"Distance: {e.New.Centimeters}cm");
        }

        public void Initalize()
        {
            Console.WriteLine("Initialize...");

            sensor = new Gp2d12(Device, Device.Pins.A01);
        }
    }
}