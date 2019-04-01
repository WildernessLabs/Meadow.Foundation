using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.LCD;

namespace Lcd2004_Sample
{
    class Lcd2004App : AppBase<F7Micro, Lcd2004App>
    {
        Lcd2004 display;

        public Lcd2004App()
        {
            display = new Lcd2004(Device, Device.Pins.D05, Device.Pins.D07,
                Device.Pins.D11, Device.Pins.D12, Device.Pins.D13, Device.Pins.D14,
                16, 2);

            int count = 0;
            display.WriteLine("Lcd2004", 0);

            while (true)
            {
                display.WriteLine($"Count is : {count++}", 1);
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
