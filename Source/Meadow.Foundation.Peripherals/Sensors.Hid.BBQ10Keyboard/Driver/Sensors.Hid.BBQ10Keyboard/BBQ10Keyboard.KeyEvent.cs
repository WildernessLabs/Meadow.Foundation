namespace Meadow.Foundation.Sensors.Hid
{
    public partial class BBQ10Keyboard
    {
        public  struct KeyEvent
        {
            public char AsciiValue { get; private set; }
            public KeyState KeyState { get; private set; }

            public KeyEvent(char asciiValue, KeyState keyState)
            {
                AsciiValue = asciiValue;
                KeyState = keyState;
            }
        }
    }
}
