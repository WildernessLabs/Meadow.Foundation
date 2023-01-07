using Meadow.Hardware;
using Meadow.Peripherals.Switches;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Switches
{
    /// <summary>
    /// Represents a simple, on/off, Single-Pole-Single-Throw (SPST) switch that closes a circuit 
    /// to either ground/common or high. 
    /// 
    /// Use the SwitchCircuitTerminationType to specify whether the other side of the switch
    /// terminates to ground or high.
    /// </summary>
    public class SpstSwitch : ISwitch, IDisposable
    {
        /// <summary>
        /// Returns the DigitalInputPort.
        /// </summary>
        protected IDigitalInputPort DigitalIn { get; set; }

        /// <summary>
        /// Track if we created the input port in the PushButton instance (true)
        /// or was it passed in via the ctor (false)
        /// </summary>
        protected bool ShouldDisposePort = false;

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
        /// Is the peripheral disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Instantiates a new SpstSwitch object connected to the specified digital pin, and with the specified CircuitTerminationType in the type parameter.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="pin"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        public SpstSwitch(
            IDigitalInputController device, 
            IPin pin, 
            InterruptMode interruptMode, 
            ResistorMode resistorMode) 
            : this (
                device.CreateDigitalInputPort(pin, interruptMode, resistorMode, TimeSpan.FromMilliseconds(20), 
                TimeSpan.Zero))
        {
            ShouldDisposePort = true;
        }

        /// <summary>
        /// Instantiates a new SpstSwitch object connected to the specified digital pin, and with the specified CircuitTerminationType in the type parameter.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="pin"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        /// <param name="debounceDuration"></param>
        /// <param name="glitchFilterCycleCount"></param>
        public SpstSwitch(
            IDigitalInputController device, 
            IPin pin, 
            InterruptMode interruptMode, 
            ResistorMode resistorMode, 
            TimeSpan debounceDuration, 
            TimeSpan glitchFilterCycleCount) 
            : this (
                device.CreateDigitalInputPort(pin, interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount))
        {
            ShouldDisposePort = true;
        }

        /// <summary>
        /// Creates a SpstSwitch on a especified interrupt port
        /// </summary>
        /// <param name="interruptPort"></param>
        public SpstSwitch(IDigitalInputPort interruptPort)
        {
            DigitalIn = interruptPort;
            DigitalIn.Changed += DigitalInChanged;
        }

        /// <summary>
        /// Event handler when switch value has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DigitalInChanged(object sender, DigitalPortResult e)
        {
            IsOn = DigitalIn.State;
        }

        /// <summary>
        /// Convenience method to get the current sensor reading
        /// </summary>
        public Task<bool> Read() => Task.FromResult(IsOn);

        /// <summary>
        /// Dispose peripheral
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && ShouldDisposePort)
                {
                    DigitalIn.Dispose();
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