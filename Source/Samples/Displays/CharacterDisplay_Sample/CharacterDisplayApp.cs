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
            display = new CharacterDisplay
            (
                device: Device, 
                pinRS: Device.Pins.D05,
                pinE: Device.Pins.D07,
                pinD4: Device.Pins.D08, 
                pinD5: Device.Pins.D09, 
                pinD6: Device.Pins.D10, 
                pinD7: Device.Pins.D11                
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