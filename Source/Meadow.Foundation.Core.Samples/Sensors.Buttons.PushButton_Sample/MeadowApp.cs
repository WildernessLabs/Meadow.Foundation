using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Sensors.Buttons.PushButton_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        RgbPwmLed led;
        PushButton pushButton;
        List<PushButton> pushButtons;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            TestSinglePort();
            //TestMultiplePorts();

            Console.WriteLine("PushButton(s) ready!!!");
        }

        void TestSinglePort()
        {
            led = new RgbPwmLed(
                Device,
                Device.Pins.D12,
                Device.Pins.D11,
                Device.Pins.D10);
            led.SetColor(Color.Red);

            //var interruptPort = Device.CreateDigitalInputPort(
            //    pin: Device.Pins.MOSI,
            //    interruptMode: InterruptMode.EdgeRising,
            //    resistorMode: ResistorMode.PullUp, 20);
            //pushButton = new PushButton(interruptPort);
            //pushButton = new PushButton(
            //    device: Device,
            //    inputPin: Device.Pins.MOSI,
            //    interruptMode: InterruptMode.EdgeRising,
            //    resistor: ResistorMode.PullUp);

            //var interruptPort = Device.CreateDigitalInputPort(
            //    pin: Device.Pins.D02,
            //    interruptMode: InterruptMode.EdgeFalling,
            //    resistorMode: ResistorMode.PullDown, 20);
            //pushButton = new PushButton(interruptPort);
            //pushButton = new PushButton(
            //    device: Device,
            //    inputPin: Device.Pins.D02,
            //    interruptMode: InterruptMode.EdgeFalling,
            //    resistor: ResistorMode.PullDown);

            var interruptPort = Device.CreateDigitalInputPort(
                pin: Device.Pins.D03,
                interruptMode: InterruptMode.EdgeBoth,
                resistorMode: ResistorMode.Disabled);
            pushButton = new PushButton(interruptPort);
            //pushButton = new PushButton(
            //    device: Device,
            //    inputPin: Device.Pins.D03,
            //    interruptMode: InterruptMode.EdgeBoth,
            //    resistor: ResistorMode.Disabled);

            //var interruptPort = Device.CreateDigitalInputPort(
            //    pin: Device.Pins.D04,
            //    interruptMode: InterruptMode.EdgeRising, 
            //    resistorMode: ResistorMode.Disabled);
            //pushButton = new PushButton(interruptPort);
            //pushButton = new PushButton(
            //    device: Device,
            //    inputPin: Device.Pins.D04,
            //    resistor: ResistorMode.Disabled);

            if (pushButton.DigitalIn.InterruptMode == InterruptMode.EdgeRising ||
                pushButton.DigitalIn.InterruptMode == InterruptMode.EdgeFalling)
            {
                pushButton.Clicked += PushButtonClicked;
            }

            if (pushButton.DigitalIn.InterruptMode == InterruptMode.EdgeBoth)
            {
                pushButton.PressStarted += PushButtonPressStarted;
                pushButton.PressEnded += PushButtonPressEnded;
                pushButton.LongPressClicked += PushButtonLongPressClicked;
            }

            led.SetColor(Color.Green);
        }

        void TestMultiplePorts() 
        {
            led = new RgbPwmLed(
                Device, 
                Device.Pins.OnboardLedRed, 
                Device.Pins.OnboardLedGreen, 
                Device.Pins.OnboardLedBlue);
            led.SetColor(Color.Red);

            // Important note: You can only use on Push Button per Group Set (GSXX)
            pushButtons = new List<PushButton>
            {
                //new PushButton(Device, Device.Pins.A04, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS00
                new PushButton(Device, Device.Pins.D06, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS00

                //new PushButton(Device, Device.Pins.A05, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS01
                new PushButton(Device, Device.Pins.D09, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS01

                //new PushButton(Device, Device.Pins.A02, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS03
                new PushButton(Device, Device.Pins.D14, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS03
                //new PushButton(Device, Device.Pins.D15, InterruptMode.EdgeRising, ResistorMode.PullUp)          // <- GS03

                new PushButton(Device, Device.Pins.A00, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS04

                //new PushButton(Device, Device.Pins.A01, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS05
                new PushButton(Device, Device.Pins.MOSI,InterruptMode.EdgeRising, ResistorMode.PullUp),        // <- GS05

                new PushButton(Device, Device.Pins.D02, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS06
                //new PushButton(Device, Device.Pins.D08, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS06

                //new PushButton(Device, Device.Pins.A03, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS07
                new PushButton(Device, Device.Pins.D05, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS07
                //new PushButton(Device, Device.Pins.D07, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS07

                new PushButton(Device, Device.Pins.D03, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS08

                new PushButton(Device, Device.Pins.D00, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS09
                //new PushButton(Device, Device.Pins.D04, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS09
                //new PushButton(Device, Device.Pins.D11, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS09               
                
                new PushButton(Device, Device.Pins.MISO, InterruptMode.EdgeRising, ResistorMode.PullUp),        // <- GS11
                
                new PushButton(Device, Device.Pins.D12, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS14

                new PushButton(Device, Device.Pins.D13, InterruptMode.EdgeRising, ResistorMode.PullUp),         // <- GS15
            };

            for (int i = 0; i < pushButtons.Count; i++)
            {
                if (pushButtons[i].DigitalIn.InterruptMode == InterruptMode.EdgeRising ||
                    pushButtons[i].DigitalIn.InterruptMode == InterruptMode.EdgeFalling)
                {
                    pushButtons[i].Clicked += PushButtonClicked;
                }

                if (pushButtons[i].DigitalIn.InterruptMode == InterruptMode.EdgeBoth)
                {
                    pushButtons[i].PressStarted += PushButtonPressStarted;
                    pushButtons[i].PressEnded += PushButtonPressEnded;
                    pushButtons[i].LongPressClicked += PushButtonLongPressClicked;
                }
            }

            led.SetColor(Color.Green);
        }

        void PushButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine($"PushButton Clicked!");
            led.SetColor(Color.Magenta);
            Thread.Sleep(100);
            led.SetColor(Color.Green);
        }

        void PushButtonPressStarted(object sender, EventArgs e)
        {
            Console.WriteLine($"PushButton PressStarted!");
            led.SetColor(Color.Red);
        }

        void PushButtonPressEnded(object sender, EventArgs e)
        {
            Console.WriteLine($"PushButton PressEnded!");
            led.SetColor(Color.Green);
        }

        void PushButtonLongPressClicked(object sender, EventArgs e)
        {
            Console.WriteLine($"PushButton Clicked!");
            led.SetColor(Color.Blue);
            Thread.Sleep(100);
            led.SetColor(Color.Green);
        }
    }
}