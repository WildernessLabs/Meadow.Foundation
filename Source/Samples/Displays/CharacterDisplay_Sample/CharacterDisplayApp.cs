using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Lcd;

namespace CharacterDisplay_Sample
{
    class CharacterDisplayApp : AppBase<F7Micro, CharacterDisplayApp>
    {
        CharacterDisplay display;

        public CharacterDisplayApp()
        {
            display = new CharacterDisplay(Device, Device.Pins.D05, Device.Pins.D07,
                Device.Pins.D11, Device.Pins.D12, Device.Pins.D13, Device.Pins.D14,
                16, 2);

            int count = 0;
            display.WriteLine("CharacterDisplay", 0);

            while (true)
            {
                display.WriteLine($"Count is : {count++}", 1);
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
