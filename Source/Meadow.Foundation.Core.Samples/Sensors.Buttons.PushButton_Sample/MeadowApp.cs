﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sensors.Buttons.PushButton_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private RgbPwmLed led;
        private List<PushButton> pushButtons;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            led = new RgbPwmLed(
                Device.Pins.OnboardLedRed,
                Device.Pins.OnboardLedGreen,
                Device.Pins.OnboardLedBlue);
            led.SetColor(Color.Red);

            //TestAllResistorTypes();
            TestMultiplePorts();

            Resolver.Log.Info("PushButton(s) ready!!!");

            led.SetColor(Color.Green);

            return Task.CompletedTask;
        }

        private void TestAllResistorTypes()
        {
            pushButtons = new List<PushButton>();

            var inputInternalPullUp = Device.CreateDigitalInterruptPort(
                Device.Pins.MISO,
                InterruptMode.EdgeBoth,
                ResistorMode.InternalPullUp, TimeSpan.FromMilliseconds(20), TimeSpan.Zero);
            var buttonInternalPullUp = new PushButton(inputInternalPullUp);

            pushButtons.Add(buttonInternalPullUp);

            var inputInternalPullDown = Device.CreateDigitalInterruptPort(
                pin: Device.Pins.D02,
                InterruptMode.EdgeBoth,
                resistorMode: ResistorMode.InternalPullDown, TimeSpan.FromMilliseconds(20), TimeSpan.Zero);
            var buttonInternalPullDown = new PushButton(inputInternalPullDown);

            pushButtons.Add(buttonInternalPullDown);

            var inputExternalPullUp = Device.CreateDigitalInterruptPort(
                pin: Device.Pins.D03,
                InterruptMode.EdgeBoth,
                resistorMode: ResistorMode.ExternalPullUp);
            var buttonExternalPullUp = new PushButton(inputExternalPullUp);

            pushButtons.Add(buttonExternalPullUp);

            var inputExternalPullDown = Device.CreateDigitalInterruptPort(
                pin: Device.Pins.D04,
                InterruptMode.EdgeBoth,
                resistorMode: ResistorMode.ExternalPullDown);
            var buttonExternalPullDown = new PushButton(inputExternalPullDown);

            pushButtons.Add(buttonExternalPullDown);

            foreach (var pushButton in pushButtons)
            {
                pushButton.LongClickedThreshold = new TimeSpan(0, 0, 1);

                pushButton.Clicked += PushButtonClicked;
                pushButton.PressStarted += PushButtonPressStarted;
                pushButton.PressEnded += PushButtonPressEnded;
                pushButton.LongClicked += PushButtonLongClicked;
            }

            led.SetColor(Color.Green);
        }

        private void TestMultiplePorts()
        {
            // Important note: You can only use on Push Button per Group Set (GSXX)
            pushButtons = new List<PushButton>
            {
                new PushButton(Device.Pins.A03),         // <- GS00
                //new PushButton(Device.Pins.A05),         // <- GS00

                new PushButton(Device.Pins.A04),         // <- GS01

                new PushButton(Device.Pins.A02),         // <- GS03
 
                //new PushButton(Device.Pins.A00),         // <- GS04
                new PushButton(Device.Pins.D05),         // <- GS04

                new PushButton(Device.Pins.A01),         // <- GS05
                //new PushButton(Device.Pins.COPI),        // <- GS05

                new PushButton(Device.Pins.D08),         // <- GS06
                //new PushButton(Device.Pins.D09),         // <- GS06

                new PushButton(Device.Pins.D07),         // <- GS07
                //new PushButton(Device.Pins.D10),         // <- GS07

                new PushButton(Device.Pins.D03),         // <- GS08

                new PushButton(Device.Pins.D00),         // <- GS09
                //new PushButton(Device.Pins.D04),         // <- GS09
                
                new PushButton(Device.Pins.CIPO),         // <- GS11

                new PushButton(Device.Pins.D01),         // <- GS13
                //new PushButton(Device.Pins.D06),         // <- GS13

                new PushButton(Device.Pins.D12),         // <- GS14

                new PushButton(Device.Pins.D13),         // <- GS15
            };

            foreach (var pushButton in pushButtons)
            {
                pushButton.Clicked += PushButtonClicked;
            }
        }

        private void PushButtonClicked(object sender, EventArgs e)
        {
            Resolver.Log.Info($"PushButton Clicked!");
            led.SetColor(Color.Orange);
            Thread.Sleep(500);
            led.SetColor(Color.Green);
        }

        private void PushButtonPressStarted(object sender, EventArgs e)
        {
            Resolver.Log.Info($"PushButton PressStarted!");
            led.SetColor(Color.Red);
        }

        private void PushButtonPressEnded(object sender, EventArgs e)
        {
            Resolver.Log.Info($"PushButton PressEnded!");
            led.SetColor(Color.Green);
        }

        private void PushButtonLongClicked(object sender, EventArgs e)
        {
            Resolver.Log.Info($"PushButton LongClicked!");
            led.SetColor(Color.Blue);
            Thread.Sleep(500);
            led.SetColor(Color.Green);
        }

        //<!=SNOP=>
    }
}