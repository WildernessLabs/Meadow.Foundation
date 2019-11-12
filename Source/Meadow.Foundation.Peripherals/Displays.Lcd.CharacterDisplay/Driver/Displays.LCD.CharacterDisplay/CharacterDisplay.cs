/*
Driver ported from http://wiki.sunfounder.cc/images/b/bb/CharacterDisplay_for_Raspberry_Pi.zip
For reference: http://wiki.sunfounder.cc/index.php?title=CharacterDisplay_Module
Brian Kim 5/5/2018
*/

using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;

namespace Meadow.Foundation.Displays.Lcd
{
    public class CharacterDisplay : ITextDisplay
    {
        private byte LCD_LINE_1 = 0x80; // # LCD RAM address for the 1st line
        private byte LCD_LINE_2 = 0xC0; // # LCD RAM address for the 2nd line
        private byte LCD_LINE_3 = 0x94; // # LCD RAM address for the 3rd line
        private byte LCD_LINE_4 = 0xD4; // # LCD RAM address for the 4th line

        private const byte LCD_SETDDRAMADDR = 0x80;
        private const byte LCD_SETCGRAMADDR = 0x40;

        protected IDigitalOutputPort LCD_E;
        protected IDigitalOutputPort LCD_RS;
        protected IDigitalOutputPort LCD_D4;
        protected IDigitalOutputPort LCD_D5;
        protected IDigitalOutputPort LCD_D6;
        protected IDigitalOutputPort LCD_D7;
        protected IDigitalOutputPort LED_ON;

        private bool LCD_INSTRUCTION = false;
        private bool LCD_DATA = true;
        private static object _lock = new object();

        public TextDisplayConfig DisplayConfig { get; protected set; }

        public CharacterDisplay(IIODevice device, IPin pinRS, IPin pinE, IPin pinD4, IPin pinD5, IPin pinD6, IPin pinD7, 
            ushort rows = 4, ushort columns = 20)
        {
            DisplayConfig = new TextDisplayConfig { Height = rows, Width = columns };

            LCD_RS = device.CreateDigitalOutputPort(pinRS); 
            LCD_E  = device.CreateDigitalOutputPort(pinE);
            LCD_D4 = device.CreateDigitalOutputPort(pinD4);
            LCD_D5 = device.CreateDigitalOutputPort(pinD5);
            LCD_D6 = device.CreateDigitalOutputPort(pinD6);
            LCD_D7 = device.CreateDigitalOutputPort(pinD7);

            Initialize();
        }

        public CharacterDisplay(IDigitalOutputPort portRS,
                        IDigitalOutputPort portE,
                        IDigitalOutputPort portD4,
                        IDigitalOutputPort portD5,
                        IDigitalOutputPort portD6,
                        IDigitalOutputPort portD7,
                        ushort rows = 4, ushort columns = 20)
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

    /*    public CharacterDisplay(MCP23008 mcp, ushort rows = 20, ushort columns = 4)
        {
            DisplayConfig = new TextDisplayConfig { Height = rows, Width = columns };

            LCD_RS = mcp.CreateOutputPort(1, false);
            LCD_E =  mcp.CreateOutputPort(2, false);
            LCD_D4 = mcp.CreateOutputPort(3, false);
            LCD_D5 = mcp.CreateOutputPort(4, false);
            LCD_D6 = mcp.CreateOutputPort(5, false);
            LCD_D7 = mcp.CreateOutputPort(6, false);

            var lite = mcp.CreateOutputPort(7, true);

            Initialize();
        } */

        private void Initialize()
        {
            SendByte(0x33, LCD_INSTRUCTION); // 110011 Initialise
            SendByte(0x32, LCD_INSTRUCTION); // 110010 Initialise
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

        public void WriteLine(string text, byte lineNumber)
        {
            ClearLine(lineNumber);
            SetLineAddress(lineNumber);

            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            foreach (var b in bytes)
            {
                SendByte(b, LCD_DATA);
            }
        }

        public void Write(string text)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            foreach (var b in bytes)
            {
                SendByte(b, LCD_DATA);
            }
        }

        public void SetCursorPosition(byte column, byte line)
        {
            byte lineAddress = GetLineAddress(line);
            var address = column + lineAddress;
            SendByte(((byte)(LCD_SETDDRAMADDR | address)), LCD_INSTRUCTION);
        }

        public void Clear()
        {
            SendByte(0x01, LCD_INSTRUCTION);
            SetCursorPosition(1, 0);
            Thread.Sleep(5);
        }

        public void ClearLine(byte lineNumber)
        {
            SetLineAddress(lineNumber);

            for(int i=0; i < DisplayConfig.Width; i++)
            {
                Write(" ");
            }
        }

        public void SetBrightness(float brightness = 0.75F)
        {
            Console.WriteLine("Set brightness not enabled");
        }

        public void SaveCustomCharacter(byte[] characterMap, byte address)
        {
            address &= 0x7; // we only have 8 locations 0-7
            SendByte((byte)(LCD_SETCGRAMADDR | (address << 3)), LCD_INSTRUCTION);

            for (var i = 0; i < 8; i++)
            {
                SendByte(characterMap[i], LCD_DATA);
            }
        }
    }
}