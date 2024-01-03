namespace Meadow.Foundation.Sensors.Hid
{
    public partial class BBQ10Keyboard
    {
        /// <summary>
        /// Key event struct
        /// </summary>
        public struct KeyEvent
        {
            /// <summary>
            /// Key ASCII value
            /// </summary>
            public char AsciiValue { get; private set; }

            /// <summary>
            /// The key state at the time of the event
            /// </summary>
            public KeyState KeyState { get; private set; }

            /// <summary>
            /// Create a new KeyEvent object
            /// </summary>
            /// <param name="asciiValue">The ASCII value</param>
            /// <param name="keyState">The key state</param>
            public KeyEvent(char asciiValue, KeyState keyState)
            {
                AsciiValue = asciiValue;
                KeyState = keyState;
            }
        }
    }
}