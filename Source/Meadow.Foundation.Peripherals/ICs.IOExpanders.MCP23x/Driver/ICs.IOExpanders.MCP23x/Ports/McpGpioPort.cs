using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Meadow.Hardware;

[assembly: InternalsVisibleTo("ICs.IOExpenders.MCP23x.UnitTests")]
namespace Meadow.Foundation.ICs.IOExpanders.Ports
{
    public class McpGpioPort : IPinDefinitions
    {
        public McpGpioPort(string namePrefix = "GP")
        {
            if (namePrefix == null)
            {
                throw new ArgumentNullException(nameof(namePrefix));
            }

            IPin CreatePin(byte i)
            {
                var name = $"{namePrefix}{i}";
                return new Pin(
                    name,
                    i,
                    new List<IChannelInfo>
                    {
                        new DigitalChannelInfo(name, pullDownCapable: false)
                    });
            }

            GP0 = CreatePin(0x00);
            GP1 = CreatePin(0x01);
            GP2 = CreatePin(0x02);
            GP3 = CreatePin(0x03);
            GP4 = CreatePin(0x04);
            GP5 = CreatePin(0x05);
            GP6 = CreatePin(0x06);
            GP7 = CreatePin(0x07);

            AllPins = new[]
            {
                GP0,
                GP1,
                GP2,
                GP3,
                GP4,
                GP5,
                GP6,
                GP7
            };
        }

        public IPin GP0 { get; }
        public IPin GP1 { get; }
        public IPin GP2 { get; }
        public IPin GP3 { get; }
        public IPin GP4 { get; }
        public IPin GP5 { get; }
        public IPin GP6 { get; }
        public IPin GP7 { get; }

        public IList<IPin> AllPins { get; }

        /// <summary>
        /// Raised when the value of a pin configured for input changes. Use in
        /// conjunction with parallel port reads via ReadFromPorts(). When using
        /// individual `DigitalInputPort` objects, each one will have their own
        /// `Changed` event
        /// </summary>
        // TODO: make a custom event args that has the pin that triggered
        public event EventHandler<IOExpanderPortInputChangedEventArgs> InputChanged = delegate { };

        /// <summary>
        /// Invoke the input changed event. Called from <see cref="Mcp23x"/>.
        /// </summary>
        /// <param name="e"></param>
        internal void InvokeInputChanged(IOExpanderPortInputChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            InputChanged?.Invoke(this, e);
        }
    }
}
