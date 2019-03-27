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
            protected set => Changed(this, new EventArgs());
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
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private SpdtSwitch() { }

        /// <summary>
        /// Instantiates a new SpdtSwitch object with the center pin connected to the specified digital pin, one pin connected to common/ground and one pin connected to high/3.3V.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="pin"></param>
        /// <param name="interruptMode"></param>
        /// <param name="resistorMode"></param>
        /// <param name="debounceDuration"></param>
        /// <param name="glitchFilterCycleCount"></param>
        public SpdtSwitch(IIODevice device, IPin pin, InterruptMode interruptMode, ResistorMode resistorMode, int debounceDuration = 20, int glitchFilterCycleCount = 0) :
            this (device.CreateDigitalInputPort(pin, interruptMode, resistorMode, debounceDuration, glitchFilterCycleCount)) {}

        /// <summary>
        /// Instantiates a new SpdtSwitch object with the center pin connected to the specified digital pin, one pin connected to common/ground and one pin connected to high/3.3V.
        /// </summary>
        /// <param name="interruptPort"></param>
        public SpdtSwitch(IDigitalInputPort interruptPort)
        {
            DigitalIn = interruptPort;
            DigitalIn.Changed += DigitalInChanged;
        }

        void DigitalInChanged(object sender, DigitalInputPortEventArgs e)
        {
            IsOn = DigitalIn.State;
        }
    }
}