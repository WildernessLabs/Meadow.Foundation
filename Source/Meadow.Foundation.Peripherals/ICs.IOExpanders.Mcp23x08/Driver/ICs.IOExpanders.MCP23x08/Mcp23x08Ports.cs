using System;
using System.Collections;
using System.Collections.Generic;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class Mcp23x08Ports : McpGpioPort, IMcpGpioPorts
    {
        private readonly IMcpGpioPort[] _ports;

        internal Mcp23x08Ports()
        {
            _ports = new IMcpGpioPort[]
            {
                this
            };
        }

        public int Count => 1;

        public IMcpGpioPort this[int index] => _ports[index];

        public IEnumerator<IMcpGpioPort> GetEnumerator()
        {
            return ((IEnumerable<IMcpGpioPort>)_ports).GetEnumerator();
        }

        public int GetPortIndex(IMcpGpioPort port)
        {
            if (port == this)
            {
                return 0;
            }

            throw new ArgumentException("Provided port is not part of this device.", nameof(port));
        }

        public int GetPortIndexOfPin(IPin pin)
        {
            for (var i = 0; i < Count; i++)
            {
                if (this[i].AllPins.Contains(pin))
                {
                    return i;
                }
            }

            throw new ArgumentException("Pin is not from this port set", nameof(pin));
        }

        public IMcpGpioPort GetPortOfPin(IPin pin)
        {
            return this[GetPortIndexOfPin(pin)];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
