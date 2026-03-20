namespace Meadow.Foundation.Sensors.Hid;

public partial class BBQ10Keyboard
{
    /// <summary>
    /// Describes a single key event from the BBQ10 keyboard FIFO
    /// </summary>
    public struct KeyEvent
    {
        /// <summary>
        /// ASCII character value of the key
        /// </summary>
        public char AsciiValue { get; private set; }

        /// <summary>
        /// The key state at the time of the event
        /// </summary>
        public KeyState KeyState { get; private set; }

        /// <summary>
        /// Creates a new KeyEvent
        /// </summary>
        /// <param name="asciiValue">The ASCII character value</param>
        /// <param name="keyState">The key state</param>
        public KeyEvent(char asciiValue, KeyState keyState)
        {
            AsciiValue = asciiValue;
            KeyState = keyState;
        }
    }
}
