using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;

namespace Sensors.Motion.ParallaxPir_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        ParallaxPir parallaxPir;
        Led blueLed;

        public MeadowApp()
        {
            parallaxPir = new ParallaxPir(Device.CreateDigitalInputPort(Device.Pins.D05, InterruptMode.EdgeBoth, ResistorMode.Disabled));
            parallaxPir.OnMotionStart += ParallaxPirOnMotionStart;
            parallaxPir.OnMotionEnd += ParallaxPirOnMotionEnd;

            blueLed = new Led(Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue));
        }

        void ParallaxPirOnMotionEnd(object sender)
        {
            Console.WriteLine($"Motion endedt  {DateTime.Now}");
            blueLed.IsOn = true;
        }

        void ParallaxPirOnMotionStart(object sender)
        {
            Console.WriteLine($"Motion started {DateTime.Now}");
            blueLed.IsOn = false;
        }
    }
}