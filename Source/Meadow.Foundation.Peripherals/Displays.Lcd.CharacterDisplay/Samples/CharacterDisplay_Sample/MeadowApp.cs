using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Lcd;
using Meadow.Hardware;
using System.Threading;
using System.Threading.Tasks;

namespace Displays.Lcd.CharacterDisplay_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        CharacterDisplay display;

        public override Task Initialize()
        {
            //InitGpio();
            //InitGpioWithPWM();
            //InitI2c();
            InitGrove();

            return base.Initialize();
        }

        void InitGpio()
        {
            Resolver.Log.Info("InitGpio...");

            display = new CharacterDisplay
            (
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
            Resolver.Log.Info("InitGpioWithPWM...");

            display = new CharacterDisplay
            (
                pinV0: Device.Pins.D11,
                pinRS: Device.Pins.D10,
                pinE: Device.Pins.D09,
                pinD4: Device.Pins.D08,
                pinD5: Device.Pins.D07,
                pinD6: Device.Pins.D06,
                pinD7: Device.Pins.D05,
                rows: 4, columns: 20
            );
        }

        void InitI2c()
        {
            Resolver.Log.Info("InitI2c...");

            display = new CharacterDisplay
            (
                i2cBus: Device.CreateI2cBus(I2cBusSpeed.Standard),
                address: (byte)I2cCharacterDisplay.Address.Default,
                rows: 4, columns: 20
            );
        }

        void InitGrove()
        {
            Resolver.Log.Info("InitGrove...");

            display = new CharacterDisplay
            (
                i2cBus: Device.CreateI2cBus(I2cBusSpeed.Standard),
                address: (byte)I2cCharacterDisplay.Address.Grove,
                rows: 2, columns: 16,
                isGroveDisplay: true
            );
        }

        void TestCharacterDisplay()
        {
            Resolver.Log.Info("TestCharacterDisplay...");

            display.WriteLine("Hello", 0);

            display.WriteLine("Display", 1);

            Thread.Sleep(1000);
            display.WriteLine("Will delete in", 0);

            int count = 5;
            while (count > 0)
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

        public override Task Run()
        {
            TestCharacterDisplay();

            Resolver.Log.Info("Test complete");

            return base.Run();
        }

        //<!=SNOP=>
    }
}