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
            //InitGpioWithPWM();
            InitI2c();

            TestCharacterDisplay();

            Console.WriteLine("Test complete");
        }

        void InitGpio() 
        {
            Console.WriteLine("InitGpio...");

            //display = new CharacterDisplay
            //(
            //    portRS: Device.CreateDigitalOutputPort(Device.Pins.D10),
            //    portE: Device.CreateDigitalOutputPort(Device.Pins.D09),
            //    portD4: Device.CreateDigitalOutputPort(Device.Pins.D08),
            //    portD5: Device.CreateDigitalOutputPort(Device.Pins.D07),
            //    portD6: Device.CreateDigitalOutputPort(Device.Pins.D06),
            //    portD7: Device.CreateDigitalOutputPort(Device.Pins.D05),
            //    rows: 4, columns: 20
            //);

            display = new CharacterDisplay
            (
                device: Device,
                pinRS: Device.Pins.D10,
                pinE: Device.Pins.D09,
                pinD4: Device.Pins.D08,
                pinD5: Device.Pins.D07,
                pinD6: Device.Pins.D06,
                pinD7: Device.Pins.D05,
                rows: 4, columns: 20
            );
        }

        void InitGpioWithPWM()
        {
            Console.WriteLine("InitGpioWithPWM...");

            //display = new CharacterDisplay
            //(
            //    portV0: Device.CreatePwmPort(Device.Pins.D11, 100, 0.5f, true),
            //    portRS: Device.CreateDigitalOutputPort(Device.Pins.D10),
            //    portE:  Device.CreateDigitalOutputPort(Device.Pins.D09),
            //    portD4: Device.CreateDigitalOutputPort(Device.Pins.D08),
            //    portD5: Device.CreateDigitalOutputPort(Device.Pins.D07),
            //    portD6: Device.CreateDigitalOutputPort(Device.Pins.D06),
            //    portD7: Device.CreateDigitalOutputPort(Device.Pins.D05),
            //    rows: 4, columns: 20    // Adjust dimensions to fit your display
            //);

            display = new CharacterDisplay
            (
                device: Device,
                pinV0: Device.Pins.D11,
                pinRS: Device.Pins.D10,
                pinE:  Device.Pins.D09,
                pinD4: Device.Pins.D08,
                pinD5: Device.Pins.D07,
                pinD6: Device.Pins.D06,
                pinD7: Device.Pins.D05,
                rows: 4, columns: 20
            );
        }

        void InitI2c()
        {
            Console.WriteLine("InitI2c...");

            display = new CharacterDisplay
            (
                i2cBus: Device.CreateI2cBus(I2cBusSpeed.Standard),
                address: I2cCharacterDisplay.DefaultI2cAddress,
                rows: 4, columns: 20
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