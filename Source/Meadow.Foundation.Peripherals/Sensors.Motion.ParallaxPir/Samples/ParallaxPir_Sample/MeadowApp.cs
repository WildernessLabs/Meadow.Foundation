using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Hardware;

namespace Sensors.Motion.ParallaxPir_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        ParallaxPir parallaxPir;

        public MeadowApp()
        {
            parallaxPir = new ParallaxPir(Device.CreateDigitalInputPort(Device.Pins.D05, InterruptMode.EdgeBoth, ResistorMode.Disabled));
            parallaxPir.OnMotionStart += (sender)=> Console.WriteLine($"Motion start  {DateTime.Now}");
            parallaxPir.OnMotionEnd += (sender) => Console.WriteLine($"Motion end  {DateTime.Now}");
        }

        //<!=SNOP=>
    }
}