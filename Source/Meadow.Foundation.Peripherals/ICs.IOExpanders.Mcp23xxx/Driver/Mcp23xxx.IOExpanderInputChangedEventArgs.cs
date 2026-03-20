using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// IOExpanderInputChangedEventArgs class
    /// </summary>
    public class IOExpanderInputChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Which pins were interrupted (bit mask; bits 0-7 = Port A, bits 8-15 = Port B on 16-pin devices)
        /// </summary>
        public ushort InterruptPins { get; }

        /// <summary>
        /// The values of pins that were interrupted
        /// </summary>
        public ushort InputState { get; }

        /// <summary>
        /// Create a new IOExpanderInputChangedEventArgs object
        /// </summary>
        /// <param name="interruptPins">The interrupt pins</param>
        /// <param name="inputState">The input state</param>
        public IOExpanderInputChangedEventArgs(ushort interruptPins, ushort inputState)
        {
            InterruptPins = interruptPins;
            InputState = inputState;
        }
    }
}