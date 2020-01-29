using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Motion;

namespace Sensors.Motion.ParallaxPir_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Hcsens0040 sensor;
        Led blueLed;

        public MeadowApp()
        {
            sensor = new Hcsens0040(Device.CreateDigitalInputPort(Device.Pins.D05));
            sensor.OnMotionDetected += ParallaxPirOnMotionStart;

            blueLed = new Led(Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue));
        }

        //here's an example where it's ok to use async void
        //the method signature is required to be void because of the event
        async void ParallaxPirOnMotionStart(object sender)
        {
            Console.WriteLine($"Motion started {DateTime.Now}");
            blueLed.IsOn = true;

            await Task.Delay(2000);

            blueLed.IsOn = false;
        }
    }
}