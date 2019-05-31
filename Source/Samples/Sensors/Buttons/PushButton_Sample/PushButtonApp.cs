using System;
using Meadow;
using Meadow.Hardware;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Buttons;

namespace PushButton_Sample
{
    public class PushButtonApp : App<F7Micro, PushButtonApp>
    {
        PushButton pushButton;

        public PushButtonApp()
        {
            pushButton = new PushButton(Device, Device.Pins.D02);

            pushButton.PressStarted += (s, e) => {
                Console.WriteLine("Press started");
            };

            pushButton.PressEnded += (s, e) => {
                Console.WriteLine("Press ended");
            };

            pushButton.Clicked += (s, e) => {
                Console.WriteLine("Button Clicked");
            };

            pushButton.LongPressClicked += (s, e) => {
                Console.WriteLine("Long pressed!");
            };
        }
    }
}
