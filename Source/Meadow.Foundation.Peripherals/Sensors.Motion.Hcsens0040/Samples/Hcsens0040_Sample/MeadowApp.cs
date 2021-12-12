using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace Sensors.Motion.ParallaxPir_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        //<!—SNIP—>

        Hcsens0040 sensor;

        public MeadowApp()
        {
            sensor = new Hcsens0040(Device.CreateDigitalInputPort(Device.Pins.D05));
            sensor.OnMotionDetected += Sensor_OnMotionDetected;
        }

        private void Sensor_OnMotionDetected(object sender)
        {
        
            Console.WriteLine($"Motion detected {DateTime.Now}");
        }

        //<!—SNOP—>
    }
}