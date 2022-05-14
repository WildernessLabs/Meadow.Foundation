using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;

namespace WiiClassicControllerPro_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //Snip

        readonly WiiClassicControllerPro classicControllerPro;

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");
            var i2cBus = Device.CreateI2cBus(WiiClassicControllerPro.DefaultSpeed);

            classicControllerPro = new WiiClassicControllerPro(i2cBus: i2cBus, 
                                                         useHighResolutionMode: true);

            //onetime update - could be used in a game loop
            classicControllerPro.Update();

            //check the state of a button
            Console.WriteLine("X Button is " + (classicControllerPro.XButton.State == true ? "pressed" : "not pressed"));

            //.NET events
            classicControllerPro.AButton.Clicked += (s, e) => Console.WriteLine("A button clicked");
            classicControllerPro.BButton.Clicked += (s, e) => Console.WriteLine("B button clicked");
            classicControllerPro.XButton.Clicked += (s, e) => Console.WriteLine("X button clicked");
            classicControllerPro.YButton.Clicked += (s, e) => Console.WriteLine("Y button clicked");

            classicControllerPro.LButton.Clicked += (s, e) => Console.WriteLine("L button clicked");
            classicControllerPro.RButton.Clicked += (s, e) => Console.WriteLine("R button clicked");
            classicControllerPro.ZLButton.Clicked += (s, e) => Console.WriteLine("ZL button clicked");
            classicControllerPro.ZRButton.Clicked += (s, e) => Console.WriteLine("ZR button clicked");

            classicControllerPro.PlusButton.Clicked += (s, e) => Console.WriteLine("+ button clicked");
            classicControllerPro.MinusButton.Clicked += (s, e) => Console.WriteLine("- button clicked");
            classicControllerPro.HomeButton.Clicked += (s, e) => Console.WriteLine("Home button clicked");

            classicControllerPro.DPad.Updated += (s, e) => Console.WriteLine($"DPad {e.New}");

            classicControllerPro.LeftAnalogStick.Updated += (s, e) => Console.WriteLine($"Left Analog Stick {e.New.Horizontal}, {e.New.Vertical}");
            classicControllerPro.RightAnalogStick.Updated += (s, e) => Console.WriteLine($"Right Analog Stick {e.New.Horizontal}, {e.New.Vertical}");

            //Start reading updates
            classicControllerPro.StartUpdating(TimeSpan.FromMilliseconds(200));
        }

        //Snop
    }
}