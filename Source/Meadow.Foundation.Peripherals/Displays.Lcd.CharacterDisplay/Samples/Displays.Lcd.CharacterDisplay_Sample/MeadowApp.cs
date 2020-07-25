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
          //  InitI2c();

            Console.WriteLine("Test character display");

            I2cCharacterDisplay iDisplay = new I2cCharacterDisplay(Device.CreateI2cBus(I2cBusSpeed.Standard),
                I2cCharacterDisplay.DefaultI2cAddress,
                2, 16);

            //display off
            Console.WriteLine("Display off");
            iDisplay.DisplayOff();
            Thread.Sleep(100);

            //display on
            Console.WriteLine("Display on");
            iDisplay.DisplayOn();
            Thread.Sleep(100);

            Console.WriteLine("Cursor on");
            iDisplay.CursorOn();

            Console.WriteLine("Blick on");
            iDisplay.BlinkOn();

            for(byte i = 0; i < 10; i++)
            {
                iDisplay.SetCursorPosition(i, 0);
                Thread.Sleep(100);
            }

            iDisplay.Write("Hello");

            Thread.Sleep(4000);

            int count = 0;
            while(true)
            {
                iDisplay.SetCursorPosition(0, 0);
                iDisplay.Write($"{count++}");
            }

            //    TestCharacterDisplay();

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

            int count = 0;
            display.Write("CharDisplay");

        /*    while (true)
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
            } */
        }
    }
}