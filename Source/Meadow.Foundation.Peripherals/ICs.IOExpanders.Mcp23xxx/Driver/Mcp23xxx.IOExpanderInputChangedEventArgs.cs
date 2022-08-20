using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class IOExpanderInputChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Which pins were interrupted
        /// </summary>
        public byte InterruptPins { get; }

        /// <summary>
        /// The values of pins that were interrupted
        /// </summary>
        public byte InputState { get; }
    
        public IOExpanderInputChangedEventArgs(byte interruptPins, byte inputState)
        {
            InterruptPins = interruptPins;
            InputState = inputState;
        }
    }
}