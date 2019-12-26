using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Max7219 display;

        public MeadowApp()
        {
            Init();

            while(true)
            {
                TestDigitalMode();
                TestCharacterMode();
                CharacterDemo();
            }
        }

        void CharacterDemo()
        {
            display.SetMode(true);
            display.ClearAll();

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

        void TestCharacterMode()
        {
            display.SetMode(true);

            for (int i = 0; i < (int)Max7219.CharacterType.count; i++)
            {
                for (int digit = 0; digit < 8; digit++)
                {
                    display.SetCharacter((Max7219.CharacterType)i, digit, i%2 == 0);
                }
                display.Show();
                Console.WriteLine(((Max7219.CharacterType)i).ToString());
            }
        }
        
        void TestDigitalMode()
        {
            display.SetMode(false);

            for (byte i = 0; i < 64; i++)
            {
                for(int d = 0; d < 8; d++)
                {
                    display.SetDigit(i, d);
                }
                display.Show();
            }
        }

        public void Init()
        {
            Console.WriteLine("Init...");

            display = new Max7219(Device, Device.CreateSpiBus(), Device.Pins.D02, 1, true);
        }
    }
}