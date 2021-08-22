using System;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.FeatherWings;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace FeatherWings.OLED128x32_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //<!—SNIP—>

        GraphicsLibrary graphics;

        public MeadowApp()
        {
            Console.WriteLine("Initializing ...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);

            var oledWing = new OLED128x32Wing(i2cBus, Device, Device.Pins.D11, Device.Pins.D10, Device.Pins.D09);

            graphics = new GraphicsLibrary(oledWing.Display);
            graphics.CurrentFont = new Font12x16();

            oledWing.ButtonA.Clicked += (sender, e) => UpdateDisplay("A pressed");
            oledWing.ButtonB.Clicked += (sender, e) => UpdateDisplay("B pressed");
            oledWing.ButtonC.Clicked += (sender, e) => UpdateDisplay("C pressed");
        }

        void UpdateDisplay(string message)
        {
            graphics.Clear();
            graphics.DrawText(0, 8, message);
            graphics.Show();
        }

        //<!—SNOP—>
    }
}