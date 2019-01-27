using System;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Switches;

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
        /// <summary>
        /// Describes whether or not the switch circuit is closed/connected (IsOn = true), or open (IsOn = false).
        /// </summary>
        public bool IsOn
        {
            get => DigitalIn.State;
            protected set => Changed(this, new EventArgs());
        }

        /// <summary>
        /// Raised when the switch circuit is opened or closed.
        /// </summary>
        public event EventHandler Changed = delegate { };

        /// <summary>
        /// Returns the DigitalInputPort.
        /// </summary>
        public DigitalInputPort DigitalIn { get; protected set; }

        /// <summary>
        /// Instantiates a new SpstSwitch object connected to the specified digital pin, and with the specified CircuitTerminationType in the type parameter.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="type"></param>
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

            DigitalIn = new DigitalInputPort(pin, true, resistorMode);

            DigitalIn.Changed += DigitalIn_Changed;
        }

        private void DigitalIn_Changed(object sender, PortEventArgs e)
        {
            IsOn = DigitalIn.State;
        }
    }
}
