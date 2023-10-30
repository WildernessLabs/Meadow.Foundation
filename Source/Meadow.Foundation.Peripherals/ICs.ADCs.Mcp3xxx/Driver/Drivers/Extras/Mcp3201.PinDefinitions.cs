using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp3201
    {
        /// <summary>
        /// Mcp3001 pin definition class
        /// </summary>
        public class PinDefinitions : IPinDefinitions
        {
            /// <summary>
            /// Analog-digital converter precision
            /// </summary>
            public virtual byte ADCPrecisionBits => 12;

            /// <summary>
            /// Collection of pins
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            /// <inheritdoc/>
            public IPinController? Controller { get; set; }

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            public PinDefinitions(Mcp3xxx mcp)
            {
                Controller = mcp;
                InitAllPins();
            }

            /// <summary>
            /// Pin INPlus
            /// </summary>
            public IPin INPlus => new Pin(
                Controller,
                "IN+",
                (byte)0x00,
                new List<IChannelInfo> {
                    new AnalogChannelInfo("IN+", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin INMinus
            /// </summary>
            public IPin INMinus => new Pin(
                Controller,
                "IN-",
                (byte)0x01,
                new List<IChannelInfo> {
                    new AnalogChannelInfo("IN-", ADCPrecisionBits, true, false),
                }
            );

            /// <summary>
            /// Pin Initialize all serial wombat pins
            /// </summary>
            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(INPlus);
                AllPins.Add(INMinus);
            }

            /// <summary>
            /// Get Enumerator
            /// </summary>
            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}