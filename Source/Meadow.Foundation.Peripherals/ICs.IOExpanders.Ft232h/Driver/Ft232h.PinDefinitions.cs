using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ft232h
    {
        public class PinDefinitions : IPinDefinitions
        {
            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Collection of pins
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            public PinDefinitions()
            {
                InitAllPins();
            }

            // Aliases
            public IPin SPI_SCK => D0;
            public IPin SPI_COPI => D1;
            public IPin SPI_CIPO => D2;
            public IPin SPI_CS0 => D3;

            public IPin I2C_SCL => D0;
            public IPin I2C_SDA => D1;

            public readonly IPin D0 = new Pin(
                "D0",
                (byte)0x10,
                new List<IChannelInfo> {
                    new SpiChannelInfo("SPI_SCK", SpiLineType.Clock),
                    new I2cChannelInfo("I2C_SCL", I2cChannelFunctionType.Clock)
                });

            public readonly IPin D1 = new Pin(
                "D1",
                (byte)0x11,
                new List<IChannelInfo> {
                    new SpiChannelInfo("SPI_COPI", SpiLineType.MOSI),
                    new I2cChannelInfo("I2C_SDA", I2cChannelFunctionType.Data)
                });

            public readonly IPin D2 = new Pin(
                "D2",
                (byte)0x12,
                new List<IChannelInfo> {
                    new SpiChannelInfo("SPI_CIPO", SpiLineType.MISO)
                });

            public readonly IPin D3 = new Pin(
                "D3",
                (byte)0x12,
                new List<IChannelInfo> {
                    new SpiChannelInfo("SPI_CS0", SpiLineType.ChipSelect)
                });

            // TODO: D4-D7 can be used as CS, and (probably??) GPIO. The docs are not terribly clear on this.  Maybe just outputs and direct write the CS?

            public readonly IPin SPI_COPI_D1 = new Pin(
                "SPI_COPI_D1",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("D1", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public readonly IPin C0 = new Pin(
                "C0",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C0", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public readonly IPin C1 = new Pin(
                "C1",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C1", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public readonly IPin C2 = new Pin(
                "C2",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C2", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public readonly IPin C3 = new Pin(
                "C3",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C3", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public readonly IPin C4 = new Pin(
                "C4",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C4", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public readonly IPin C5 = new Pin(
                "C5",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C5", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public readonly IPin C6 = new Pin(
                "C6",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C6", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public readonly IPin C7 = new Pin(
                "C7",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C7", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            /// <summary>
            /// Pin Initialize all serial wombat pins
            /// </summary>
            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(D0);
                AllPins.Add(D1);
                AllPins.Add(D2);
                AllPins.Add(D3);

                AllPins.Add(C0);
                AllPins.Add(C1);
                AllPins.Add(C2);
                AllPins.Add(C3);
                AllPins.Add(C4);
                AllPins.Add(C5);
                AllPins.Add(C6);
                AllPins.Add(C7);
            }
        }
    }
}