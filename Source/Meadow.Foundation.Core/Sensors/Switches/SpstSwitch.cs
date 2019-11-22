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
        #region Properties

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

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private SpstSwitch() { }

        /// <summary>
        /// Instantiates a new SpstSwitch object connected to the specified digital pin, and with the specified CircuitTerminationType in the type parameter.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="pin"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        /// <param name="debounceDuration"></param>
        /// <param name="glitchFilterCycleCount"></param>
        public SpstSwitch(IIODevice device, IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, int debounceDuration = 20, int glitchFilterCycleCount = 0) :
            this(device.CreateDigitalInputPort(pin, interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount)) { }

        /// <summary>
        /// Creates a SpstSwitch on a especified interrupt port
        /// </summary>
        /// <param name="interruptPort"></param>
        public SpstSwitch(IDigitalInputPort interruptPort)
        {
            DigitalIn = interruptPort;
            DigitalIn.Changed += DigitalInChanged;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Event handler when switch value has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DigitalInChanged(object sender, DigitalInputPortEventArgs e)
        {
            IsOn = DigitalIn.State;
        }

        #endregion
    }
}
