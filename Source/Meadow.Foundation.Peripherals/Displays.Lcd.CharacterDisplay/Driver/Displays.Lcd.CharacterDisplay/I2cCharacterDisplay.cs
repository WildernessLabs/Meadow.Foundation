using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System.Threading;

namespace Meadow.Foundation.Displays.Lcd
{
    public partial class I2cCharacterDisplay : ICharacterDisplay
    {
        protected II2cPeripheral i2cPeripheral;

        private byte displayControl;
        private byte displayMode;
        private byte backlightValue;

        private byte cursorLine = 0;
        private byte cursorColumn = 0;

        enum I2CCommands
        {
            LCD_CLEARDISPLAY = 0x01,
            LCD_RETURNHOME = 0x02,
            LCD_ENTRYMODESET = 0x04,
            LCD_DISPLAYCONTROL = 0x08,
            LCD_CURSORSHIFT = 0x10,
            LCD_FUNCTIONSET = 0x20,
            LCD_SETCGRAMADDR = 0x40,
            LCD_SETDDRAMADDR = 0x80,
        }
  
        // flags for display entry mode
        static byte LCD_ENTRYRIGHT = 0x00;
        static byte LCD_ENTRYLEFT = 0x02;
        static byte LCD_ENTRYSHIFTINCREMENT = 0x01;
        static byte LCD_ENTRYSHIFTDECREMENT = 0x00;
        // flags for display on/off control
        static byte LCD_DISPLAYON = 0x04;
        static byte LCD_DISPLAYOFF = 0x00;
        static byte LCD_CURSORON = 0x02;
        static byte LCD_CURSOROFF = 0x00;
        static byte LCD_BLINKON = 0x01;
        static byte LCD_BLINKOFF = 0x00;
        // flags for display/cursor shift
        static byte LCD_DISPLAYMOVE = 0x08;
        static byte LCD_CURSORMOVE = 0x00;
        static byte LCD_MOVERIGHT = 0x04;
        static byte LCD_MOVELEFT = 0x00;
        // flags for function set
        static byte LCD_8BITMODE = 0x10;
        static byte LCD_4BITMODE = 0x00;
        static byte LCD_2LINE = 0x08;
        static byte LCD_1LINE = 0x00;
        static byte LCD_5x10DOTS = 0x04;
        static byte LCD_5x8DOTS = 0x00;
        // flags for backlight control
        static byte LCD_BACKLIGHT = 0x08;
        static byte LCD_NOBACKLIGHT = 0x00;
        static byte En = 0b00000100;  // Enable bit
        static byte Rw = 0b00000010;  // Read/Write bit
        static byte Rs = 0b00000001;  // Register select bit

        public TextDisplayConfig DisplayConfig { get; protected set; }

        public I2cCharacterDisplay(II2cBus i2cBus, byte address = (byte)Addresses.Default, byte rows = 4, byte columns = 20)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);
            DisplayConfig = new TextDisplayConfig() { Width = columns, Height = rows };

            backlightValue = LCD_BACKLIGHT;

            Initialize();
        }

        void Initialize()
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

        void Command(byte value)
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

        void Send(byte value, byte mode)
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

        public void WriteLine(string text, byte lineNumber, bool showCursor = false)
        {
            SetCursorPosition(0, lineNumber);
            var screenText = text.PadRight(DisplayConfig.Width, ' ');
            Write(screenText);
        }

        // Turn the display on/off (quickly)
        public void DisplayOff()
        {
            displayControl &= (byte)~LCD_DISPLAYON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }
        public void DisplayOn()
        {
            displayControl |= LCD_DISPLAYON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        // Turns the underline cursor on/off
        public void CursorOff()
        {
            displayControl &= (byte)~LCD_CURSORON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }
        public void CursorOn()
        {
            displayControl |= LCD_CURSORON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        // Turn on and off the blinking cursor
        public void BlinkOff()
        {
            displayControl &= (byte)~LCD_BLINKON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }
        public void BlinkOn()
        {
            displayControl |= LCD_BLINKON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        // These commands scroll the display without changing the RAM
        public void ScrollDisplayLeft()
        {
            Command((byte)((byte)I2CCommands.LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVELEFT));
        }

        public void ScrollDisplayRight()
        {
            Command((byte)((byte)I2CCommands.LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVERIGHT));
        }

        // This is for text that flows Left to Right
        public void SetLeftToRight()
        {
            displayMode |= LCD_ENTRYLEFT;
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
        }

        // This is for text that flows Right to Left
        public void SetRightToLeft()
        {
            displayMode &= (byte)~LCD_ENTRYLEFT;
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
        }

        // This will 'right justify' text from the cursor
        public void AutoscrollOn()
        {
            displayMode |= LCD_ENTRYSHIFTINCREMENT;
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
        }

        // This will 'left justify' text from the cursor
        public void AutoscrollOff()
        {
            displayMode &= (byte)~LCD_ENTRYSHIFTINCREMENT;
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
        }

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