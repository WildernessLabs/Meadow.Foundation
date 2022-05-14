using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;

namespace WiiClassicController_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //<!=SNIP=>

        readonly WiiClassicController classicController;

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");
            var i2cBus = Device.CreateI2cBus(WiiClassicController.DefaultSpeed);

            classicController = new WiiClassicController(i2cBus: i2cBus, 
                                                         useHighResolutionMode: true);

            //onetime update - could be used in a game loop
            classicController.Update();

            //check the state of a button
            Console.WriteLine("X Button is " + (classicController.XButton.State == true ? "pressed" : "not pressed"));

            //.NET events
            classicController.AButton.Clicked += (s, e) => Console.WriteLine("A button clicked");
            classicController.BButton.Clicked += (s, e) => Console.WriteLine("B button clicked");
            classicController.XButton.Clicked += (s, e) => Console.WriteLine("X button clicked");
            classicController.YButton.Clicked += (s, e) => Console.WriteLine("Y button clicked");

            classicController.LButton.Clicked += (s, e) => Console.WriteLine("L button clicked");
            classicController.RButton.Clicked += (s, e) => Console.WriteLine("R button clicked");
            classicController.ZLButton.Clicked += (s, e) => Console.WriteLine("ZL button clicked");
            classicController.ZRButton.Clicked += (s, e) => Console.WriteLine("ZR button clicked");

            classicController.PlusButton.Clicked += (s, e) => Console.WriteLine("+ button clicked");
            classicController.MinusButton.Clicked += (s, e) => Console.WriteLine("- button clicked");
            classicController.HomeButton.Clicked += (s, e) => Console.WriteLine("Home button clicked");

            classicController.DPad.Updated += (s, e) => Console.WriteLine($"DPad {e.New}");

           
            classicController.LeftTrigger.Updated += (s, e) => Console.WriteLine($"Left Trigger {e.New}");
            classicController.RightTrigger.Updated += (s, e) => Console.WriteLine($"Left Trigger {e.New}");

            classicController.LeftAnalogStick.Updated += (s, e) => Console.WriteLine($"Left Analog Stick {e.New.Horizontal}, {e.New.Vertical}");
            classicController.RightAnalogStick.Updated += (s, e) => Console.WriteLine($"Right Analog Stick {e.New.Horizontal}, {e.New.Vertical}");
            

            //Start reading updates
            classicController.StartUpdating(TimeSpan.FromMilliseconds(200));
        }

        //<!=SNOP=>
    }
}