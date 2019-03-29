using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;

namespace ParallaxPir_Sample
{
    public class ParallaxPirApp : AppBase<F7Micro, ParallaxPirApp>
    {
        ParallaxPir parallaxPir;

        public ParallaxPirApp()
        {
            parallaxPir = new ParallaxPir(Device.CreateDigitalInputPort(Device.Pins.D15, InterruptMode.LevelHigh, ResistorMode.PullDown));
            parallaxPir.OnMotionStart += ParallaxPirOnMotionStart;
            parallaxPir.OnMotionEnd += ParallaxPirOnMotionEnd;
        }

        void ParallaxPirOnMotionEnd(object sender)
        {
            Console.WriteLine("Motion started");

        }


        void ParallaxPirOnMotionStart(object sender)
        {
            Console.WriteLine("Motion ended");
        }
    }
}