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

            //            public IPin I2C_SDA => new Pin;
            //            public IPin I2C_SCL => new Ft232Pin();

            public readonly IPin D00 = new Pin(
                "D00",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("D00", interruptCapable: false)
                }
                );

            /// <summary>
            /// Pin Initialize all serial wombat pins
            /// </summary>
            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(this.D00);
            }
        }
    }
}