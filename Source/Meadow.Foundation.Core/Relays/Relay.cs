using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Peripherals.Relays;

namespace Meadow.Foundation.Relays
{
    /// <summary>
    /// Electrical switch (usually mechanical) that switches on an isolated circuit.
    /// </summary>
    public class Relay : IRelay
    {
        /// <summary>
        /// Returns digital output port
        /// </summary>
        public IDigitalOutputPort DigitalOut { get; protected set; }

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
                // if turning on,
                isOn = value;
                DigitalOut.State = isOn ? onValue : !onValue;
                
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
            this(device.CreateDigitalOutputPort(pin), type) { }

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

            DigitalOut = port;
        }

        /// <summary>
        /// Toggles the relay on or off.
        /// </summary>
        public void Toggle()
        {
            IsOn = !IsOn;
        }
    }
}