using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System;
using System.Threading;

namespace Meadow.Foundation.Displays.Lcd
{
    /// <summary>
    /// Represents a GPIO character display
    /// </summary>
    public class GpioCharacterDisplay : ICharacterDisplay
    {
        private readonly byte LCD_LINE_1 = 0x80; // # LCD RAM address for the 1st line
        private readonly byte LCD_LINE_2 = 0xC0; // # LCD RAM address for the 2nd line
        private readonly byte LCD_LINE_3 = 0x94; // # LCD RAM address for the 3rd line
        private readonly byte LCD_LINE_4 = 0xD4; // # LCD RAM address for the 4th line

        private byte cursorLine = 0;
        private byte cursorColumn = 0;

        private const byte LCD_SETDDRAMADDR = 0x80;
        private const byte LCD_SETCGRAMADDR = 0x40;

        readonly IPwmPort LCD_V0;
        readonly IDigitalOutputPort LCD_E;
        readonly IDigitalOutputPort LCD_RS;
        readonly IDigitalOutputPort LCD_D4;
        readonly IDigitalOutputPort LCD_D5;
        readonly IDigitalOutputPort LCD_D6;
        readonly IDigitalOutputPort LCD_D7;

        readonly bool LCD_INSTRUCTION = false;
        readonly bool LCD_DATA = true;
        static readonly object _lock = new object();

        /// <summary>
        /// The text display menu configuration
        /// </summary>
        public TextDisplayConfig DisplayConfig { get; protected set; }

        /// <summary>
        /// Create a new GpioCharacterDisplay
        /// </summary>
        /// <param name="pinRS">The RS pin</param>
        /// <param name="pinE">The E pin</param>
        /// <param name="pinD4">The D4 pin</param>
        /// <param name="pinD5">The D5 pin</param>
        /// <param name="pinD6">The D6 pin</param>
        /// <param name="pinD7">The D7 pin</param>
        /// <param name="rows">The number of character rows</param>
        /// <param name="columns">The number of character columns</param>
        public GpioCharacterDisplay(
            IPin pinRS,
            IPin pinE,
            IPin pinD4,
            IPin pinD5,
            IPin pinD6,
            IPin pinD7,
            byte rows = 4, byte columns = 20) :
            this(
                pinRS.CreateDigitalOutputPort(),
                pinE.CreateDigitalOutputPort(),
                pinD4.CreateDigitalOutputPort(),
                pinD5.CreateDigitalOutputPort(),
                pinD6.CreateDigitalOutputPort(),
                pinD7.CreateDigitalOutputPort(),
                rows, columns)
        { }

        /// <summary>
        /// Create a new GpioCharacterDisplay object
        /// </summary>
        /// <param name="portRS">Port for RS pin</param>
        /// <param name="portE">Port for W pin</param>
        /// <param name="portD4">Port for D4 pin</param>
        /// <param name="portD5">Port for D5 pin</param>
        /// <param name="portD6">Port for D6 pin</param>
        /// <param name="portD7">Port for D7 pin</param>
        /// <param name="rows">Number of character rows</param>
        /// <param name="columns">Number of character columns</param>
        public GpioCharacterDisplay(
            IDigitalOutputPort portRS,
            IDigitalOutputPort portE,
            IDigitalOutputPort portD4,
            IDigitalOutputPort portD5,
            IDigitalOutputPort portD6,
            IDigitalOutputPort portD7,
            byte rows = 4, byte columns = 20)
        {
            DisplayConfig = new TextDisplayConfig { Height = rows, Width = columns };

            LCD_RS = portRS;
            LCD_E = portE;
            LCD_D4 = portD4;
            LCD_D5 = portD5;
            LCD_D6 = portD6;
            LCD_D7 = portD7;

            Initialize();
        }

