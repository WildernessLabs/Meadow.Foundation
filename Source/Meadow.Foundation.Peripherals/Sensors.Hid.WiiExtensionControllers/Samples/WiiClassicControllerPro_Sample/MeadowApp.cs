using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;
using System.Threading.Tasks;

namespace WiiClassicControllerPro_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        WiiClassicControllerPro classicControllerPro;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var i2cBus = Device.CreateI2cBus(WiiClassicControllerPro.DefaultI2cSpeed);

            classicControllerPro = new WiiClassicControllerPro(i2cBus: i2cBus,
                                                         useHighResolutionMode: true);

            //onetime update - could be used in a game loop
            classicControllerPro.Update();

            //check the state of a button
            Resolver.Log.Info("X Button is " + (classicControllerPro.XButton.State == true ? "pressed" : "not pressed"));

            //.NET events
            classicControllerPro.AButton.Clicked += (s, e) => Resolver.Log.Info("A button clicked");
            classicControllerPro.BButton.Clicked += (s, e) => Resolver.Log.Info("B button clicked");
            classicControllerPro.XButton.Clicked += (s, e) => Resolver.Log.Info("X button clicked");
            classicControllerPro.YButton.Clicked += (s, e) => Resolver.Log.Info("Y button clicked");

            classicControllerPro.LButton.Clicked += (s, e) => Resolver.Log.Info("L button clicked");
            classicControllerPro.RButton.Clicked += (s, e) => Resolver.Log.Info("R button clicked");
            classicControllerPro.ZLButton.Clicked += (s, e) => Resolver.Log.Info("ZL button clicked");
            classicControllerPro.ZRButton.Clicked += (s, e) => Resolver.Log.Info("ZR button clicked");

            classicControllerPro.PlusButton.Clicked += (s, e) => Resolver.Log.Info("+ button clicked");
            classicControllerPro.MinusButton.Clicked += (s, e) => Resolver.Log.Info("- button clicked");
            classicControllerPro.HomeButton.Clicked += (s, e) => Resolver.Log.Info("Home button clicked");

            classicControllerPro.DPad.Updated += (s, e) => Resolver.Log.Info($"DPad {e.New}");

            classicControllerPro.LeftAnalogStick.Updated += (s, e) => Resolver.Log.Info($"Left Analog Stick {e.New.Horizontal}, {e.New.Vertical}");
            classicControllerPro.RightAnalogStick.Updated += (s, e) => Resolver.Log.Info($"Right Analog Stick {e.New.Horizontal}, {e.New.Vertical}");

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            classicControllerPro.StartUpdating(TimeSpan.FromMilliseconds(200));
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}