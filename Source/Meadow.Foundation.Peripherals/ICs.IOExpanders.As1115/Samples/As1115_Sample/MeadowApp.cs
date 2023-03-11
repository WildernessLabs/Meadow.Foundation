using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using System;
using System.Threading.Tasks;
using static Meadow.Foundation.ICs.IOExpanders.As1115;

namespace ICs.IOExpanders.As1115_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        As1115 as1115;
        MicroGraphics graphics;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize...");
            as1115 = new As1115(Device.CreateI2cBus(), Device.Pins.D03);

            //general key scan events - will raise for all buttons
            as1115.KeyScanPressStarted += KeyScanPressStarted;

            //or access buttons as IButtons individually
            as1115.KeyScanButtons[KeyScanButtonType.Button1].LongClickedThreshold = TimeSpan.FromSeconds(1);
            as1115.KeyScanButtons[KeyScanButtonType.Button1].Clicked += Button1_Clicked;
            as1115.KeyScanButtons[KeyScanButtonType.Button1].LongClicked += Button1_LongClicked; ;

            graphics = new MicroGraphics(as1115);

            return base.Initialize(args);
        }

        private void Button1_LongClicked(object sender, EventArgs e)
        {
            Resolver.Log.Info("Button 1 long press");
        }

        private void Button1_Clicked(object sender, EventArgs e)
        {
            Resolver.Log.Info("Button 1 clicked");
        }

        private void KeyScanPressStarted(object sender, KeyScanEventArgs e)
        {
            Resolver.Log.Info($"{e.Button} pressed");
        }

        public override Task Run()
        {
            graphics.Clear();
            graphics.DrawLine(0, 0, 7, 7, true);
            graphics.DrawLine(0, 7, 7, 0, true);

            graphics.Show();

            return base.Run();
        }

        //<!=SNOP=>
    }
}