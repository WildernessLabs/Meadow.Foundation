using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp3002
    {
        /// <summary>
        /// Mcp3002 pin definition class
        /// </summary>
        public class PinDefinitions : IPinDefinitions
        {
            /// <summary>
            /// Analog-digital converter precision
            /// </summary>
            public virtual byte ADCPrecisionBits => 10;

            /// <summary>
            /// Collection of pins
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            /// <summary>
            /// The pin controller
            /// </summary>
            public IPinController Controller { get; set; }

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            public PinDefinitions(Mcp3xxx mcp)
            {
                Controller = mcp;
                InitAllPins();
            }

            /// <summary>
            /// Pin CH0
            /// </summary>
            public IPin CH0 => new Pin(
                Controller,
                "CH0",
                (byte)0x00,
                new List<IChannelInfo> {
                    new AnalogChannelInfo("A0", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin CH1
            /// </summary>
            public IPin CH1 => new Pin(
                Controller,
                "CH1",
                (byte)0x01,
                new List<IChannelInfo> {
                    new AnalogChannelInfo("A1", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin Initialize all serial wombat pins
            /// </summary>
            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(CH0);
                AllPins.Add(CH1);
            }

            /// <summary>
            /// Get Enumerator
            /// </summary>
            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}