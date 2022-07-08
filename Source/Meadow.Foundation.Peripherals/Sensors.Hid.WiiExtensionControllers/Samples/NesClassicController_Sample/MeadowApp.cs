using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Hid;
using System;
using System.Threading.Tasks;

namespace NesClassicController_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        NesClassicController nesController;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            var i2cBus = Device.CreateI2cBus(NesClassicController.DefaultSpeed);

            nesController = new NesClassicController(i2cBus: i2cBus);

            //onetime update - could be used in a game loop
            nesController.Update();

            //check the state of a button
            Console.WriteLine("X Button is " + (nesController.AButton.State == true ? "pressed" : "not pressed"));

            //.NET events
            nesController.AButton.Clicked += (s, e) => Console.WriteLine("A button clicked");
            nesController.BButton.Clicked += (s, e) => Console.WriteLine("B button clicked");

            nesController.StartButton.Clicked += (s, e) => Console.WriteLine("+ button clicked");
            nesController.SelectButton.Clicked += (s, e) => Console.WriteLine("- button clicked");

            nesController.DPad.Updated += (s, e) => Console.WriteLine($"DPad {e.New}");

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            nesController.StartUpdating(TimeSpan.FromMilliseconds(200));
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}