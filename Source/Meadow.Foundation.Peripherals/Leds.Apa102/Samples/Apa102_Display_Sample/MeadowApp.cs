﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using System.Threading.Tasks;

namespace Leds.Apa102_Display_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        Apa102 display;

        MicroGraphics canvas;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            display = new Apa102(Device.CreateSpiBus(), 32, 8, Apa102.PixelOrder.BGR);
            canvas = new MicroGraphics(display);

            Resolver.Log.Info("Hardware initialized.");

            return base.Initialize();
        }

        public override async Task Run()
        {
            canvas.CurrentFont = new Font4x8();

            while (true)
            {
                canvas.Clear();
                canvas.DrawText(0, 1, "MEADOW", Colors.AzureBlue.WithBrightness(0.25));
                canvas.DrawText(24, 1, "F7", Colors.ChileanFire.WithBrightness(0.25));
                canvas.Show();

                await Task.Delay(1000);

                canvas.Clear();
                canvas.DrawText(0, 1, "Rocks", Colors.PearGreen.WithBrightness(0.25));
                canvas.Show();

                await Task.Delay(1000);
            }
        }

        static class Colors
        {
            public static Color AzureBlue
            {
                get
                {
                    var azureBlue = Color.FromHex("#23abe3");
                    // make it way less bright
                    return Color.FromHsba(azureBlue.Hue, azureBlue.Saturation, 0.025);
                }
            }
            public static Color ChileanFire
            {
                get
                {
                    var chileanFire = Color.FromHex("#ef7d3b");
                    // make it way less bright
                    return Color.FromHsba(chileanFire.Hue, chileanFire.Saturation, 0.025);
                }
            }
            public static Color PearGreen
            {
                get
                {
                    var PearGreen = Color.FromHex("#c9db31");
                    // make it way less bright
                    return Color.FromHsba(PearGreen.Hue, PearGreen.Saturation, 0.025);
                }
            }
        }
    }
}