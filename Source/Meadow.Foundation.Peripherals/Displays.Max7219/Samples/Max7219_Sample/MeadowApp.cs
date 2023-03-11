using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using System.Threading;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Max7219 display;

        public override Task Initialize(string[]? args)
        {
            Resolver.Log.Info("Initialize...");

            display = new Max7219(Device.CreateSpiBus(), Device.Pins.D01, 1, Max7219.Max7219Mode.Character);

            return base.Initialize(args);
        }

        void TestCharacterMode()
        {
            display.SetMode(Max7219.Max7219Mode.Character);
            //show every supported character 
            for (int i = 0; i < (int)Max7219.CharacterType.Count; i++)
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
            Resolver.Log.Info("Digital test");

            display.SetMode(Max7219.Max7219Mode.Digital);
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

        public override Task Run()
        {
            while (true)
            {
                TestDigitalMode();
                TestCharacterMode();
            }
        }

        //<!=SNOP=>

        void CharacterDemo()
        {
            display.SetMode(Max7219.Max7219Mode.Character);
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