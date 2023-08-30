using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class Pcx8574
    {
        /// <summary>
        /// Pin definitions for 8 pin MCP IO expanders
        /// </summary>
        public class PinDefinitions : IPinDefinitions
        {
            /// <summary>
            /// The controller for the pins
            /// </summary>
            public IPinController Controller { get; set; }

            /// <summary>
            /// List of pins
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            /// <summary>
            /// Pin P0
            /// </summary>
            public IPin P0 => new Pin(
                Controller,
                "P0", (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P0"),
                }
            );

            /// <summary>
            /// Pin P1
            /// </summary>
            public IPin P1 => new Pin(
                Controller,
                "P1", (byte)0x01,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P1"),
                }
            );

            /// <summary>
            /// Pin P2
            /// </summary>
            public IPin P2 => new Pin(
                Controller,
                "P2", (byte)0x02,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P2"),
                }
            );

            /// <summary>
            /// Pin P3
            /// </summary>
            public IPin P3 => new Pin(
                Controller,
                "P3", (byte)0x03,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P3"),
                }
            );

            /// <summary>
            /// Pin P4
            /// </summary>
            public IPin P4 => new Pin(
                Controller,
                "P4", (byte)0x04,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P4"),
                }
            );

            /// <summary>
            /// Pin P5
            /// </summary>
            public IPin P5 => new Pin(
                Controller,
                "P5", (byte)0x05,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P5"),
                }
            );

            /// <summary>
            /// Pin P6
            /// </summary>
            public IPin P6 => new Pin(
                Controller,
                "P6", (byte)0x06,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P6"),
                }
            );

            /// <summary>
            /// Pin P7
            /// </summary>
            public IPin P7 => new Pin(
                Controller,
                "P7", (byte)0x07,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P7"),
                }
            );

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            public PinDefinitions(Pcx8574 controller)
            {
                Controller = controller;
                InitAllPins();
            }

            /// <summary>
            /// Initalize all pins
            /// </summary>
            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(P0);
                AllPins.Add(P1);
                AllPins.Add(P2);
                AllPins.Add(P3);
                AllPins.Add(P4);
                AllPins.Add(P5);
                AllPins.Add(P6);
                AllPins.Add(P7);
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