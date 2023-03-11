using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;
using System.Threading.Tasks;

namespace WiiClassicController_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        WiiClassicController classicController;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize...");

            var i2cBus = Device.CreateI2cBus(WiiClassicController.DefaultSpeed);

            classicController = new WiiClassicController(i2cBus: i2cBus, 
                                                         useHighResolutionMode: true);

            //onetime update - could be used in a game loop
            classicController.Update();

            //check the state of a button
            Resolver.Log.Info("X Button is " + (classicController.XButton.State == true ? "pressed" : "not pressed"));

            //.NET events
            classicController.AButton.Clicked += (s, e) => Resolver.Log.Info("A button clicked");
            classicController.BButton.Clicked += (s, e) => Resolver.Log.Info("B button clicked");
            classicController.XButton.Clicked += (s, e) => Resolver.Log.Info("X button clicked");
            classicController.YButton.Clicked += (s, e) => Resolver.Log.Info("Y button clicked");

            classicController.LButton.Clicked += (s, e) => Resolver.Log.Info("L button clicked");
            classicController.RButton.Clicked += (s, e) => Resolver.Log.Info("R button clicked");
            classicController.ZLButton.Clicked += (s, e) => Resolver.Log.Info("ZL button clicked");
            classicController.ZRButton.Clicked += (s, e) => Resolver.Log.Info("ZR button clicked");

            classicController.PlusButton.Clicked += (s, e) => Resolver.Log.Info("+ button clicked");
            classicController.MinusButton.Clicked += (s, e) => Resolver.Log.Info("- button clicked");
            classicController.HomeButton.Clicked += (s, e) => Resolver.Log.Info("Home button clicked");

            classicController.DPad.Updated += (s, e) => Resolver.Log.Info($"DPad {e.New}");

            classicController.LeftTrigger.Updated += (s, e) => Resolver.Log.Info($"Left Trigger {e.New}");
            classicController.RightTrigger.Updated += (s, e) => Resolver.Log.Info($"Left Trigger {e.New}");

            classicController.LeftAnalogStick.Updated += (s, e) => Resolver.Log.Info($"Left Analog Stick {e.New.Horizontal}, {e.New.Vertical}");
            classicController.RightAnalogStick.Updated += (s, e) => Resolver.Log.Info($"Right Analog Stick {e.New.Horizontal}, {e.New.Vertical}");

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            classicController.StartUpdating(TimeSpan.FromMilliseconds(200));
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}