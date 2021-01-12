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
        List<PushButton> pushButtons;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            //TestSinglePort();
            TestMultiplePorts();

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

            pushButtons = new List<PushButton>();
            
            pushButtons.Add(new PushButton(Device.CreateDigitalInputPort(
                Device.Pins.MOSI, InterruptMode.EdgeBoth, ResistorMode.PullDown, 25, 50), ResistorType.InternalPullUp));
            //pushButtons.Add(new PushButton(Device, Device.Pins.MOSI, ResistorType.InternalPullUp));

            pushButtons.Add(new PushButton(Device.CreateDigitalInputPort(
                Device.Pins.D02, InterruptMode.EdgeBoth, ResistorMode.PullUp, 25, 50), ResistorType.InternallPullDown));
            //pushButtons.Add(new PushButton(Device, Device.Pins.D02, ResistorType.InternallPullDown));

            pushButtons.Add(new PushButton(Device.CreateDigitalInputPort(
                Device.Pins.D03, InterruptMode.EdgeBoth, ResistorMode.Disabled, 25, 50), ResistorType.ExternalPullUp));
            //pushButtons.Add(new PushButton(Device, Device.Pins.D03, ResistorType.ExternalPullUp));

            pushButtons.Add(new PushButton(Device.CreateDigitalInputPort(
                Device.Pins.D04, InterruptMode.EdgeBoth, ResistorMode.Disabled, 25, 50), ResistorType.ExternallPulldown));
            //pushButtons.Add(new PushButton(Device, Device.Pins.D04, ResistorType.ExternallPulldown));            

            foreach (var pushButton in pushButtons)
            {
                pushButton.Clicked += PushButtonClicked;
                //pushButton.PressStarted += PushButtonPressStarted;
                //pushButton.PressEnded += PushButtonPressEnded;
                //pushButton.LongPressClicked += PushButtonLongPressClicked;
            }

            led.SetColor(Color.Green);
            Console.WriteLine("===================================================================================================");
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
                //new PushButton(Device, Device.Pins.A04, ResistorType.InternalPullUp),         // <- GS00
                new PushButton(Device, Device.Pins.D06, ResistorType.InternalPullUp),         // <- GS00

                //new PushButton(Device, Device.Pins.A05, ResistorType.InternalPullUp),         // <- GS01
                new PushButton(Device, Device.Pins.D09, ResistorType.InternalPullUp),         // <- GS01

                //new PushButton(Device, Device.Pins.A02, ResistorType.InternalPullUp),         // <- GS03
                new PushButton(Device, Device.Pins.D14, ResistorType.InternalPullUp),         // <- GS03
                //new PushButton(Device, Device.Pins.D15, ResistorType.InternalPullUp)          // <- GS03

                new PushButton(Device, Device.Pins.A00, ResistorType.InternalPullUp),         // <- GS04

                //new PushButton(Device, Device.Pins.A01, ResistorType.InternalPullUp),         // <- GS05
                new PushButton(Device, Device.Pins.MOSI,ResistorMode.PullUp),        // <- GS05

                new PushButton(Device, Device.Pins.D02, ResistorType.InternalPullUp),         // <- GS06
                //new PushButton(Device, Device.Pins.D08, ResistorType.InternalPullUp),         // <- GS06

                //new PushButton(Device, Device.Pins.A03, ResistorType.InternalPullUp),         // <- GS07
                new PushButton(Device, Device.Pins.D05, ResistorType.InternalPullUp),         // <- GS07
                //new PushButton(Device, Device.Pins.D07, ResistorType.InternalPullUp),         // <- GS07

                new PushButton(Device, Device.Pins.D03, ResistorType.InternalPullUp),         // <- GS08

                new PushButton(Device, Device.Pins.D00, ResistorType.InternalPullUp),         // <- GS09
                //new PushButton(Device, Device.Pins.D04, ResistorType.InternalPullUp),         // <- GS09
                //new PushButton(Device, Device.Pins.D11, ResistorType.InternalPullUp),         // <- GS09               
                
                new PushButton(Device, Device.Pins.MISO, ResistorType.InternalPullUp),        // <- GS11
                
                new PushButton(Device, Device.Pins.D12, ResistorType.InternalPullUp),         // <- GS14

                new PushButton(Device, Device.Pins.D13, ResistorType.InternalPullUp),         // <- GS15
            };

            foreach (var pushButton in pushButtons)
            {
                pushButton.Clicked += PushButtonClicked;
                //pushButton.PressStarted += PushButtonPressStarted;
                //pushButton.PressEnded += PushButtonPressEnded;
                //pushButton.LongPressClicked += PushButtonLongPressClicked;
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