using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;
using System.Threading.Tasks;

namespace WiiNunchuck_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        WiiNunchuck nunchuck;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize...");

            nunchuck = new WiiNunchuck(Device.CreateI2cBus(WiiNunchuck.DefaultSpeed));

            nunchuck.GetIdentification();

            Resolver.Log.Info("Update");

            //onetime update - could be used in a game loop
            nunchuck.Update();

            //check the state of a button
            Resolver.Log.Info("C Button is " + (nunchuck.CButton.State == true ? "pressed" : "not pressed"));

            //.NET events
            nunchuck.CButton.Clicked += (s, e) => Resolver.Log.Info("C button clicked");
            nunchuck.ZButton.Clicked += (s, e) => Resolver.Log.Info("Z button clicked");

            nunchuck.AnalogStick.Updated += (s, e) => Resolver.Log.Info($"Analog Stick {e.New.Horizontal}, {e.New.Vertical}");

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            nunchuck.StartUpdating(TimeSpan.FromMilliseconds(200));
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}