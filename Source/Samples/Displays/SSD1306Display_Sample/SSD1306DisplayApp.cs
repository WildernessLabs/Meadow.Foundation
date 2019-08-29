using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;

namespace SSD1306Display_Sample
{
    public class SSD1306DisplayApp : App<F7Micro, SSD1306DisplayApp>
    {
        SSD1306 display;

        public SSD1306DisplayApp()
        {
            display = new SSD1306(Device, Device.Pins.D08, Device.Pins.D07, 60, 400, SSD1306.DisplayType.OLED128x32);

            display.Clear(true);
            
            for(int i = 0; i < 30; i++)
            {
                display.DrawPixel(i, i, true);
                display.DrawPixel(30 + i, i, true);
                display.DrawPixel(60 + i, i, true);
            }

            display.Show();
        }
    }
}
