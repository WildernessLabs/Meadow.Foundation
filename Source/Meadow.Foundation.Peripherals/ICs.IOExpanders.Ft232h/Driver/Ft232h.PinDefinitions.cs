using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents the pin definitions for the Ft232h IC.
    /// </summary>
    public partial class Ft232h
    {
        /// <summary>
        /// Defines the pin definitions for the Ft232h IC.
        /// </summary>
        public class PinDefinitions : IPinDefinitions
        {
            /// <summary>
            /// Gets an enumerator for all the pins.
            /// </summary>
            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Collection of all pins.
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            /// <summary>
            /// The pin controller
            /// </summary>
            public IPinController? Controller { get; set; }

            /// <summary>
            /// Creates a new PinDefinitions object.
            /// </summary>
            /// <param name="controller">The Ft232h controller associated with the pins.</param>
            internal PinDefinitions(Ft232h controller)
            {
                Controller = controller;
                InitAllPins();
            }

            // Aliases
            /// <summary>
            /// Gets the IPin representing the SPI clock (SCK) pin.
            /// </summary>
            public IPin SPI_SCK => D0;

            /// <summary>
            /// Gets the IPin representing the SPI data out (COPI) pin.
            /// </summary>
            public IPin SPI_COPI => D1;

            /// <summary>
            /// Gets the IPin representing the SPI data in (CIPO) pin.
            /// </summary>
            public IPin SPI_CIPO => D2;

            /// <summary>
            /// Gets the IPin representing the SPI chip select (CS0) pin.
            /// </summary>
            public IPin SPI_CS0 => D3;

            /// <summary>
            /// Gets the IPin representing the I2C clock (SCL) pin.
            /// </summary>
            public IPin I2C_SCL => D0;

            /// <summary>
            /// Gets the IPin representing the I2C data (SDA) pin.
            /// </summary>
            public IPin I2C_SDA => D1;


            /// <summary>
            /// Pin D0 definition.
            /// </summary>
            public IPin D0 => new Pin(
                Controller,
                "D0",
                (byte)0x10,
                new List<IChannelInfo> {
                    new SpiChannelInfo("SPI_SCK", SpiLineType.Clock),
                    new I2cChannelInfo("I2C_SCL", I2cChannelFunctionType.Clock)
                });

            /// <summary>
            /// Pin D1 definition.
            /// </summary>
            public IPin D1 => new Pin(
                Controller,
                "D1",
                (byte)0x11,
                new List<IChannelInfo> {
                    new SpiChannelInfo("SPI_COPI", SpiLineType.COPI),
                    new I2cChannelInfo("I2C_SDA", I2cChannelFunctionType.Data)
                });

            /// <summary>
            /// Pin D2 definition.
            /// </summary>
            public IPin D2 => new Pin(
                Controller,
                "D2",
                (byte)0x12,
                new List<IChannelInfo> {
                    new SpiChannelInfo("SPI_CIPO", SpiLineType.CIPO)
                });

            /// <summary>
            /// Pin D3 definition.
            /// </summary>
            public IPin D3 => new Pin(
                Controller,
                "D3",
                (byte)0x12,
                new List<IChannelInfo> {
                    new SpiChannelInfo("SPI_CS0", SpiLineType.ChipSelect)
                });

            // TODO: D4-D7 can be used as CS, and (probably??) GPIO. The docs are not terribly clear on this.  Maybe just outputs and direct write the CS?

            /// <summary>
            /// Pin SPI_COPI_D1 definition.
            /// </summary>
            public IPin SPI_COPI_D1 => new Pin(
                Controller,
                "SPI_COPI_D1",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("D1", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            /// <summary>
            /// Pin C0 definition.
            /// </summary>
            public IPin C0 => new Pin(
                Controller,
                "C0",
                (byte)(1 << 0),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C0", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            /// <summary>
            /// Pin C1 definition.
            /// </summary>
            public IPin C1 => new Pin(
                Controller,
                "C1",
                (byte)(1 << 1),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C1", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            /// <summary>
            /// Pin C2 definition.
            /// </summary>
            public IPin C2 => new Pin(
                Controller,
                "C2",
                (byte)(1 << 2),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C2", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            /// <summary>
            /// Pin C3 definition.
            /// </summary>
            public IPin C3 => new Pin(
                Controller,
                "C3",
                (byte)(1 << 3),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C3", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            /// <summary>
            /// Pin C4 definition.
            /// </summary>
            public IPin C4 => new Pin(
                Controller,
                "C4",
                (byte)(1 << 4),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C4", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            /// <summary>
            /// Pin C5 definition.
            /// </summary>
            public IPin C5 => new Pin(
                Controller,
                "C5",
                (byte)(1 << 5),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C5", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            /// <summary>
            /// Pin C6 definition.
            /// </summary>
            public IPin C6 => new Pin(
                Controller,
                "C6",
                (byte)(1 << 6),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C6", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            /// <summary>
            /// Pin C7 definition.
            /// </summary>
            public IPin C7 => new Pin(
                Controller,
                "C7",
                (byte)(1 << 7),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("C7", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            /// <summary>
            /// Initializes all serial wombat pins.
            /// </summary>
            protected void InitAllPins()
            {
                // Add all our pins to the collection
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
