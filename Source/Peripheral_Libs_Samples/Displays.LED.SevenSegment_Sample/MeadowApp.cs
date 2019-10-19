using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.LED;

namespace Displays.LED.SevenSegment_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        SevenSegment sevenSegment;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            sevenSegment = new SevenSegment
            (
                portA: Device.CreateDigitalOutputPort(Device.Pins.D01),
                portB: Device.CreateDigitalOutputPort(Device.Pins.D00),
                portC: Device.CreateDigitalOutputPort(Device.Pins.D08),
                portD: Device.CreateDigitalOutputPort(Device.Pins.D07),
                portE: Device.CreateDigitalOutputPort(Device.Pins.D06),
                portF: Device.CreateDigitalOutputPort(Device.Pins.D11),
                portG: Device.CreateDigitalOutputPort(Device.Pins.D09),
                portDecimal: Device.CreateDigitalOutputPort(Device.Pins.D10),
                isCommonCathode: false
            );

            TestSevenSegment();
        }

        protected void TestSevenSegment()
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