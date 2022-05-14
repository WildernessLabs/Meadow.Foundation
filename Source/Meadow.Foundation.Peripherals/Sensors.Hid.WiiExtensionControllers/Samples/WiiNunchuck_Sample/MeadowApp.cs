using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;

namespace WiiNunchuck_Sample
{
    public class MeadowApp : App<F7FeatherV2, MeadowApp>
    {
        //Snip

        readonly WiiNunchuck nunchuck;

        public MeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

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

            //Start reading updates
            nunchuck.StartUpdating(TimeSpan.FromMilliseconds(200));
        }

        //Snop
    }
}