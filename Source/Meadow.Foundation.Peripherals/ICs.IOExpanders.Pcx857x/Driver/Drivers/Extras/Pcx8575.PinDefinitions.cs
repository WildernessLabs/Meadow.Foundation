using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class Pcx8575
    {
        /// <summary>
        /// Pin definitions for 16 pin MCP IO expanders
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
            /// Pin P00
            /// </summary>
            public IPin P00 => new Pin(
                Controller,
                "P00", (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P00"),
                }
            );

            /// <summary>
            /// Pin P01
            /// </summary>
            public IPin P01 => new Pin(
                Controller,
                "P01", (byte)0x01,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P01"),
                }
            );

            /// <summary>
            /// Pin P02
            /// </summary>
            public IPin P02 => new Pin(
                Controller,
                "P02", (byte)0x02,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P02"),
                }
            );

            /// <summary>
            /// Pin P03
            /// </summary>
            public IPin P03 => new Pin(
                Controller,
                "P03", (byte)0x03,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P03"),
                }
            );

            /// <summary>
            /// Pin P04
            /// </summary>
            public IPin P04 => new Pin(
                Controller,
                "P04", (byte)0x04,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P04"),
                }
            );

            /// <summary>
            /// Pin P05
            /// </summary>
            public IPin P05 => new Pin(
                Controller,
                "P05", (byte)0x05,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P05"),
                }
            );

            /// <summary>
            /// Pin P06
            /// </summary>
            public IPin P06 => new Pin(
                Controller,
                "P06", (byte)0x06,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P06"),
                }
            );

            /// <summary>
            /// Pin P07
            /// </summary>
            public IPin P07 => new Pin(
                Controller,
                "P07", (byte)0x07,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P07"),
                }
            );

            /// <summary>
            /// Pin P10
            /// </summary>
            public IPin P10 => new Pin(
                Controller,
                "P10", (byte)0x08,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P10"),
                }
            );

            /// <summary>
            /// Pin P11
            /// </summary>
            public IPin P11 => new Pin(
                Controller,
                "P11", (byte)0x09,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P11"),
                }
            );

            /// <summary>
            /// Pin P12
            /// </summary>
            public IPin P12 => new Pin(
                Controller,
                "P12", (byte)0x0A,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P12"),
                }
            );

            /// <summary>
            /// Pin P13
            /// </summary>
            public IPin P13 => new Pin(
                Controller,
                "P13", (byte)0x0B,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P13"),
                }
            );

            /// <summary>
            /// Pin P14
            /// </summary>
            public IPin P14 => new Pin(
                Controller,
                "P14", (byte)0x0C,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P14"),
                }
            );

            /// <summary>
            /// Pin P15
            /// </summary>
            public IPin P15 => new Pin(
                Controller,
                "P15", (byte)0x0D,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P15"),
                }
            );

            /// <summary>
            /// Pin P16
            /// </summary>
            public IPin P16 => new Pin(
                Controller,
                "P16", (byte)0x0E,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P16"),
                }
            );

            /// <summary>
            /// Pin P17
            /// </summary>
            public IPin P17 => new Pin(
                Controller,
                "P17", (byte)0x0F,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("P17"),
                }
            );

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            public PinDefinitions(Pcx8575 controller)
            {
                Controller = controller;
                InitAllPins();
            }

            /// <summary>
            /// Initialize all pins
            /// </summary>
            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(P00);
                AllPins.Add(P01);
                AllPins.Add(P02);
                AllPins.Add(P03);
                AllPins.Add(P04);
                AllPins.Add(P05);
                AllPins.Add(P06);
                AllPins.Add(P07);

                AllPins.Add(P10);
                AllPins.Add(P11);
                AllPins.Add(P12);
                AllPins.Add(P13);
                AllPins.Add(P14);
                AllPins.Add(P15);
                AllPins.Add(P16);
                AllPins.Add(P17);
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