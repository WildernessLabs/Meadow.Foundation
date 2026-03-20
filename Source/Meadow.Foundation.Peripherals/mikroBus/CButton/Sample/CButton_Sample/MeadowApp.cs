using Meadow;
using Meadow.Devices;
using Meadow.Foundation.mikroBUS.Sensors.Buttons;
using System;

namespace CButton_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        CButton ledButton;

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");

            ledButton = new CButton(Device.Pins.D03, Device.Pins.D04);

            ledButton.StartPulse(TimeSpan.FromSeconds(2), 0.75f, 0);
            ledButton.Clicked += (s, e) =>
            {
                Console.WriteLine("Button clicked");
                ledButton.IsOn = !ledButton.IsOn;
            };
        }

        //<!=SNOP=>
    }
}