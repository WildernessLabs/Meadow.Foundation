using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays.Led;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Displays.Led.SevenSegment_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        SevenSegment sevenSegment;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing...");

            sevenSegment = new SevenSegment
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

            return base.Initialize();
        }

        public override Task Run()
        {
            sevenSegment.SetDisplay(character: '1', showDecimal: true);

            return base.Run();
        }

        //<!=SNOP=>

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