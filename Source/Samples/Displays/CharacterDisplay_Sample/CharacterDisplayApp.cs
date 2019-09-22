using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Lcd;

namespace CharacterDisplay_Sample
{
    class CharacterDisplayApp : App<F7Micro, CharacterDisplayApp>
    {
        CharacterDisplay display;

        public CharacterDisplayApp()
        {
            display = new CharacterDisplay
            (
                device: Device, 
                pinRS: Device.Pins.D15,
                pinE: Device.Pins.D14,
                pinD4: Device.Pins.D13, 
                pinD5: Device.Pins.D12, 
                pinD6: Device.Pins.D11, 
                pinD7: Device.Pins.D10
            );

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