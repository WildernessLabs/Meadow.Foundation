using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using static Meadow.Peripherals.Leds.IRgbLed;

namespace Leds.RgbPwmLed_Onboard_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        RgbPwmLed _onboard;

        public MeadowApp()
        {
            ConfigurePeripherals();
            TestSomeColors();
            RunColors();
        }

        public void ConfigurePeripherals()
        {
            Console.WriteLine("Creating peripherals...");
            this._onboard = new RgbPwmLed(
                Device,
                Device.Pins.OnboardLedRed,
                Device.Pins.OnboardLedGreen,
                Device.Pins.OnboardLedBlue,
                commonType: CommonType.CommonAnode);            
        }

        public void TestSomeColors()
        {
            _onboard.SetColor(Color.Crimson);
            Thread.Sleep(3000);
            _onboard.SetColor(Color.MediumPurple);
            Thread.Sleep(3000);
            _onboard.SetColor(Color.FromHex("#23abe3"));
        }

        public void RunColors()
        {
            while (true) {

                // loop through the entire hue spectrum (360 degrees)
                for (int i = 0; i < 360; i++) {
                    var hue = ((double)i / 360F);
                    Console.WriteLine($"Hue: {hue}");

                    // set the color of the RGB
                    _onboard.SetColor(Color.FromHsba((hue), 1, 1));

                    // for a fun, fast rotation through the hue spectrum:
                    //Thread.Sleep (1);
                    // for a gentle walk through the forest of colors;
                    Thread.Sleep(18);
                }
            }
        }
    }
}