using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System.Threading;

namespace Meadow.Foundation.Displays.Lcd
{
    /// <summary>
    /// Represents a character display using I2C
    /// </summary>
    public partial class I2cCharacterDisplay : ICharacterDisplay
    {
        /// <summary>
        /// The I2C peripheral used to communicate with the display
        /// </summary>
        protected readonly II2cPeripheral i2cPeripheral;

        protected byte displayControl;
        protected byte displayMode;
        private byte backlightValue;

        /// <summary>
        /// The cursor current line 
        /// </summary>
        protected byte cursorLine = 0;

        /// <summary>
        /// The cursor current column
        /// </summary>
        protected byte cursorColumn = 0;

        /// <summary>
        /// I2C commands
        /// </summary>
        protected enum I2CCommands
        {
            /// <summary>
            /// Clear the display
            /// </summary>
            LCD_CLEARDISPLAY = 0x01,
            /// <summary>
            /// Send the cursor home
            /// </summary>
            LCD_RETURNHOME = 0x02,
            /// <summary>
            /// Set entry mode
            /// </summary>
            LCD_ENTRYMODESET = 0x04,
            /// <summary>
            /// Display control
            /// </summary>
            LCD_DISPLAYCONTROL = 0x08,
            /// <summary>
            /// Cursor shift
            /// </summary>
            LCD_CURSORSHIFT = 0x10,
            /// <summary>
            /// Function set
            /// </summary>
            LCD_FUNCTIONSET = 0x20,
            /// <summary>
            /// Set CGRAM address
            /// </summary>
            LCD_SETCGRAMADDR = 0x40,
            /// <summary>
            /// Set DDRAM address
            /// </summary>
            LCD_SETDDRAMADDR = 0x80,
        }
  
        // flags for display entry mode
        protected static byte LCD_ENTRYRIGHT = 0x00;
        protected static byte LCD_ENTRYLEFT = 0x02;
        protected static byte LCD_ENTRYSHIFTINCREMENT = 0x01;
        protected static byte LCD_ENTRYSHIFTDECREMENT = 0x00;
        // flags for display on/off control
        protected static byte LCD_DISPLAYON = 0x04;
        protected static byte LCD_DISPLAYOFF = 0x00;
        protected static byte LCD_CURSORON = 0x02;
        protected static byte LCD_CURSOROFF = 0x00;
        protected static byte LCD_BLINKON = 0x01;
        protected static byte LCD_BLINKOFF = 0x00;
        // flags for display/cursor shift
        protected static byte LCD_DISPLAYMOVE = 0x08;
        protected static byte LCD_CURSORMOVE = 0x00;
        protected static byte LCD_MOVERIGHT = 0x04;
        protected static byte LCD_MOVELEFT = 0x00;
        // flags for function set
        protected static byte LCD_8BITMODE = 0x10;
        protected static byte LCD_4BITMODE = 0x00;
        protected static byte LCD_2LINE = 0x08;
        protected static byte LCD_1LINE = 0x00;
        protected static byte LCD_5x10DOTS = 0x04;
        protected static byte LCD_5x8DOTS = 0x00;
        // flags for backlight control
        protected static byte LCD_BACKLIGHT = 0x08;
        protected static byte LCD_NOBACKLIGHT = 0x00;
        protected static byte En = 0b00000100;  // Enable bit
        protected static byte Rw = 0b00000010;  // Read/Write bit
        protected static byte Rs = 0b00000001;  // Register select bit

        /// <summary>
        /// The text display configuration
        /// </summary>
        public TextDisplayConfig DisplayConfig { get; protected set; }

        public I2cCharacterDisplay(II2cBus i2cBus, byte address = (byte)Addresses.Default, byte rows = 4, byte columns = 20)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);
            DisplayConfig = new TextDisplayConfig() { Width = columns, Height = rows };

            backlightValue = LCD_BACKLIGHT;

