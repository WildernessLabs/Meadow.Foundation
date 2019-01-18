using Meadow;
using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Switches
{
    /// <summary>
    /// Represents a simple, two position, Single-Pole-Dual-Throw (SPDT) switch that closes a circuit 
    /// to either ground/common or high depending on position.
    /// </summary>
    public class SpdtSwitch : ISwitch, ISensor
    {
        public event EventHandler Changed = delegate { };

        public DigitalInputPort DigitalIn { get; protected set; }

        public bool IsOn
        {
            get => DigitalIn.State;
            protected set
            {
                Changed(this, new EventArgs());
            }
        }

        public SpdtSwitch(IDigitalPin pin)
        {
            var resistorMode = ResistorMode.Disabled;

            DigitalIn = new DigitalInputPort(pin, true, resistorMode);

            DigitalIn.Changed += DigitalIn_Changed;
        }

        private void DigitalIn_Changed(object sender, PortEventArgs e)
        {
            IsOn = DigitalIn.State;
        }
    }
}