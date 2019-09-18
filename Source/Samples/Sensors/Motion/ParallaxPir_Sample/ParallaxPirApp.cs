using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;

namespace ParallaxPir_Sample
{
    public class ParallaxPirApp : App<F7Micro, ParallaxPirApp>
    {
        ParallaxPir parallaxPir;
        IDigitalOutputPort blueLed;

        public ParallaxPirApp()
        {
            parallaxPir = new ParallaxPir(Device.CreateDigitalInputPort(Device.Pins.D05, InterruptMode.EdgeBoth, ResistorMode.Disabled));
            parallaxPir.OnMotionStart += ParallaxPirOnMotionStart;
            parallaxPir.OnMotionEnd += ParallaxPirOnMotionEnd;

            blueLed = Device.CreateDigitalOutputPort(Device.Pins.OnboardLedBlue);
        }

        void ParallaxPirOnMotionEnd(object sender)
        {
            Console.WriteLine($"Motion endedt  {DateTime.Now}");
            blueLed.State = true;

        }


        void ParallaxPirOnMotionStart(object sender)
        {
            Console.WriteLine($"Motion started {DateTime.Now}");
            blueLed.State = false;
        }
    }
}