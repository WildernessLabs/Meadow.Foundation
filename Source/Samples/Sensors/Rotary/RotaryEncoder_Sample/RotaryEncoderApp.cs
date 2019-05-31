using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Rotary;
using System;

namespace RotaryEncoder_Sample
{
    public class RotaryEncoderApp : App<F7Micro, RotaryEncoderApp>
    {
        int value = 0;
        RotaryEncoder rotaryEncoder;

        public RotaryEncoderApp()
        {
            Console.WriteLine("Initializing...");
            rotaryEncoder = new RotaryEncoder(Device, Device.Pins.D00, Device.Pins.D01);
            rotaryEncoder.Rotated += RotaryEncoderRotated;

            Console.WriteLine("Start rotating the encoder...");
        }

        private void RotaryEncoderRotated(object sender, Meadow.Peripherals.Sensors.Rotary.RotaryTurnedEventArgs e)
        {
            if (e.Direction == Meadow.Peripherals.Sensors.Rotary.RotationDirection.Clockwise)
                value++;
            else
                value--;

            Console.WriteLine("Value = {0}", value);
        }
    }
}