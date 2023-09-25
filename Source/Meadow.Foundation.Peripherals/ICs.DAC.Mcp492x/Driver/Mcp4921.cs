using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.DAC
{
    public class Mcp4921 : Mcp492x
    {
        /// <summary>
        /// The pins
        /// </summary>
        public PinDefinitions Pins { get; }

        public Mcp4921(ISpiBus spiBus, IDigitalOutputPort chipSelect)
            : base(spiBus, chipSelect)
        {
            Pins = new PinDefinitions(this);
        }

        public class PinDefinitions : IPinDefinitions
        {
            /// <summary>
            /// Analog-digital converter precision
            /// </summary>
            public virtual byte DACPrecisionBits => 12;

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
            public PinDefinitions(Mcp492x mcp)
            {
                Controller = mcp;
            }

            /// <summary>
            /// Channel A pin
            /// </summary>
            public IPin ChannelA => new Pin(
                Controller,
                "A",
                Channel.ChannelA,
                new List<IChannelInfo> {
                    new AnalogChannelInfo("A", DACPrecisionBits, false, true),
                }
            );

            /// <summary>
            /// Get Enumerator
            /// </summary>
            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}