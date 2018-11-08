using Meadow;
using Meadow.Hardware;
using System;

namespace Netduino.Foundation.Relays
{
    public class Relay : IRelay
    {
        public IDigitalOutputPort DigitalOut { get; protected set; }

        public RelayType Type { get; protected set; }

        public bool IsOn {
            get => _isOn; 
            set
            {
                // if turning on,
                _isOn = value;
                DigitalOut.State = _isOn ? _onValue : !_onValue;
                
            }
        } protected bool _isOn = false;
        protected bool _onValue = true;

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
            if (Type == RelayType.NormallyClosed)
            {
                _onValue = false;
            }

            DigitalOut = port;
        }

        public Relay(Pins pin, RelayType type = RelayType.NormallyOpen)
        {
            // if it's normally closed, we have to invert the "on" value
            Type = type;
            if (Type == RelayType.NormallyClosed)
            {
                _onValue = false;
            }

            // create a digital output port shim
            DigitalOut = new DigitalOutputPort(pin, !_onValue);
        }

        public void Toggle()
        {
            IsOn = !IsOn;
        }
    }
}