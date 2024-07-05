using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an Sc16is7x2 SPI/I2C dual UART (with 8 GPIO's)
    /// </summary>
    public abstract partial class Sc16is7x2
    {
        /// <summary>
        /// Pin definitions for the 8 GPIO pins
        /// 03.12.2023: This is a copy of the Mcp23x0x.PinDefinitions.cs file.
        /// </summary>
        public class PinDefinitions : IPinDefinitions
        {
            /// <summary>
            /// The controller for the pins
            /// </summary>
            public IPinController? Controller { get; set; }

            /// <summary>
            /// List of pins
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            /// <summary>
            /// Pin GP0
            /// </summary>
            public IPin GP0 => new Pin(
                Controller,
                "GP0", (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP0"),
                }
            );

            /// <summary>
            /// Pin GP1
            /// </summary>
            public IPin GP1 => new Pin(
                Controller,
                "GP1", (byte)0x01,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP1"),
                }
            );

            /// <summary>
            /// Pin GP2
            /// </summary>
            public IPin GP2 => new Pin(
                Controller,
                "GP2", (byte)0x02,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP2"),
                }
            );

            /// <summary>
            /// Pin GP3
            /// </summary>
            public IPin GP3 => new Pin(
                Controller,
                "GP3", (byte)0x03,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP3"),
                }
            );

            /// <summary>
            /// Pin GP4
            /// </summary>
            public IPin GP4 => new Pin(
                Controller,
                "GP4", (byte)0x04,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP4"),
                }
            );

            /// <summary>
            /// Pin GP5
            /// </summary>
            public IPin GP5 => new Pin(
                Controller,
                "GP5", (byte)0x05,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP5"),
                }
            );

            /// <summary>
            /// Pin GP6
            /// </summary>
            public IPin GP6 => new Pin(
                Controller,
                "GP6", (byte)0x06,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP6"),
                }
            );

            /// <summary>
            /// Pin GP7
            /// </summary>
            public IPin GP7 => new Pin(
                Controller,
                "GP7", (byte)0x07,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP7"),
                }
            );

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            public PinDefinitions(Sc16is7x2 controller)
            {
                Controller = (IPinController?)controller;
                InitAllPins();
            }

            /// <summary>
            /// Initialize all pins
            /// </summary>
            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(GP0);
                AllPins.Add(GP1);
                AllPins.Add(GP2);
                AllPins.Add(GP3);
                AllPins.Add(GP4);
                AllPins.Add(GP5);
                AllPins.Add(GP6);
                AllPins.Add(GP7);
            }

            /// <summary>
            /// Get Pins
            /// </summary>
            /// <returns>IEnumerator of IPin with all pins</returns>
            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
