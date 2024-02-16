﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Motors;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Rotary;
using Meadow.Hardware;
using Meadow.Peripherals;
using System;
using System.Threading.Tasks;

namespace Motors.Tb67h420ftg_Encoder_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private Tb67h420ftg motorDriver;
        private RotaryEncoder encoder;
        private MicroGraphics display;
        private PushButton button1;
        private PushButton button2;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            // this causes interrupts to fail, for some reason:
            //IDigitalInputPort test = Device.CreateDigitalInputPort(Device.Pins.D07);
            // this does not.
            IDigitalOutputPort test = Device.CreateDigitalOutputPort(Device.Pins.D07);

            Resolver.Log.Info("Made it here.");

            button1 = new PushButton(Device.Pins.D13, ResistorMode.InternalPullDown);
            button2 = new PushButton(Device.Pins.D12, ResistorMode.InternalPullDown);

            button1.PressStarted += Button1_PressStarted;
            button1.PressEnded += Button1_PressEnded;
            button2.PressStarted += Button2_PressStarted;
            button2.PressEnded += Button2_PressEnded;

            motorDriver = new Tb67h420ftg(
                inA1: Device.Pins.D04, inA2: Device.Pins.D03, pwmA: Device.Pins.D01,
                inB1: Device.Pins.D05, inB2: Device.Pins.D06, pwmB: Device.Pins.D00,
                fault1: Device.Pins.D02, fault2: Device.Pins.D07,
                hbMode: Device.Pins.D11, tblkab: Device.Pins.D10);

            // 6V motors with a 12V input. this clamps them to 6V
            motorDriver.Motor1.MotorCalibrationMultiplier = 0.5f;
            motorDriver.Motor2.MotorCalibrationMultiplier = 0.5f;

            Resolver.Log.Info("Init encoder");
            encoder = new RotaryEncoder(Device.Pins.D09, Device.Pins.D15);
            //encoder.Rotated += Encoder_Rotated;

            Resolver.Log.Info("Init display");
            var ssd1306 = new Ssd1306(Device.CreateI2cBus(), 60, Ssd1306.DisplayType.OLED128x32);
            display = new MicroGraphics(ssd1306);
            display.CurrentFont = new Font8x12();

            Resolver.Log.Info("Initialization complete.");
            UpdateDisplay("Initialization", "Complete");

            return base.Initialize();
        }

        private int forwardCount = 0;
        private int backwardsCount = 0;
        private void Encoder_Rotated(object _, Meadow.Peripherals.Sensors.Rotary.RotaryChangeResult e)
        {
            if (e.New == RotationDirection.Clockwise)
            {
                forwardCount++;
            }
            else
            {
                backwardsCount++;
            }

            //   display.Clear();
            //   display.DrawText(0, 0, $"{++count} - {e.Direction}");
            //   display.Show();
            //   Resolver.Log.Info($"{++count} - {e.Direction}");
        }

        private void UpdateDisplay(string line1, string line2)
        {
            display.Clear();
            display.DrawText(0, 0, line1);
            display.DrawText(0, 16, line2);
            display.Show();
        }

        private long pressed;
        private int count;
        private void Button1_PressStarted(object sender, EventArgs e)
        {
            count = forwardCount + backwardsCount;
            pressed = DateTime.Now.Ticks;

            Resolver.Log.Info("Motor 1 start.");
            motorDriver.Motor1.Power = 1f;
        }
        private void Button1_PressEnded(object sender, EventArgs e)
        {
            double eventsPerSec = 10000000 * (forwardCount + backwardsCount - count) / (DateTime.Now.Ticks - pressed);

            //  UpdateDisplay($"CW: {forwardCount}, CCW {backwardsCount}", $"Events/s {eventsPerSec}");

            Resolver.Log.Info("Motor 1 stop.");
            motorDriver.Motor1.Power = 0f;
        }

        private void Button2_PressStarted(object sender, EventArgs e)
        {
            Resolver.Log.Info("Motor 2 start.");
            motorDriver.Motor2.Power = 0.5f;
        }
        private void Button2_PressEnded(object sender, EventArgs e)
        {
            Resolver.Log.Info("Motor 2 stop.");
            motorDriver.Motor2.Power = 0f;
        }
    }
}