using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Buttons;

namespace Sensors.Buttons.PushButton_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected PushButton pushButton;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            pushButton = new PushButton(Device, Device.Pins.D02);
            pushButton.PressStarted += (s, e) => 
            {
                Console.WriteLine("Press started");
            };
            pushButton.PressEnded += (s, e) => 
            {
                Console.WriteLine("Press ended");
            };
            pushButton.Clicked += (s, e) => 
            {
                Console.WriteLine("Button Clicked");
            };
            pushButton.LongPressClicked += (s, e) => 
            {
                Console.WriteLine("Long pressed!");
            };

            Console.WriteLine("PushButton ready...");
        }
    }
}