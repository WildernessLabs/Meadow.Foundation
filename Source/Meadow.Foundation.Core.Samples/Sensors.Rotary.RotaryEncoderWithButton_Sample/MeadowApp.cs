using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Rotary;

namespace Sensors.Rotary.RotaryEncoderWithButton_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected int value = 0;
        protected RotaryEncoderWithButton rotaryEncoderWithButton;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            rotaryEncoderWithButton = new RotaryEncoderWithButton(Device, Device.Pins.D08, Device.Pins.D07, Device.Pins.D06); /// Changed to working pins. D14 causes issues.
            rotaryEncoderWithButton.Rotated += (s, e) =>
            {
                if (e.Direction == Meadow.Peripherals.Sensors.Rotary.RotationDirection.Clockwise)
                { value++; }
                else
                {   value--; }

                Console.WriteLine("Value = {0}", value);
            };
            rotaryEncoderWithButton.Clicked += (s, e) =>
            {
                Console.WriteLine("Button Clicked");
            };
            rotaryEncoderWithButton.PressEnded += (s, e) =>
            {
                Console.WriteLine("Press ended");
            };
            rotaryEncoderWithButton.PressStarted += (s, e) =>
            {
                Console.WriteLine("Press started");
            };

            Console.WriteLine("RotaryEncoderWithButton ready...");
        }
    }
}