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
    /// 
    /// Note: This class is not yet implemented.
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
        public IDigitalInputPort DigitalIn { get; protected set; }

        /// <summary>
        /// Instantiates a new SpstSwitch object connected to the specified digital pin, and with the specified CircuitTerminationType in the type parameter.
        /// </summary>
        /// <param name="pin"></param>
        public SpstSwitch(IIODevice device, IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, int debounceDuration = 20)
        {
            DigitalIn = device.CreateDigitalInputPort(pin, interruptMode, resistorMode, debounceDuration);      
            DigitalIn.Changed += DigitalInChanged;
        }

        /// <summary>
        /// Instantiates a new SpstSwitch object connected to the specified digital pin, and with the specified CircuitTerminationType in the type parameter.
        /// </summary>
        public SpstSwitch(IDigitalInputPort interruptPort)
        {
            DigitalIn = interruptPort;

            // wire up the interrupt handler
            DigitalIn.Changed += DigitalInChanged;
        }

        void DigitalInChanged(object sender, DigitalInputPortEventArgs e)
        {
            IsOn = DigitalIn.State;
        }
    }
}
