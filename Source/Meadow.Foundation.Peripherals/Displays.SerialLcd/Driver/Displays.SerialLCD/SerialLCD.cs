using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays
{
    // TODO:
    // * SelectLine (uchar lineNumber)
    // * WriteMarquee (string text) // or similar
    // * SaveCustomCharacter (character)


    /// <summary>
    ///     Encapsulate the functionality required to control the Sparkfun serial LCD display.
    /// </summary>
    public class SerialLCD : ITextDisplay
    {
        #region Properties

        /// <summary>
        ///     Display configuration (width and height).
        /// </summary>
        public TextDisplayConfig DisplayConfig { get; private set; }

        #endregion

        #region Enums

        /// <summary>
        ///     Describe the cursor style to be displayed.
        /// </summary>
        public enum CursorStyle
        {
            UnderlineOn = 0x0e,
            UnderlineOff = 0x0c,
            BlinkingBoxOn = 0x0d,
            BlinkingBoxOff = 0x0c
        }

        /// <summary>
        ///     Display power state.
        /// </summary>
        public enum DisplayPowerState
        {
            On = 0x0c,
            Off = 0x08
        }

        /// <summary>
        ///     Describe the number of lines and characters on the display.
        /// </summary>
        public enum LCDDimensions
        {
            Characters20Wide = 0x03,
            Characters16Wide = 0x04,
            Lines4 = 0x05,
            Lines2 = 0x06
        }

        /// <summary>
        ///     Possible baud rates for the display.
        /// </summary>
        public enum LCDBaudRate
        {
            Baud2400 = 11,
            Baud4800,
            Baud9600,
            Baud14400,
            Baud19200,
            Baud38400
        }

        /// <summary>
        ///     Direction to move the cursor or the display.
        /// </summary>
        public enum Direction
        {
            Left,
            Right
        }

        #endregion Enums

        #region Constants

        /// <summary>
        ///     Byte used to prefix the extended PCD display commands.
        /// </summary>
        private const byte ExtendedCommandCharacter = 0xfe;

        /// <summary>
        ///     Byte used to prefix the interface commands.
        /// </summary>
        private const byte ConfigurationCommandCharacter = 0x7c;

        #endregion Constants

        #region Member variables / fields

        /// <summary>
        ///     Comp port being used to communicate with the display.
        /// </summary>
        private readonly ISerialPort comPort;

        /// <summary>
        ///     object for using lock() to do thread synch
        /// </summary>
        protected object _lock = new object();

        #endregion Member variable / fields

        #region Constructors

        /// <summary>
        ///     Make the default constructor private to prevent it being called.
        /// </summary>
        private SerialLCD()
        {
        }

        /// <summary>
        ///     Create a new SerialLCD object.
        /// </summary>
        /// <param name="config">TextDisplayConfig object defining the LCD dimension (null will default to 16x2).</param>
        /// <param name="baudRate">Baud rate to use (default = 9600).</param>
        /// <param name="parity">Parity to use (default is None).</param>
        /// <param name="dataBits">Number of data bits (default is 8 data bits).</param>
        /// <param name="stopBits">Number of stop bits (default is one stop bit).</param>
        public SerialLCD(IIODevice device, SerialPortName port, TextDisplayConfig config = null, int baudRate = 9600,
            Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            if (config == null)
            {
                // assume a 16x2 LCD.
                DisplayConfig = new TextDisplayConfig() { Height = 2, Width = 16 };
            } 
            else
            {
                DisplayConfig = config;
            }

            comPort = device.CreateSerialPort(port, baudRate, dataBits, parity, stopBits);

            comPort.Open();

            // configure the LCD controller for the appropriate screen size
            byte lines = 0;
            byte characters = 0;
            switch (DisplayConfig.Width)
            {
                case 16:
                    characters = (byte)LCDDimensions.Characters16Wide;
                    break;
                case 20:
                    characters = (byte)LCDDimensions.Characters20Wide;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(config.Width), "Display width should be 16 or 20.");
            }
            switch (DisplayConfig.Height)
            {
                case 2:
                    lines = (byte)LCDDimensions.Lines2;
                    break;
                case 4:
                    lines = (byte)LCDDimensions.Lines4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(config.Height), "Display height should be 2 or 4 lines.");
            }
            Send(new[] { ConfigurationCommandCharacter, characters, ConfigurationCommandCharacter, lines });
            Thread.Sleep(10);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Write the buffer of data to the COM port (i.e. the display).
        /// </summary>
        /// <param name="buffer">Bytes of data to be sent to the display.</param>
        private void Send(byte[] buffer)
        {
            // critical section so we don't have mixed messages
            lock (_lock)
            {
                comPort.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        ///     Toggle the splash screen.
        /// </summary>
        public void ToggleSplashScreen()
        {
            Send(new byte[] { ConfigurationCommandCharacter, 9 });
        }

        /// <summary>
        ///     Set up the splash screen.
        /// </summary>
        public void SetSplashScreen(string line1, string line2)
        {
            WriteLine(line1, 0);
            WriteLine(line2, 1);
            Send(new byte[] { ConfigurationCommandCharacter, 10 });
        }

        /// <summary>
        ///     Change the baud rate of the display using the command interface.
        /// </summary>
        public void SetBaudRate(LCDBaudRate baudRate)
        {
            Send(new[] { ConfigurationCommandCharacter, (byte) baudRate });
        }

        /// <summary>
        ///     Clear the display.
        /// </summary>
        public void Clear()
        {
            byte[] buffer = { ExtendedCommandCharacter, 0x01 };
            Send(buffer);
        }

        /// <summary>
        ///     Set the cursor position on the display to the specified column and line.
        /// </summary>
        /// <param name="column">Column on the display to move the cursor to (0-15 or 0-19).</param>
        /// <param name="line">Line on the display to move the cursor to (0-3).</param>
        public void SetCursorPosition(byte column, byte line)
        {
            if (column >= DisplayConfig.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(column), "Column exceeds with width of the display.");
            }
            if (line >= DisplayConfig.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "Line exceeds the height of the display.");
            }
            //
            //  The following calculations are taken from the data on page
            //  3 of the data sheet.
            //
            var absoluteCharacterPosition = column;
            switch (line)
            {
                case 0:
                    break;
                case 1:
                    absoluteCharacterPosition += 64;
                    break;
                case 2:
                    absoluteCharacterPosition += DisplayConfig.Width == 16 ? (byte)16 : (byte)20;
                    break;
                case 3:
                    absoluteCharacterPosition += DisplayConfig.Width == 16 ? (byte)80 : (byte) 84;
                    break;
            }
            Send(new[] { ExtendedCommandCharacter, (byte) (0x80 + (absoluteCharacterPosition & 0xff)) });
        }

        /// <summary>
        ///     Move the cursor either right or left on the display.
        /// </summary>
        /// <param name="direction">Direction to move the cursor, left or right.</param>
        public void MoveCursor(Direction direction)
        {
            if (direction == Direction.Left)
            {
                Send(new byte[] { ExtendedCommandCharacter, 0x10 });
            }
            else
            {
                Send(new byte[] { ExtendedCommandCharacter, 0x14 });
            }
        }

        /// <summary>
        ///     Scroll the contents of the display one character in the specified direction.
        /// </summary>
        /// <param name="direction">Direction to scroll the display, left or right.</param>
        public void ScrollDisplay(Direction direction)
        {
            if (direction == Direction.Left)
            {
                Send(new byte[] { ExtendedCommandCharacter, 0x18 });
            }
            else
            {
                Send(new byte[] { ExtendedCommandCharacter, 0x1c });
            }
        }

        /// <summary>
        ///     Set the cursor style to underline or block.  The cursor can also be blinking or solid.
        /// </summary>
        /// <param name="style">New cursor style (Block/Underline, Blinking/Solid).</param>
        public void SetCursorStyle(CursorStyle style)
        {
            Send(new[] { ExtendedCommandCharacter, (byte) style });
        }

        /// <summary>
        ///     Display the text at the current cursor position.
        /// </summary>
        /// <param name="text">Text to display.</param>
        [Obsolete("Use .Write methods instead.")]
        public void DisplayText(string text)
        {
            Write(text);
        }

        /// <summary>
        ///     Display the text at the current cursor position.
        /// </summary>
        /// <param name="text">Text to display.</param>
        public void Write(string text)
        {
            //
            //  The conversion below is explicit as UTF8 encoding for characters with
            //  an ASCII value > 127 the characters are broken out into two bytes when
            //  only one is required.  This means that some characters cannot be
            //  displayed correctly when UTF8 is used.
            //
            int length = text.Length;
            byte[] bytes = new byte[length];
            for (int index = 0; index < length; index++)
            {
                bytes[index] = (byte) text[index];
            }
            Send(bytes);
        }

        /// <summary>
        ///     Displays the characters at the current cursor position. 
        ///     Unlike the `string text` overload, this assumes the text
        ///     has already been encoded to characters. Can be useful in 
        ///     sending pre-encoded characters, or accessing custom a 
        ///     custom character saved in the 0 slot.
        /// </summary>
        /// <param name="chars"></param>
        public void Write(byte[] chars)
        {
            Send(chars);
        }

        /// <summary>
        ///     Writes the specified text to the specified line.
        /// </summary>
        /// <param name="lineNumber">Line to write the text on (0-3).</param>
        /// <param name="text">Text to display.</param>
        public void WriteLine(string text, byte lineNumber)
        {
            string lineText = text;
            if (text.Length > DisplayConfig.Width)
            {
                Console.WriteLine("Text length exceeds number of columns, truncating.");
                Console.WriteLine("Text length: " + text.Length.ToString() + ", Columns: " + DisplayConfig.Width.ToString());
                //throw new Exception("Number characters must be <= columns");
                lineText = text.Substring(0, DisplayConfig.Width);
                Console.WriteLine("Truncating text to: " + lineText);
            }

            // clear the line
            ClearLine(lineNumber);

            // write the line
            SetCursorPosition(0, lineNumber);
            Write(lineText);
        }

        /// <summary>
        ///     Clears the specified line by writing a string of empty characters
        ///     to it.
        /// </summary>
        /// <param name="lineNumber">Line to clear (0-3)</param>
        public void ClearLine(byte lineNumber)
        {
            // clear the line
            SetCursorPosition(0, lineNumber);
            var clearChars = new char[DisplayConfig.Width];
            for (var i = 0; i < DisplayConfig.Width; i++)
            {
                clearChars[i] = ' ';
            }
            var clearString = new string(clearChars);
            Write(clearString);
        }

        /// <summary>
        ///     Turn the display on or off.
        /// </summary>
        /// <param name="state">New power state for the display.</param>
        public void SetDisplayVisualState(DisplayPowerState state)
        {
            Send(new[] {ExtendedCommandCharacter, (byte) state});
        }

        /// <summary>
        ///     Sets the backlight brightness of the LCD. Valid values
        ///     are 0 through 1. Sleeps for 125milliseconds after setting
        ///     to let the display settle.
        ///     
        ///     0 = Off
        ///     0.5 = 50%
        ///     1 = 100%
        /// </summary>
        /// <param name="brightness"></param>
        public void SetBrightness(float brightness = 0.75f)
        {
            // clamp
            if (brightness < 0) brightness = 0.0F;
            if (brightness > 1) brightness = 1.0F;

            // valid values are 128 - 157, inclusive
            byte bValue = (byte)(128 + brightness * 29); 

            // clamp again
            if (bValue < 128) bValue = 128;
            if (bValue > 157) bValue = 157;

            // set the brightness
            Send(new[] { ConfigurationCommandCharacter, bValue });
            // let the display settle
            Thread.Sleep(125);
        }

        /// <summary>
        /// Saves a custom character to one of 8 slots in the CGRAM. 
        /// Custom characters are defined as charater maps and can 
        /// be created using online graphical tools such as the one
        /// found at:
        /// http://maxpromer.github.io/LCD-Character-Creator/
        /// 
        /// **Note:** due to .Net's underlying string implementation,
        /// slot 0 is unusable unless you decode the string yourself 
        /// and use the `Write(byte[] chars)` method. Otherwise, 
        /// when the string is created, .Net treats the `0` character
        /// as a string terminator. 
        /// 
        /// </summary>
        /// <param name="characterMap">Character map defining the 5x8 character.</param>
        /// <param name="address">0-7</param>
        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
            if (address > 7 || address < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address must be 0 - 7");
            }

            // tell the LCD we want to save a character to an address
            // 0x4 + address
            // 0x40 = save in slot 0, 0x40 + (6 * 8) = save in slot 6

            // note that i bundle this all up into a single command in case two threads are calling Send()
            byte[] command = { ExtendedCommandCharacter, (byte)(0x40 + (address * 8)) };
            byte[] fullCommand = new byte[command.Length + characterMap.Length];
            command.CopyTo(fullCommand, 0);
            characterMap.CopyTo(fullCommand, command.Length);
            Send(fullCommand);

            // simpler, but not threadsafe way:
            //Send(new byte[] { ConfigurationCommandCharacter, (byte)(0x40 + (address * 8)) });
            //Send(characterMap);
        }

        #endregion Methods
    }
}