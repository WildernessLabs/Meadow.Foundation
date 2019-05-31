using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.LED;
using System;
using System.Threading;
using static Meadow.Foundation.Displays.LED.SevenSegment;

namespace SevenSegment_Sample
{
    public class SevenSegmentApp : App<F7Micro, SevenSegmentApp>
    {
        SevenSegment sevenSegment;

        public SevenSegmentApp()
        {
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
            bool showDecimal = false;

            while (true)
            {
                foreach (CharacterType character in Enum.GetValues(typeof(CharacterType)))
                {
                    if (character != CharacterType.count)
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