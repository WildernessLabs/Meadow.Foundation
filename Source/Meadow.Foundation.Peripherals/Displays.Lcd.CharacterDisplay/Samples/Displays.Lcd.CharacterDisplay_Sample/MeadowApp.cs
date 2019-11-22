using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Lcd;

namespace Displays.Lcd.CharacterDisplay_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        CharacterDisplay display;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            display = new CharacterDisplay
            (
                device: Device,
                pinRS: Device.Pins.D15,
                pinE: Device.Pins.D14,
                pinD4: Device.Pins.D13,
                pinD5: Device.Pins.D12,
                pinD6: Device.Pins.D11,
                pinD7: Device.Pins.D10
            );

            TestCharacterDisplay();
        }

        void TestCharacterDisplay() 
        {
            Console.WriteLine("TestCharacterDisplay...");

            int count = 0;
            display.WriteLine("CharacterDisplay", 0);

            while (true)
            {
                display.WriteLine($"Count is : {count++}", 1);
                Thread.Sleep(1000);
            }
        }
    }
}