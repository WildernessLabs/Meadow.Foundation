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

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            nunchuck = new WiiNunchuck(Device.CreateI2cBus(WiiNunchuck.DefaultSpeed));

            nunchuck.GetIdentification();

            Console.WriteLine("Update");

            //onetime update - could be used in a game loop
            nunchuck.Update();

            //check the state of a button
            Console.WriteLine("C Button is " + (nunchuck.CButton.State == true ? "pressed" : "not pressed"));

            //.NET events
            nunchuck.CButton.Clicked += (s, e) => Console.WriteLine("C button clicked");
            nunchuck.ZButton.Clicked += (s, e) => Console.WriteLine("Z button clicked");

            nunchuck.AnalogStick.Updated += (s, e) => Console.WriteLine($"Analog Stick {e.New.Horizontal}, {e.New.Vertical}");

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