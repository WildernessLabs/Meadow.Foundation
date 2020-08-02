using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders.Ports
{
    public class Mcp23xPorts : IMcpGpioPorts
    {
        private readonly IMcpGpioPort[] _ports;

        public Mcp23xPorts(params IMcpGpioPort[] ports)
        {
            if (ports == null)
            {
                throw new ArgumentNullException(nameof(ports));
            }

            if (ports.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty collection.", nameof(ports));
            }

            if (ports.Any(p => p == null))
            {
                throw new ArgumentNullException(nameof(ports));
            }

            _ports = ports;
            AllPins = _ports.Length == 1 ? _ports[0].AllPins : _ports.SelectMany(p => p.AllPins).ToArray();
        }

        public IList<IPin> AllPins { get; }

        public int Count => _ports.Length;

        public IMcpGpioPort this[int index] => _ports[index];

        public IEnumerator<IMcpGpioPort> GetEnumerator()
        {
            return (IEnumerator<IMcpGpioPort>) _ports.GetEnumerator();
        }

        public int GetPortIndex(IMcpGpioPort port)
        {
            if (port == null)
            {
                throw new ArgumentNullException(nameof(port));
            }

            var index = Array.IndexOf(_ports, port);
            if (index < 0)
            {
                throw new ArgumentException("Provided port is not part of this device.", nameof(port));
            }

            return index;
        }

        public int GetPortIndexOfPin(IPin pin)
        {
            if (pin == null)
            {
                throw new ArgumentNullException(nameof(pin));
            }

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
            if (pin == null)
            {
                throw new ArgumentNullException(nameof(pin));
            }

            return this[GetPortIndexOfPin(pin)];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
