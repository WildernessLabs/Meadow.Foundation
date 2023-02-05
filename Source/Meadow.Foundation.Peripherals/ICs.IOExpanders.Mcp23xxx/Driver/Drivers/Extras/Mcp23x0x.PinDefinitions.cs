using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class Mcp23x0x
    {
        /// <summary>
        /// Pin definitions for 8 pin MCP IO expanders
        /// </summary>
        public class PinDefinitions : IPinDefinitions
        {
            public IPinController Controller { get; set; }

            /// <summary>
            /// List of pins
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            /// <summary>
            /// GP0
            /// </summary>
            public IPin GP0 => new Pin(
                Controller,
                "GP0", (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP0", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP1
            /// </summary>
            public IPin GP1 => new Pin(
                Controller,
                "GP1", (byte)0x01,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP1", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP2
            /// </summary>
            public IPin GP2 => new Pin(
                Controller,
                "GP2", (byte)0x02,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP2", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP3
            /// </summary>
            public IPin GP3 => new Pin(
                Controller,
                "GP3", (byte)0x03,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP3", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP4
            /// </summary>
            public IPin GP4 => new Pin(
                Controller,
                "GP4", (byte)0x04,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP4", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP5
            /// </summary>
            public IPin GP5 => new Pin(
                Controller,
                "GP5", (byte)0x05,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP5", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP6
            /// </summary>
            public IPin GP6 => new Pin(
                Controller,
                "GP6", (byte)0x06,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP6", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GP7
            /// </summary>
            public IPin GP7 => new Pin(
                Controller,
                "GP7", (byte)0x07,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GP7", pullDownCapable:false),
                }
            );

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            public PinDefinitions(Mcp23x0x controller)
            {
                controller = controller;
                InitAllPins();
            }

            /// <summary>
            /// Initalize all pins
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