using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Lcd;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Displays.Lcd.CharacterDisplay_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        CharacterDisplay display;

        public MeadowApp()
        {
            //InitGpio();
            InitI2c();

            TestCharacterDisplay();

            Console.WriteLine("Test complete");
        }

        void InitI2c()
        {
            Console.WriteLine("Initializing I2C...");

            display = new CharacterDisplay(Device.CreateI2cBus(I2cBusSpeed.Standard), 
                I2cCharacterDisplay.DefaultI2cAddress,
                2, 16);
        }

        void InitGpio()
        { 
            Console.WriteLine("Initializing GPIO...");

            display = new CharacterDisplay
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
        }

        void TestCharacterDisplay() 
        {
            Console.WriteLine("TestCharacterDisplay...");

            display.WriteLine("Hello", 0);

            display.WriteLine("Display", 1);

            Thread.Sleep(1000);
            display.WriteLine("Will delete in", 0);

            int count = 5;
            while(count > 0)
            {
                display.WriteLine($"{count--}", 1);
                Thread.Sleep(500);
            }

            display.ClearLines();
            Thread.Sleep(2000);

            display.WriteLine("Cursor test", 0);

            for (int i = 0; i < display.DisplayConfig.Width; i++)
            {
                display.SetCursorPosition((byte)i, 1);
                display.Write("*");
                Thread.Sleep(100);
                display.SetCursorPosition((byte)i, 1);
                display.Write(" ");
            }

            display.ClearLines();
            display.WriteLine("Complete!", 0);
        }
    }
}