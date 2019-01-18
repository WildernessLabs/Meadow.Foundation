using System;
using Meadow;
using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Switches
{
    /// <summary>
    /// Represents a simple, on/off, Single-Pole-Single-Throw (SPST) switch that closes a circuit 
    /// to either ground/common or high. 
    /// 
    /// Use the SwitchCircuitTerminationType to specify whether the other side of the switch
    /// terminates to ground or high.
    /// </summary>
    public class SpstSwitch : ISwitch, ISensor
    {
        public event EventHandler Changed = delegate { };

        public DigitalInputPort DigitalIn { get; protected set; }

        public bool IsOn
        {
            get => DigitalIn.State;
            protected set => Changed(this, new EventArgs());
        }

        public SpstSwitch(IDigitalPin pin, CircuitTerminationType type)
        {
            // if we terminate in ground, we need to pull the port high to test for circuit completion, otherwise down.
            var resistorMode = ResistorMode.Disabled;
            switch (type)
            {
                case CircuitTerminationType.CommonGround:
                    resistorMode = ResistorMode.PullUp;
                    break;
                case CircuitTerminationType.High:
                    resistorMode = ResistorMode.PullDown;
                    break;
                case CircuitTerminationType.Floating:
                    resistorMode = ResistorMode.Disabled;
                    break;
            } 

            this.DigitalIn = new DigitalInputPort(pin, true, resistorMode);

            this.DigitalIn.Changed += DigitalIn_Changed;
        }

        private void DigitalIn_Changed(object sender, PortEventArgs e)
        {
            IsOn = DigitalIn.State;
        }
    }
}
