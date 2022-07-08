using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace Sensors.Motion.ParallaxPir_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Hcsens0040 sensor;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            sensor = new Hcsens0040(Device.CreateDigitalInputPort(Device.Pins.D05));
            sensor.OnMotionDetected += Sensor_OnMotionDetected;

            return Task.CompletedTask;
        }

        private void Sensor_OnMotionDetected(object sender)
        {
            Console.WriteLine($"Motion detected {DateTime.Now}");
        }

        //<!=SNOP=>
    }
}