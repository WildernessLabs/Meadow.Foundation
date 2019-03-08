using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Switches;
using System;

namespace Meadow.Foundation.Sensors.Switches
{
    /// <summary>
    /// Represents a simple, two position, Single-Pole-Dual-Throw (SPDT) switch that closes a circuit 
    /// to either ground/common or high depending on position.
    /// 
    /// Note: This class is not yet implemented.
    /// </summary>
    public class SpdtSwitch : ISwitch, ISensor
    {
        /// <summary>
        /// Describes whether or not the switch circuit is closed/connected (IsOn = true), or open (IsOn = false).
        /// </summary>
        public bool IsOn
        {
            get => DigitalIn.State;
            protected set
            {
                Changed(this, new EventArgs());
            }
        }

        /// <summary>
        /// Returns the DigitalInputPort.
        /// </summary>
        public IDigitalInputPort DigitalIn { get; protected set; }

        /// <summary>
        /// Raised when the switch circuit is opened or closed.
        /// </summary>
        public event EventHandler Changed = delegate { };

        /// <summary>
        /// Instantiates a new SpdtSwitch object with the center pin connected to the specified digital pin, one pin connected to common/ground and one pin connected to high/3.3V.
        /// </summary>
        /// <param name="pin"></param>
        public SpdtSwitch(IIODevice device, IPin pin)
        {
            DigitalIn = device.CreateDigitalInputPort(pin, true, false, ResistorMode.Disabled);            
            DigitalIn.Changed += DigitalInChanged;
        }

        private void DigitalInChanged(object sender, PortEventArgs e)
        {
            IsOn = DigitalIn.State;
        }
    }
}