using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        //<!—SNIP—>

        Max7219 display;

        public MeadowApp()
        {
            Console.WriteLine("Initialize...");

            display = new Max7219(Device, Device.CreateSpiBus(), Device.Pins.D01, 1, Max7219.Max7219Type.Character);

            while (true)
            {
                TestDigitalMode();
                TestCharacterMode();
            }
        }

        void TestCharacterMode()
        {
            display.SetMode(Max7219.Max7219Type.Character);
            //show every supported character 
            for (int i = 0; i < (int)Max7219.CharacterType.count; i++)
            {
                for (int digit = 0; digit < 8; digit++)
                {
                    display.SetCharacter((Max7219.CharacterType)i, digit, i % 2 == 0);
                }
                display.Show();
            }
        }

        void TestDigitalMode()
        {
            Console.WriteLine("Digital test");

            display.SetMode(Max7219.Max7219Type.Digital);
            //control indivial LEDs - for 8x8 matrix configurations - use the Meadow graphics library
            for (byte i = 0; i < 64; i++)
            {
                for (int d = 0; d < 8; d++)
                {
                    display.SetDigit(i, d);
                }
                display.Show();
            }
        }

        //<!—SNOP—>

        void CharacterDemo()
        {
            display.SetMode(Max7219.Max7219Type.Character);
            display.Clear();

            for (int i = 0; i < 8; i++)
            {
                display.SetCharacter(Max7219.CharacterType.Blank, i);
            }

            for (int i = 980; i < 999; i++)
            {
                display.SetNumber(i);
                display.Show();
            }

            display.SetCharacter(Max7219.CharacterType.Hyphen, 0);
            display.SetCharacter(Max7219.CharacterType.Hyphen, 1);
            display.SetCharacter(Max7219.CharacterType.P, 2);
            display.SetCharacter(Max7219.CharacterType.L, 3);
            display.SetCharacter(Max7219.CharacterType.E, 4);
            display.SetCharacter(Max7219.CharacterType.H, 5);
            display.SetCharacter(Max7219.CharacterType.Hyphen, 6);
            display.SetCharacter(Max7219.CharacterType.Hyphen, 7);
            display.Show();

            Thread.Sleep(1000);
        }

        
    }
}