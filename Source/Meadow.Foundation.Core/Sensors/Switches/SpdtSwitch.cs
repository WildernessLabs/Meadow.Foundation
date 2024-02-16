﻿using Meadow.Hardware;
using Meadow.Peripherals.Switches;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Switches
{
    /// <summary>
    /// Represents a simple, two position, Single-Pole-Dual-Throw (SPDT) switch that closes a circuit 
    /// to either ground/common or high depending on position  
    /// </summary>
    public class SpdtSwitch : ISwitch, IDisposable
    {
        /// <summary>
        /// Describes whether or not the switch circuit is closed/connected (IsOn = true), or open (IsOn = false).
        /// </summary>
        public bool IsOn
        {
            get => DigitalInputPort.State;
            protected set => Changed(this, new EventArgs());
        }

        /// <summary>
        /// Returns the DigitalInputPort.
        /// </summary>
        protected IDigitalInterruptPort DigitalInputPort { get; set; }

        /// <summary>
        /// Raised when the switch circuit is opened or closed.
        /// </summary>
        public event EventHandler Changed = default!;

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        readonly bool createdPort = false;

        /// <summary>
        /// Instantiates a new SpdtSwitch object with the center pin connected to the specified digital pin, one pin connected to common/ground and one pin connected to high/3.3V.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        public SpdtSwitch(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode) :
            this(pin.CreateDigitalInterruptPort(interruptMode, resistorMode, TimeSpan.FromMilliseconds(20), TimeSpan.Zero))
        {
            createdPort = true;
        }

        /// <summary>
        /// Instantiates a new SpdtSwitch object with the center pin connected to the specified digital pin, one pin connected to common/ground and one pin connected to high/3.3V.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        /// <param name="debounceDuration"></param>
        /// <param name="glitchFilterCycleCount"></param>
        public SpdtSwitch(IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, TimeSpan debounceDuration, TimeSpan glitchFilterCycleCount) :
            this(pin.CreateDigitalInterruptPort(interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount))
        { }

        /// <summary>
        /// Creates a SpdtSwitch on a specified interrupt port
        /// </summary>
        /// <param name="interruptPort"></param>
        public SpdtSwitch(IDigitalInterruptPort interruptPort)
        {
            DigitalInputPort = interruptPort;
            DigitalInputPort.Changed += DigitalInChanged;
        }

        /// <summary>
        /// Event handler when switch value has been changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DigitalInChanged(object sender, DigitalPortResult e)
        {
            IsOn = DigitalInputPort.State;
        }

        /// <summary>
        /// Convenience method to get the current sensor reading
        /// </summary>
        public Task<bool> Read() => Task.FromResult(IsOn);

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    DigitalInputPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}