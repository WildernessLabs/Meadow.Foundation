﻿using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;

namespace Displays.ST7565_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        St7565 sT7565;
        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            sT7565 = new St7565
            (
                device: Device, 
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D02,
                dcPin: Device.Pins.D00,
                resetPin: Device.Pins.D01,
                width: 128, 
                height: 64
            );

            graphics = new GraphicsLibrary(sT7565);

            TestST7565();
        }

        void TestST7565()
        {
            Console.WriteLine("TestST7565...");

            // Drawing with Display Graphics Library
            graphics.CurrentFont = new Font8x8();
            graphics.Clear();
            graphics.DrawTriangle(10, 10, 50, 50, 10, 50, Meadow.Foundation.Color.Red);
            graphics.DrawRectangle(20, 15, 40, 20, Meadow.Foundation.Color.Yellow, true);            
            graphics.DrawText(5, 5, "ST7565");
            graphics.Show();
        }
    }
}