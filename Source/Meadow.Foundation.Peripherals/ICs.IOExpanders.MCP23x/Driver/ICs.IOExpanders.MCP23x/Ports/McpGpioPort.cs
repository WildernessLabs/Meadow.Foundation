using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Meadow.Hardware;

[assembly: InternalsVisibleTo("ICs.IOExpenders.MCP23x.UnitTests")]

namespace Meadow.Foundation.ICs.IOExpanders.Ports
{
    public class McpGpioPort : IMcpGpioPort
    {
        /// <inheritdoc />
        public event EventHandler<IOExpanderPortInputChangedEventArgs> InputChanged = delegate { };

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


        /// <inheritdoc />
        public void InvokeInputChanged(IOExpanderPortInputChangedEventArgs e)
        {
            Console.WriteLine("Port interrupt invoke");
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            InputChanged?.Invoke(this, e);
            Console.WriteLine("Invoke done");
        }
    }
}
