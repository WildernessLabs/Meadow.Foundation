﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System.Threading.Tasks;

namespace Displays.ePaper.IL3897_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        MicroGraphics graphics;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize ...");

            var display = new Il3897(
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D01,
                resetPin: Device.Pins.D00,
                busyPin: Device.Pins.D03,
                width: 122,
                height: 250);

            graphics = new MicroGraphics(display);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            graphics.DrawRectangle(1, 1, 126, 32, Color.Red, false);

            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(2, 2, "IL0373", Color.Black);
            graphics.DrawText(2, 20, "Meadow F7", Color.Black);

            graphics.Show();

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}