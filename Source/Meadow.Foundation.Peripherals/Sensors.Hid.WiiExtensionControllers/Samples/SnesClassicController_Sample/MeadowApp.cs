using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;
using System.Threading.Tasks;

namespace SnesClassicController_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        SnesClassicController snesController;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
        
            var i2cBus = Device.CreateI2cBus(SnesClassicController.DefaultI2cSpeed);

            snesController = new SnesClassicController(i2cBus: i2cBus);

            //onetime update - could be used in a game loop
            snesController.Update();

            //check the state of a button
            Resolver.Log.Info("X Button is " + (snesController.XButton.State == true ? "pressed" : "not pressed"));

            //.NET events
            snesController.AButton.Clicked += (s, e) => Resolver.Log.Info("A button clicked");
            snesController.BButton.Clicked += (s, e) => Resolver.Log.Info("B button clicked");
            snesController.XButton.Clicked += (s, e) => Resolver.Log.Info("X button clicked");
            snesController.YButton.Clicked += (s, e) => Resolver.Log.Info("Y button clicked");

            snesController.LButton.Clicked += (s, e) => Resolver.Log.Info("L button clicked");
            snesController.RButton.Clicked += (s, e) => Resolver.Log.Info("R button clicked");

            snesController.StartButton.Clicked += (s, e) => Resolver.Log.Info("+ button clicked");
            snesController.SelectButton.Clicked += (s, e) => Resolver.Log.Info("- button clicked");

            snesController.DPad.Updated += (s, e) => Resolver.Log.Info($"DPad {e.New}");

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            snesController.StartUpdating(TimeSpan.FromMilliseconds(200));
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}