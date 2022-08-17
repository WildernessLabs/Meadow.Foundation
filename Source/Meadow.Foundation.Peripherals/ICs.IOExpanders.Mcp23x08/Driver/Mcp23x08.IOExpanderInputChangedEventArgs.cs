using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class IOExpanderInputChangedEventArgs : EventArgs
    {
        public byte InterruptPins { get; }
        public byte InputState { get; }
    
        public IOExpanderInputChangedEventArgs(byte interruptPins, byte inputState)
        {
            InterruptPins = interruptPins;
            InputState = inputState;
        }
    }
}