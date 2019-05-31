using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Rotary;
using System;

namespace RotaryEncoderWithButton_Sample
{
    public class RotaryEncoderWithButtonApp : App<F7Micro, RotaryEncoderWithButtonApp>
    {
        int value = 0;
        RotaryEncoderWithButton rotaryEncoderWithButton;

        public RotaryEncoderWithButtonApp()
        {
            Console.WriteLine("Initializing...");
            rotaryEncoderWithButton = new RotaryEncoderWithButton(Device, Device.Pins.D00, Device.Pins.D01, Device.Pins.D02);
            rotaryEncoderWithButton.Rotated += RotaryEncoderRotated;
            rotaryEncoderWithButton.Clicked += RotaryEncoderButtonClicked;            
            rotaryEncoderWithButton.PressEnded += RotaryEncoderButtonPressEnded;
            rotaryEncoderWithButton.PressStarted += RotaryEncoderButtonPressStarted;

            Console.WriteLine("Start rotating the encoder or press the button...");
        }

        private void RotaryEncoderRotated(object sender, Meadow.Peripherals.Sensors.Rotary.RotaryTurnedEventArgs e)
        {
            if (e.Direction == Meadow.Peripherals.Sensors.Rotary.RotationDirection.Clockwise)
                value++;
            else
                value--;

            Console.WriteLine("Value = {0}", value);
        }

        private void RotaryEncoderButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine("Button Clicked");
        }

        private void RotaryEncoderButtonPressEnded(object sender, EventArgs e)
        {
            Console.WriteLine("Press ended");
        }

        private void RotaryEncoderButtonPressStarted(object sender, EventArgs e)
        {
            Console.WriteLine("Press started");
        }
    }
}