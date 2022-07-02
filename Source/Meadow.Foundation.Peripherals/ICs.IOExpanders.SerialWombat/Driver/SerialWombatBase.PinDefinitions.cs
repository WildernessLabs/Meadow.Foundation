using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        public class PinDefinitions : IPinDefinitions
        {
            public IList<IPin> AllPins { get; } = new List<IPin>();

            public PinDefinitions()
            {
                InitAllPins();
            }

            public readonly IPin WP0 = new Pin(
                "WP0",
                (byte)0x00,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP0", interruptCapable: false),
                }
            );

            public readonly IPin WP1 = new Pin(
                "WP1",
                (byte)0x01,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP1", interruptCapable: false),
                }
            );

            public readonly IPin WP2 = new Pin(
                "WP2",
                (byte)0x02,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP2", interruptCapable: false),
                }
            );

            public readonly IPin WP5 = new Pin(
                "WP5",
                (byte)0x05,
                new List<IChannelInfo> {
                    new DigitalChannelInfo("WP5", interruptCapable: false),
                }
            );

            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(this.WP0);
                AllPins.Add(this.WP1);
                AllPins.Add(this.WP2);
                AllPins.Add(this.WP5);
            }

            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}