using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;

namespace FeatherWings.OLED128x32_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        II2cBus _i2cBus;
        OLED128x32Wing oledWing;

        public MeadowApp()
        {
            Initialize();
            Run();
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            _i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);

            oledWing = new OLED128x32Wing(_i2cBus, Device, Device.Pins.D11, Device.Pins.D10, Device.Pins.D09);

            oledWing.OnA += (sender, e) =>
            {
                Console.WriteLine("A");
                oledWing.WriteLines("Button A");
            };

            oledWing.OnB += (sender, e) => 
            {
                Console.WriteLine("B");
                oledWing.WriteLines("", "Button B");
            };

            oledWing.OnC += (sender, e) => {
                Console.WriteLine("C");
                oledWing.WriteLines("", "", "Button C");
            };
        }

        void Run()
        {
            oledWing.WriteLines("     Hello", "----------------", "    Meadow!", "----------------");

            while (true) { }
        }

    }
}
