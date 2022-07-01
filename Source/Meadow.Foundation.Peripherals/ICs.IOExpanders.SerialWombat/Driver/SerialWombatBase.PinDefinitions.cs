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
                    new DigitalChannelInfo("WP0", pullDownCapable:false),
                }
            );

            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(this.WP0);
            }

            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}