using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// The KeyScanEventArgs class
    /// </summary>
    public class KeyScanEventArgs : EventArgs
    {
        /// <summary>
        /// The button for the event args
        /// </summary>
        public As1115.KeyScanButtonType Button { get; protected set; }

        /// <summary>
        /// KeyA value
        /// </summary>
        public byte KeyA { get; protected set; }

        /// <summary>
        /// KeyB value
        /// </summary>
        public byte KeyB { get; protected set; }

        /// <summary>
        /// Create a new KeyScanEventArgs object
        /// </summary>
        /// <param name="button">The button</param>
        /// <param name="keyA">KeyA value</param>
        /// <param name="keyB">KeyB value</param>
        public KeyScanEventArgs(As1115.KeyScanButtonType button, byte keyA, byte keyB)
        {
            Button = button;
            KeyA = keyA;
            KeyB = keyB;
        }
    }
}