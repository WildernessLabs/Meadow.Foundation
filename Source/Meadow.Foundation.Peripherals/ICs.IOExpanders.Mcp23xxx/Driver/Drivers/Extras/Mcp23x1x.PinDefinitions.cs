using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class Mcp23x1x
    {
        /// <summary>
        /// Pin definitions for 16 pin MCP IO expanders
        /// </summary>
        public class PinDefinitions : IPinDefinitions
        {
            public IPinController Controller { get; set; }

            /// <summary>
            /// List of pins
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            /// <summary>
            /// GPA0
            /// </summary>
            public IPin GPA0 => new Pin(
                Controller,
                "GPA0", (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPA0", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPA1
            /// </summary>
            public IPin GPA1 => new Pin(
                Controller,
                "GPA1", (byte)0x01,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPA1", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPA2
            /// </summary>
            public IPin GPA2 => new Pin(
                Controller,
                "GPA2", (byte)0x02,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPA2", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPA3
            /// </summary>
            public IPin GPA3 => new Pin(
                Controller,
                "GPA3", (byte)0x03,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPA3", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPA4
            /// </summary>
            public IPin GPA4 => new Pin(
                Controller,
                "GPA4", (byte)0x04,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPA4", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPA5
            /// </summary>
            public IPin GPA5 => new Pin(
                Controller,
                "GPA5", (byte)0x05,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPA5", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPA6
            /// </summary>
            public IPin GPA6 => new Pin(
                Controller,
                "GPA6", (byte)0x06,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPA6", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPA7
            /// </summary>
            public IPin GPA7 => new Pin(
                Controller,
                "GPA7", (byte)0x07,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPA7", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPB0
            /// </summary>
            public IPin GPB0 => new Pin(
                Controller,
                "GPB0", (byte)0x08,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPB0", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPB1
            /// </summary>
            public IPin GPB1 => new Pin(
                Controller,
                "GPB1", (byte)0x09,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPB1", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPB2
            /// </summary>
            public IPin GPB2 => new Pin(
                Controller,
                "GPB2", (byte)0x0A,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPB2", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPB3
            /// </summary>
            public IPin GPB3 => new Pin(
                Controller,
                "GPB3", (byte)0x0B,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPB3", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPB4
            /// </summary>
            public IPin GPB4 => new Pin(
                Controller,
                "GPB4", (byte)0x0C,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPB4", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPB5
            /// </summary>
            public IPin GPB5 => new Pin(
                Controller,
                "GPB5", (byte)0x0D,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPB5", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPB6
            /// </summary>
            public IPin GPB6 => new Pin(
                Controller,
                "GPB6", (byte)0x0E,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPB6", pullDownCapable:false),
                }
            );

            /// <summary>
            /// GPB7
            /// </summary>
            public IPin GPB7 => new Pin(
                Controller,
                "GPB7", (byte)0x0F,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("GPB7", pullDownCapable:false),
                }
            );

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            public PinDefinitions(Mcp23x1x controller)
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
                AllPins.Add(GPA0);
                AllPins.Add(GPA1);
                AllPins.Add(GPA2);
                AllPins.Add(GPA3);
                AllPins.Add(GPA4);
                AllPins.Add(GPA5);
                AllPins.Add(GPA6);
                AllPins.Add(GPA7);

                AllPins.Add(GPB0);
                AllPins.Add(GPB1);
                AllPins.Add(GPB2);
                AllPins.Add(GPB3);
                AllPins.Add(GPB4);
                AllPins.Add(GPB5);
                AllPins.Add(GPB6);
                AllPins.Add(GPB7);
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