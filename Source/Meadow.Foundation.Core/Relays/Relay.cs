using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Relays;
using System;

namespace Meadow.Foundation.Relays
{
    /// <summary>
    /// Electrical switch (usually mechanical) that switches on an isolated circuit.
    /// </summary>
    public class Relay : IRelay, IDisposable
    {
        bool createdPort = false;

        /// <summary>
        /// Is the peripheral disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Returns digital output port
        /// </summary>
        protected IDigitalOutputPort Port { get; set; }

        /// <summary>
        /// Returns type of relay.
        /// </summary>
        public RelayType Type { get; protected set; }

        /// <summary>
        /// Whether or not the relay is on. Setting this property will turn it on or off.
        /// </summary>
        public bool IsOn 
        {
            get => isOn; 
            set
            {
                isOn = value;
                Port.State = isOn ? onValue : !onValue;
            }
        } 
        bool isOn = false;
        readonly bool onValue = true;

        /// <summary>
        /// Creates a new Relay on an IDigitalOutputPort.
        /// </summary>
        /// <param name="device">IDigitalOutputController to create digital output port</param>
        /// <param name="pin">Pin connected to relay</param>
        /// <param name="type">Relay type</param>
        public Relay(IDigitalOutputController device, IPin pin, RelayType type = RelayType.NormallyOpen) :
            this(device.CreateDigitalOutputPort(pin), type)
        {
            createdPort = true;
        }

        /// <summary>
        /// Creates a new Relay on an IDigitalOutputPort. Allows you 
        /// to use any peripheral that exposes ports that conform to the
        /// IDigitalOutputPort, such as the MCP23008.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="type"></param>
        public Relay(IDigitalOutputPort port, RelayType type = RelayType.NormallyOpen)
        {
            // if it's normally closed, we have to invert the "on" value
            Type = type;
            onValue = Type != RelayType.NormallyClosed;

            Port = port;
        }

        /// <summary>
        /// Toggles the relay on or off.
        /// </summary>
        public void Toggle()
        {
            IsOn = !IsOn;
        }

        /// <summary>
        /// Dispose peripheral
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    Port.Dispose();
                }
                IsDisposed = true;
            }
        }

        /// <summary>
        /// Dispose Peripheral
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}