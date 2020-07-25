using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Lcd;
using System;
using System.Threading;

namespace Displays.Lcd.CharacterDisplay_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        GpioCharacterDisplay display;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            //display = new CharacterDisplay
            //(
            //    device: Device,
            //    pinV0: Device.Pins.D11,
            //    pinRS: Device.Pins.D10,
            //    pinE: Device.Pins.D09,
            //    pinD4: Device.Pins.D08,
            //    pinD5: Device.Pins.D07,
            //    pinD6: Device.Pins.D06,
            //    pinD7: Device.Pins.D05,
            //    rows: 4, columns: 20    // Adjust dimensions to fit your display
            //);

            display = new GpioCharacterDisplay
            (
                Device.CreatePwmPort(Device.Pins.D11, 100, 0.5f, true),
                Device.CreateDigitalOutputPort(Device.Pins.D10),
                Device.CreateDigitalOutputPort(Device.Pins.D09),
                Device.CreateDigitalOutputPort(Device.Pins.D08),
                Device.CreateDigitalOutputPort(Device.Pins.D07),
                Device.CreateDigitalOutputPort(Device.Pins.D06),
                Device.CreateDigitalOutputPort(Device.Pins.D05),
                rows: 4, columns: 20    // Adjust dimensions to fit your display
            );

            TestCharacterDisplay();

            Console.WriteLine("done");
        }

        void TestCharacterDisplay() 
        {
            Console.WriteLine("TestCharacterDisplay...");

            int count = 0;
            display.WriteLine("CharacterDisplay", 0);

            while (true)
            {
                // Increasing Contrast
                for (int i = 0; i <= 10; i++)
                {
                    display.SetContrast((float)(i / 10f));
                    Thread.Sleep(250);
                }

                // Decreasing Contrast
                for (int i = 10; i >= 0; i--)
                {
                    display.SetContrast((float)(i / 10f));
                    Thread.Sleep(250);
                }

                display.WriteLine($"Count is : {count++}", 1);
                Thread.Sleep(1000);
            }
        }
    }
}