        /// <summary>
        /// Create a new GpioCharacterDisplay object
        /// </summary>
        /// <param name="pinV0">V0 pin</param>
        /// <param name="pinRS">RS pin</param>
        /// <param name="pinE">W pin</param>
        /// <param name="pinD4">D4 pin</param>
        /// <param name="pinD5">D5 pin</param>
        /// <param name="pinD6">D6 pin</param>
        /// <param name="pinD7">D7 pin</param>
        /// <param name="rows">Number of character rows</param>
        /// <param name="columns">Number of character columns</param>
        public GpioCharacterDisplay(
            IPin pinV0,
            IPin pinRS,
            IPin pinE,
            IPin pinD4,
            IPin pinD5,
            IPin pinD6,
            IPin pinD7,
            byte rows = 4, byte columns = 20) :
            this(
                pinV0.CreatePwmPort(new Units.Frequency(IPwmOutputController.DefaultPwmDutyCycle, Units.Frequency.UnitType.Hertz), 0.5f, true),
                pinRS.CreateDigitalOutputPort(),
                pinE.CreateDigitalOutputPort(),
                pinD4.CreateDigitalOutputPort(),
                pinD5.CreateDigitalOutputPort(),
                pinD6.CreateDigitalOutputPort(),
                pinD7.CreateDigitalOutputPort(),
                rows, columns)
        { }

        /// <summary>
        /// Create a new GpioCharacterDisplay object
        /// </summary>
        /// <param name="portV0">PWM port for backlight</param>
        /// <param name="portRS">Port for RS pin</param>
        /// <param name="portE">Port for W pin</param>
        /// <param name="portD4">Port for D4 pin</param>
        /// <param name="portD5">Port for D5 pin</param>
        /// <param name="portD6">Port for D6 pin</param>
        /// <param name="portD7">Port for D7 pin</param>
        /// <param name="rows">Number of character rows</param>
        /// <param name="columns">Number of character columns</param>
        public GpioCharacterDisplay(
            IPwmPort portV0,
            IDigitalOutputPort portRS,
            IDigitalOutputPort portE,
            IDigitalOutputPort portD4,
            IDigitalOutputPort portD5,
            IDigitalOutputPort portD6,
            IDigitalOutputPort portD7,
            byte rows = 4, byte columns = 20)
        {
            DisplayConfig = new TextDisplayConfig { Height = rows, Width = columns };

            LCD_V0 = portV0; LCD_V0.Start();
            LCD_RS = portRS;
            LCD_E = portE;
            LCD_D4 = portD4;
            LCD_D5 = portD5;
            LCD_D6 = portD6;
            LCD_D7 = portD7;

            Initialize();
        }

        private void Initialize()
        {
            SendByte(0x33, LCD_INSTRUCTION); // 110011 Initialize
            SendByte(0x32, LCD_INSTRUCTION); // 110010 Initialize
            SendByte(0x06, LCD_INSTRUCTION); // 000110 Cursor move direction
            SendByte(0x0C, LCD_INSTRUCTION); // 001100 Display On,Cursor Off, Blink Off
            SendByte(0x28, LCD_INSTRUCTION); // 101000 Data length, number of lines, font size
            SendByte(0x01, LCD_INSTRUCTION); // 000001 Clear display
            Thread.Sleep(5);
        }

        private void SendByte(byte value, bool mode)
        {
            lock (_lock)
            {
                LCD_RS.State = (mode);

                // high bits
                LCD_D4.State = ((value & 0x10) == 0x10);
                LCD_D5.State = ((value & 0x20) == 0x20);
                LCD_D6.State = ((value & 0x40) == 0x40);
                LCD_D7.State = ((value & 0x80) == 0x80);

                ToggleEnable();

                // low bits
                LCD_D4.State = ((value & 0x01) == 0x01);
                LCD_D5.State = ((value & 0x02) == 0x02);
                LCD_D6.State = ((value & 0x04) == 0x04);
                LCD_D7.State = ((value & 0x08) == 0x08);

                ToggleEnable();

                Thread.Sleep(5);
            }
        }

