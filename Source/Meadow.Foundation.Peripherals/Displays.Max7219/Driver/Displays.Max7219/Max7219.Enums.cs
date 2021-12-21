namespace Meadow.Foundation.Displays
{
    public partial class Max7219
    {
        public enum Max7219Type
        {
            Character,
            Digital,
            Display,
        }

        public enum CharacterType : byte
        {
            _0 = 0x00,
            _1 = 0x01,
            _2 = 0x02,
            _3 = 0x03,
            _4 = 0x04,
            _5 = 0x05,
            _6 = 0x06,
            _7 = 0x07,
            _8 = 0x08,
            _9 = 0x09,
            Hyphen = 0x0A,
            E = 0x0B,
            H = 0x0C,
            L = 0x0D,
            P = 0x0E,
            Blank = 0x0F,
            count
        }

        internal enum Register : byte
        {
            NoOp = 0x00,
            Digit0 = 0x01,
            Digit1 = 0x02,
            Digit2 = 0x03,
            Digit3 = 0x04,
            Digit4 = 0x05,
            Digit5 = 0x06,
            Digit6 = 0x07,
            Digit7 = 0x08,
            DecodeMode = 0x09,
            Intensity = 0x0A,
            ScanLimit = 0x0B,
            ShutDown = 0x0C,
            DisplayTest = 0x0F
        }
    }
}
