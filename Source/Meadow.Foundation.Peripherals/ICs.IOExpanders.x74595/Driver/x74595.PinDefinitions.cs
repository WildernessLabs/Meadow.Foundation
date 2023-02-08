using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class x74595
    {
        /// <summary>
        /// x74595 pin definitions class
        /// </summary>
		public class PinDefinitions : IPinDefinitions
        {
            public IPinController Controller { get; set; }

            /// <summary>
            /// All pins
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            /// <summary>
            /// GP0
            /// </summary>
            public IPin GP0 => GetPin(Controller, "GP0", 0x00);

            /// <summary>
            /// GP1
            /// </summary>
            public IPin GP1 => GetPin(Controller, "GP1", 0x01);

            /// <summary>
            /// GP2
            /// </summary>
            public IPin GP2 => GetPin(Controller, "GP2", 0x02);

            /// <summary>
            /// GP3
            /// </summary>
            public IPin GP3 => GetPin(Controller, "GP3", 0x03);

            /// <summary>
            /// GP4
            /// </summary>
            public IPin GP4 => GetPin(Controller, "GP4", 0x04);

            /// <summary>
            /// GP5
            /// </summary>
            public IPin GP5 => GetPin(Controller, "GP5", 0x05);

            /// <summary>
            /// GP6
            /// </summary>
            public IPin GP6 => GetPin(Controller, "GP6", 0x06);

            /// <summary>
            /// GP7
            /// </summary>
            public IPin GP7 => GetPin(Controller, "GP7", 0x07);

            private static IPin GetPin(IPinController controller, string name, byte key)
            {
                return new Pin(
                    controller,
                    name, key,
                    new List<IChannelInfo> {
                        new DigitalChannelInfo(
                            name: name,
                            inputCapable: false,
                            outputCapable: true,
                            pullDownCapable: false,
                            pullUpCapable: false)
                    });
            }

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            public PinDefinitions(x74595 controller)
            {
                Controller = controller;
                InitAllPins();
            }

            /// <summary>
            /// Initialize pins
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
            /// Get enumerator
            /// </summary>
            /// <returns></returns>
            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}