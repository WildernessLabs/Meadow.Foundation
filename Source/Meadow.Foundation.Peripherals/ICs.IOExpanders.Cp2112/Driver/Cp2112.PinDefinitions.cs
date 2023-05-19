using Meadow.Hardware;
using System.Collections;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Cp2112
    {
        public class PinDefinitions : IPinDefinitions
        {
            public IEnumerator<IPin> GetEnumerator() => AllPins.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// Collection of pins
            /// </summary>
            public IList<IPin> AllPins { get; } = new List<IPin>();

            public IPinController Controller { get; set; }

            /// <summary>
            /// Create a new PinDefinitions object
            /// </summary>
            internal PinDefinitions(Cp2112 controller)
            {
                Controller = controller;
                InitAllPins();
            }

            public IPin IO0 => new Pin(
                Controller,
                "IO0",
                (byte)(1 << 0),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("IO0", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public IPin IO1 => new Pin(
                Controller,
                "IO1",
                (byte)(1 << 1),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("IO1", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public IPin IO2 => new Pin(
                Controller,
                "IO2",
                (byte)(1 << 2),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("IO2", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public IPin IO3 => new Pin(
                Controller,
                "IO3",
                (byte)(1 << 3),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("IO3", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public IPin IO4 => new Pin(
                Controller,
                "IO4",
                (byte)(1 << 4),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("IO4", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public IPin IO5 => new Pin(
                Controller,
                "IO5",
                (byte)(1 << 5),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("IO5", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public IPin IO6 => new Pin(
                Controller,
                "IO6",
                (byte)(1 << 6),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("IO6", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            public IPin IO7 => new Pin(
                Controller,
                "IO7",
                (byte)(1 << 7),
                new List<IChannelInfo> {
                    new DigitalChannelInfo("IO7", interruptCapable: false, pullUpCapable: false, pullDownCapable: false)
                });

            protected void InitAllPins()
            {
                // add all our pins to the collection
                AllPins.Add(IO0);
                AllPins.Add(IO1);
                AllPins.Add(IO2);
                AllPins.Add(IO3);
                AllPins.Add(IO4);
                AllPins.Add(IO5);
                AllPins.Add(IO6);
                AllPins.Add(IO7);
            }
        }
    }
}