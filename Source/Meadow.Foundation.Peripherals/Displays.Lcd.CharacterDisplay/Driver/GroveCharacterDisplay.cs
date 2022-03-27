using System.Threading;
using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Lcd
{
    public class GroveCharacterDisplay : I2cCharacterDisplay
    {
        public GroveCharacterDisplay(II2cBus i2cBus,
            byte address = (byte)Addresses.Address_0x3E,
            byte rows = 2, byte columns = 16)
            : base(i2cBus, address, rows, columns)
        {
        }

        protected override void Initialize()
        {
            var displayFunction = (byte)(LCD_4BITMODE | LCD_1LINE | LCD_5x8DOTS);

            if (DisplayConfig.Height > 1)
            {
                displayFunction |= LCD_2LINE;
            }

            Thread.Sleep(50);

            Command((byte)((byte)I2CCommands.LCD_FUNCTIONSET | displayFunction));
            Thread.Sleep(50);

            Command((byte)((byte)I2CCommands.LCD_FUNCTIONSET | displayFunction));
            Thread.Sleep(2);

            Command((byte)((byte)I2CCommands.LCD_FUNCTIONSET | displayFunction));

            Command((byte)((byte)I2CCommands.LCD_FUNCTIONSET | displayFunction));

            // turn the display on with no cursor or blinking default
            displayControl = (byte)(LCD_DISPLAYON | LCD_CURSOROFF | LCD_BLINKOFF);
            DisplayOn();

            // clear it off
            ClearLines();

            // Initialize to default text direction (for romance languages)
            displayMode = (byte)(LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT);
            // set the entry mode
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
        }

        // send command
        protected override void Command(byte value)
        {
            var data = new byte[] { 0x80, value };
            i2cPeripheral.Write(data);
        }

        protected override void Send(byte value, byte mode)
        {
            var data = new byte[] { 0x40, value };
            i2cPeripheral.Write(data);
        }

        public void DisplayOn()
        {
            displayControl &= (byte)~LCD_DISPLAYON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        public void DisplayOff()
        {
            displayControl |= LCD_DISPLAYON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        // Turns the underline cursor on/off
        public void ShowCursor()
        {
            displayControl &= (byte)~LCD_CURSORON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        public void HideCursor()
        {
            displayControl |= LCD_CURSORON;
            Command((byte)((byte)I2CCommands.LCD_DISPLAYCONTROL | displayControl));
        }

        // Turn on and off the blinking cursor
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
        public void LeftToRight()
        {
            displayMode |= LCD_ENTRYLEFT;
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
        }

        // This is for text that flows Right to Left
        public void RightToLeft()
        {
            displayMode &= (byte)~LCD_ENTRYLEFT;
            Command((byte)((byte)I2CCommands.LCD_ENTRYMODESET | displayMode));
        }

        // This will 'right justify' text from the cursor
        public void Autoscroll(bool scroll)
        {
            if(scroll)
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
    }
}