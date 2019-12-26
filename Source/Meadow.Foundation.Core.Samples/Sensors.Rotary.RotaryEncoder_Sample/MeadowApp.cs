using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Rotary;

namespace Sensors.Rotary.RotaryEncoder_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected int value = 0;
        protected RotaryEncoder rotaryEncoder;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            rotaryEncoder = new RotaryEncoder(Device, Device.Pins.D00, Device.Pins.D01);
            rotaryEncoder.Rotated += (s, e) => 
            {
                if (e.Direction == Meadow.Peripherals.Sensors.Rotary.RotationDirection.Clockwise)
                    value++;
                else
                    value--;

                Console.WriteLine("Value = {0}", value);
            };

            Console.WriteLine("RotaryEncoder ready...");
        }
    }
}