            Initialize();
        }

        protected virtual void Initialize()
        {
            var displayFunction = (byte)(LCD_4BITMODE | LCD_1LINE | LCD_5x8DOTS);

            if(DisplayConfig.Height > 1)
            {
                displayFunction |= LCD_2LINE;
            }

            Thread.Sleep(50);

            ExpanderWrite(backlightValue);

            Thread.Sleep(1000);

            Write4Bits(0x03 >> 4);
            Thread.Sleep(5);

            Write4Bits(0x03 >> 4);
            Thread.Sleep(5);

            Write4Bits(0x03 >> 4);
            Thread.Sleep(1);

            Write4Bits(0x02 << 4);

            Command((byte)((byte)I2CCommands.LCD_FUNCTIONSET | displayFunction));

            // turn the display on with no cursor or blinking default
            displayControl = (byte)(LCD_DISPLAYON | LCD_CURSOROFF | LCD_BLINKOFF);
            DisplayOn();

            // clear it off
            ClearLines();

            // Initialize to default text direction (for roman languages)
            displayMode = (byte)(LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT);

            // set the entry mode
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));

            Home();
        }

        void Home()
        {
            Command((byte)I2CCommands.LCD_RETURNHOME);  // set cursor position to zero
            Thread.Sleep(2); 
        }

        /// <summary>
        /// Send a command to the display
        /// </summary>
        /// <param name="value">The command as a byte</param>
        protected virtual void Command(byte value)
        {
            Send(value, 0);
        }

        void Write4Bits(byte value)
        {
            ExpanderWrite(value);
            PulseEnable(value);
        }

        void ExpanderWrite(byte value)
        {
            i2cPeripheral.Write((byte)(value | backlightValue));
        }

        /// <summary>
        /// Send data to the display
        /// </summary>
        /// <param name="value">The data to send</param>
        /// <param name="mode">The mode</param>
        protected virtual void Send(byte value, byte mode)
        {
            byte highnib = (byte)(value & 0xf0);
            byte lownib = (byte)((value << 4) & 0xf0);
            Write4Bits((byte)((highnib) | mode));
            Write4Bits((byte)((lownib) | mode));
        }

        void PulseEnable(byte value)
        {
            ExpanderWrite((byte)(value | En));  // En high
            Thread.Sleep(1);

            ExpanderWrite((byte)(value & ~En)); // En low
            Thread.Sleep(1);	// commands need > 37us to settle
        }

        public void ClearLine(byte lineNumber)
        {
            SetCursorPosition(0, lineNumber);
            for (int i = 0; i < DisplayConfig.Width; i++)
            {
                Write(" ");
            }
            SetCursorPosition(0, lineNumber);
        }

        public void ClearLines()
        {
            // clear display, set cursor position to zero
            Command((byte)I2CCommands.LCD_CLEARDISPLAY);
            Thread.Sleep(2);
            SetCursorPosition(0, 0);
        }

        public void SetCursorPosition(byte column, byte line)
        {
            int[] rowOffsets = { 0x00, 0x40, 0x14, 0x54 };

            if (line > DisplayConfig.Height)
            {
                line = (byte)(DisplayConfig.Height - 1);    
            }
            Command((byte)((byte)I2CCommands.LCD_SETDDRAMADDR | (column + rowOffsets[line])));

            cursorLine = line;
            cursorColumn = column;
        }

        public void Write(string text)
        {
            string screentText = text;

            if (screentText.Length + cursorColumn > DisplayConfig.Width)
            {
                screentText = screentText[..(DisplayConfig.Width - cursorColumn)];
            }
            cursorColumn += (byte)screentText.Length;

            var bytes = System.Text.Encoding.UTF8.GetBytes(screentText);

            foreach (var b in bytes)
            {
                Send(b, 1);
            }
        }

        /// <summary>
        /// Write text to a line on the display
        /// </summary>
        /// <param name="text">The text to show</param>
        /// <param name="lineNumber">The line number (0 index)</param>
        /// <param name="showCursor">Show the cursor if true</param>
        public void WriteLine(string text, byte lineNumber, bool showCursor = false)
        {
            SetCursorPosition(0, lineNumber);
            var screenText = text.PadRight(DisplayConfig.Width, ' ');
            Write(screenText);
        }

        /// <summary>
        /// Set the display off
        /// </summary>
        public virtual void DisplayOff()
        {
            displayControl &= (byte)~LCD_DISPLAYON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        /// <summary>
        /// Set the display on
        /// </summary>
        public virtual void DisplayOn()
        {
            displayControl |= LCD_DISPLAYON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        /// <summary>
        /// Hide the cursor
        /// </summary>
        public void CursorOff()
        {
            displayControl &= (byte)~LCD_CURSORON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        /// <summary>
        /// Show the cursor 
        /// </summary>
        public void CursorOn()
        {
            displayControl |= LCD_CURSORON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        /// <summary>
        /// Enable or disable cursor blinking
        /// </summary>
        /// <param name="blink">Blink if true</param>
        public void BlinkCursor(bool blink)
        {
            if (blink)
            {
                displayControl &= (byte)~LCD_BLINKON;
                Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
            }
            else
            {
                displayControl |= LCD_BLINKON;
                Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
            }
        }

        /// <summary>
        /// Scroll the display left
        /// </summary>
        public virtual void ScrollDisplayLeft()
        {
            Command((byte)((byte)I2CCommands.LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVELEFT));
        }

        /// <summary>
        /// Scroll the display right
        /// </summary>
        public virtual void ScrollDisplayRight()
        {
            Command((byte)((byte)I2CCommands.LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVERIGHT));
        }

        /// <summary>
        /// Set the display to show text left to right
        /// </summary>
        public void SetLeftToRight()
        {
            displayMode |= LCD_ENTRYLEFT;
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
        }

        /// <summary>
        /// Set the display to show text right to left
        /// </summary>
        public void SetRightToLeft()
        {
            displayMode &= (byte)~LCD_ENTRYLEFT;
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
        }

        /// <summary>
        /// Enable or disable auto scroll if the text length exeeds the display width
        /// </summary>
        /// <param name="scroll">Auto scroll if true</param>
        public void Autoscroll(bool scroll)
        {
            if (scroll)
            {
                displayMode |= LCD_ENTRYSHIFTINCREMENT;
                Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
            }
            else
            {
                displayMode &= (byte)~LCD_ENTRYSHIFTINCREMENT;
                Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
            }
        }

        /// <summary>
        /// Save a custom character to the dislay
        /// </summary>
        /// <param name="characterMap">The character data</param>
        /// <param name="address">The chracter address</param>
        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
            address &= 0x7; // we only have 8 locations 0-7
            Command((byte)((byte)I2CCommands.LCD_SETCGRAMADDR | (address << 3)));
            for (int i = 0; i < 8; i++)
            {
                i2cPeripheral.WriteRegister(Rs, characterMap[i]);
            }
        }

        // Turn the (optional) backlight off/on
        public void BacklightOn()
        {
            backlightValue = LCD_BACKLIGHT;
            ExpanderWrite(0);
        }

        public void BacklightOff()
        {
            backlightValue = LCD_NOBACKLIGHT;
            ExpanderWrite(0);
        }
        public bool IsBacklightOn()
        {
            return backlightValue == LCD_BACKLIGHT;
        }

        public void Show()
        {
            //can safely ignore
            //required for ITextDisplayMenu
        }
    }
}