        private void ToggleEnable()
        {
            LCD_E.State = false;
            LCD_E.State = true;
            LCD_E.State = false;
        }

        private byte GetLineAddress(int line)
        {
            switch (line)
            {
                case 0:
                    return LCD_LINE_1;
                case 1:
                    return LCD_LINE_2;
                case 2:
                    return LCD_LINE_3;
                case 3:
                    return LCD_LINE_4;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void SetLineAddress(int line)
        {
            SendByte(GetLineAddress(line), LCD_INSTRUCTION);
        }

        /// <summary>
        /// Write text to a line
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="lineNumber">The target line</param>
        /// <param name="showCursor">If true, show the cursor</param>
        public void WriteLine(string text, byte lineNumber, bool showCursor = false)
        {
            SetLineAddress(lineNumber);

            // Instead of clearing the line first, pad it with empty space on the end
            var screenText = text.PadRight(DisplayConfig.Width, ' ');
            if (screenText.Length > DisplayConfig.Width)
            {
                screenText = screenText.Substring(0, DisplayConfig.Width);
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(screenText);
            foreach (var b in bytes)
            {
                SendByte(b, LCD_DATA);
            }
        }

        /// <summary>
        /// Write a string to the display
        /// </summary>
        /// <param name="text">The text to show as a string</param>
        public void Write(string text)
        {
            string screentText = text;

            if (screentText.Length + cursorColumn > DisplayConfig.Width)
            {
                screentText = screentText.Substring(0, DisplayConfig.Width - cursorColumn);
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(screentText);
            foreach (var b in bytes)
            {
                SendByte(b, LCD_DATA);
            }
        }

        /// <summary>
        /// Set the cursor position
        /// </summary>
        /// <param name="column">The cursor column</param>
        /// <param name="line">The cursor line</param>
        public void SetCursorPosition(byte column, byte line)
        {
            if (column >= DisplayConfig.Width || line >= DisplayConfig.Height)
            {
                throw new Exception($"CharacterDisplay: cursor out of bounds {column}, {line}");
            }

            cursorColumn = column;
            cursorLine = line;

            byte lineAddress = GetLineAddress(line);
            var address = column + lineAddress;
            SendByte(((byte)(LCD_SETDDRAMADDR | address)), LCD_INSTRUCTION);
        }

        /// <summary>
        /// Clear all lines
        /// </summary>
        public void ClearLines()
        {
            SendByte(0x01, LCD_INSTRUCTION);
            SetCursorPosition(1, 0);
            Thread.Sleep(5);
        }

        /// <summary>
        /// Clear a line of text
        /// </summary>
        /// <param name="lineNumber">The line to clear (0 indexed)</param>
        public void ClearLine(byte lineNumber)
        {
            SetLineAddress(lineNumber);

            for (int i = 0; i < DisplayConfig.Width; i++)
            {
                Write(" ");
            }
        }


        /// <summary>
        /// Set the display contrast
        /// </summary>
        /// <param name="contrast">The contrast as a float (0-1)</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetContrast(float contrast = 0.5f)
        {
            if (contrast < 0 || contrast > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(contrast), "err: contrast must be between 0 and 1, inclusive.");
            }

            Resolver.Log.Info($"Contrast: {contrast}");
            LCD_V0.DutyCycle = contrast;
        }

        /// <summary>
        /// Save a custom character to the display
        /// </summary>
        /// <param name="characterMap">The character data</param>
        /// <param name="address">The display character address (0-7)</param>
        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
            address &= 0x7; // we only have 8 locations 0-7
            SendByte((byte)(LCD_SETCGRAMADDR | (address << 3)), LCD_INSTRUCTION);

            for (var i = 0; i < 8; i++)
            {
                SendByte(characterMap[i], LCD_DATA);
            }
        }

        /// <summary>
        /// Update the display
        /// </summary>
        public void Show()
        {   //can safely ignore
        }   //required for ITextDisplayMenu
    }
}