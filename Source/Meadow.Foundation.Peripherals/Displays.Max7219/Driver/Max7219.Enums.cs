namespace Meadow.Foundation.Displays
{
    public partial class Max7219
    {
        /// <summary>
        /// Device mode
        /// </summary>
        public enum Max7219Mode
        {
            /// <summary>
            /// Use characters
            /// </summary>
            Character,
            /// <summary>
            /// Use digital values
            /// </summary>
            Digital,
            /// <summary>
            /// Use pixel coordinates
            /// </summary>
            Display,
        }

        /// <summary>
        /// Character type
        /// </summary>
        public enum CharacterType : byte
        {
            /// <summary>
            /// Zero (0)
            /// </summary>
            _0 = 0x00,
            /// <summary>
            /// One (1) 
            /// </summary>
            _1 = 0x01,
            /// <summary>
            /// Two (2)
            /// </summary>
            _2 = 0x02,
            /// <summary>
            /// Three (3)
            /// </summary>
            _3 = 0x03,
            /// <summary>
            /// Four (4)
            /// </summary>
            _4 = 0x04,
            /// <summary>
            /// Five (5)
            /// </summary>
            _5 = 0x05,
            /// <summary>
            /// Six (6)
            /// </summary>
            _6 = 0x06,
            /// <summary>
            /// Seven (7)
            /// </summary>
            _7 = 0x07,
            /// <summary>
            /// Eight (8)
            /// </summary>
            _8 = 0x08,
            /// <summary>
            /// Nine (9)
            /// </summary>
            _9 = 0x09,
            /// <summary>
            /// Hyphen (-)
            /// </summary>
            Hyphen = 0x0A,
            /// <summary>
            /// E
            /// </summary>
            E = 0x0B,
            /// <summary>
            /// H
            /// </summary>
            H = 0x0C,
            /// <summary>
            /// L
            /// </summary>
            L = 0x0D,
            /// <summary>
            /// P
            /// </summary>
            P = 0x0E,
            /// <summary>
            /// Space ( )
            /// </summary>
            Blank = 0x0F,
            /// <summary>
            /// Count of characters
            /// </summary>
            Count = 16
        }

        /// <summary>
        /// Max7219 registers
        /// </summary>
        internal enum Register : byte
        {
            /// <summary>
            /// No operation register
            /// </summary>
            NoOp = 0x00,
            /// <summary>
            /// Digit 0 register
            /// </summary>
            Digit0 = 0x01,
            /// <summary>
            /// Digit 1 register
            /// </summary>
            Digit1 = 0x02,
            /// <summary>
            /// Digit 2 register
            /// </summary>
            Digit2 = 0x03,
            /// <summary>
            /// Digit 3 register
            /// </summary>
            Digit3 = 0x04,
            /// <summary>
            /// Digit 4 register
            /// </summary>
            Digit4 = 0x05,
            /// <summary>
            /// Digit 5 register
            /// </summary>
            Digit5 = 0x06,
            /// <summary>
            /// Digit 6 register
            /// </summary>
            Digit6 = 0x07,
            /// <summary>
            /// Digit 7 register
            /// </summary>
            Digit7 = 0x08,
            /// <summary>
            /// Decode mode register
            /// </summary>
            DecodeMode = 0x09,
            /// <summary>
            /// Intensity register
            /// </summary>
            Intensity = 0x0A,
            /// <summary>
            /// Scan limit register
            /// </summary>
            ScanLimit = 0x0B,
            /// <summary>
            /// Shut down register
            /// </summary>
            ShutDown = 0x0C,
            /// <summary>
            /// Display test register
            /// </summary>
            DisplayTest = 0x0F
        }
    }
}