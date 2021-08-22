using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Led;

namespace Displays.Led.SevenSegment_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        #region DocsSnippet

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            var sevenSegment = new SevenSegment
            (
                portA: Device.CreateDigitalOutputPort(Device.Pins.D14),
                portB: Device.CreateDigitalOutputPort(Device.Pins.D15),
                portC: Device.CreateDigitalOutputPort(Device.Pins.D06),
                portD: Device.CreateDigitalOutputPort(Device.Pins.D07),
                portE: Device.CreateDigitalOutputPort(Device.Pins.D08),
                portF: Device.CreateDigitalOutputPort(Device.Pins.D13),
                portG: Device.CreateDigitalOutputPort(Device.Pins.D12),
                portDecimal: Device.CreateDigitalOutputPort(Device.Pins.D05),
                isCommonCathode: false
            );

            sevenSegment.SetDisplay(character: '1', showDecimal: true);
        }

        #endregion

        void TestSevenSegment(SevenSegment sevenSegment)
        {
            Console.WriteLine("TestSevenSegment...");

            bool showDecimal = false;

            while (true)
            {
                foreach (SevenSegment.CharacterType character in Enum.GetValues(typeof(SevenSegment.CharacterType)))
                {
                    if (character != SevenSegment.CharacterType.count)
                    {
                        Console.WriteLine("Character: {0}", character.ToString());
                        sevenSegment.SetDisplay(character, showDecimal);
                    }

                    Thread.Sleep(1000);
                }

                showDecimal = !showDecimal;
            }
        }
    }
}