using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace BasicPCD8843Display_Sample
{
    public class PCD8854DisplayApp : App<F7Micro, PCD8854DisplayApp>
    {

        Meadow.Foundation.Displays.PCD8544 display;
        Meadow.Foundation.Graphics.GraphicsLibrary graphics;

        public PCD8854DisplayApp()
        {
            var spiBus = Device.CreateSpiBus();

            display = new Meadow.Foundation.Displays.PCD8544(Device, spiBus,
                Device.Pins.D01, Device.Pins.D00, Device.Pins.D02);

            for (int i = 0; i < 35; i++)
            {
                display.DrawPixel(i, i, true);
            }

            graphics = new Meadow.Foundation.Graphics.GraphicsLibrary(display);

            graphics.Clear();
            graphics.CurrentFont = new Font8x12();
            graphics.DrawText(0, 0, "Meadow F7");
            graphics.DrawRectangle(5, 14, 30, 10, true);

            /*
            graphics = new Meadow.Foundation.Graphics.GraphicsLibrary(display);

            graphics.Clear(true);

            graphics.DrawRectangle(2, 2, 20, 20, false);

            graphics.DrawCircle(20, 30, 12, false);

    */


        }
    }
}
