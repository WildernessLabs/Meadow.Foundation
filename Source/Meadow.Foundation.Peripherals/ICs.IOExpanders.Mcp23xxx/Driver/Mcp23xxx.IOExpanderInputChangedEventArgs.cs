using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// IOExpanderInputChangedEventArgs class
    /// </summary>
    public class IOExpanderInputChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Which pins were interrupted
        /// </summary>
        public byte InterruptPins { get; }

        /// <summary>
        /// The values of pins that were interrupted
        /// </summary>
        public ushort InputState { get; }

        /// <summary>
        /// Create a new IOExpanderInputChangedEventArgs object
        /// </summary>
        /// <param name="interruptPins">The interrupt pins</param>
        /// <param name="inputState">The input state</param>
        public IOExpanderInputChangedEventArgs(byte interruptPins, ushort inputState)
        {
            InterruptPins = interruptPins;
            InputState = inputState;
        }
    }
}