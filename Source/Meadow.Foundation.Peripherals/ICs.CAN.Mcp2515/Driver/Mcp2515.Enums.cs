using System;
using System.Runtime.InteropServices;

namespace Meadow.Foundation.ICs.CAN
{
    public partial class Mcp2515
    {
        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 16)]
        public struct Frame
        {
            [FieldOffset(0)]
            public uint ID;
            [FieldOffset(4)]
            public byte PayloadLength;
            [FieldOffset(8)]
            public byte[] Payload;
        }

        private enum Register : byte
        {
            RXF0SIDH = 0x00,
            RXF0SIDL = 0x01,
            RXF0EID8 = 0x02,
            RXF0EID0 = 0x03,
            RXF1SIDH = 0x04,
            RXF1SIDL = 0x05,
            RXF1EID8 = 0x06,
            RXF1EID0 = 0x07,
            RXF2SIDH = 0x08,
            RXF2SIDL = 0x09,
            RXF2EID8 = 0x0A,
            RXF2EID0 = 0x0B,
            CANSTAT = 0x0E,
            CANCTRL = 0x0F,
            RXF3SIDH = 0x10,
            RXF3SIDL = 0x11,
            RXF3EID8 = 0x12,
            RXF3EID0 = 0x13,
            RXF4SIDH = 0x14,
            RXF4SIDL = 0x15,
            RXF4EID8 = 0x16,
            RXF4EID0 = 0x17,
            RXF5SIDH = 0x18,
            RXF5SIDL = 0x19,
            RXF5EID8 = 0x1A,
            RXF5EID0 = 0x1B,
            TEC = 0x1C,
            REC = 0x1D,
            RXM0SIDH = 0x20,
            RXM0SIDL = 0x21,
            RXM0EID8 = 0x22,
            RXM0EID0 = 0x23,
            RXM1SIDH = 0x24,
            RXM1SIDL = 0x25,
            RXM1EID8 = 0x26,
            RXM1EID0 = 0x27,
            CNF3 = 0x28,
            CNF2 = 0x29,
            CNF1 = 0x2A,
            CANINTE = 0x2B,
            CANINTF = 0x2C,
            EFLG = 0x2D,
            TXB0CTRL = 0x30,
            TXB0SIDH = 0x31,
            TXB0SIDL = 0x32,
            TXB0EID8 = 0x33,
            TXB0EID0 = 0x34,
            TXB0DLC = 0x35,
            TXB0DATA = 0x36,
            TXB1CTRL = 0x40,
            TXB1SIDH = 0x41,
            TXB1SIDL = 0x42,
            TXB1EID8 = 0x43,
            TXB1EID0 = 0x44,
            TXB1DLC = 0x45,
            TXB1DATA = 0x46,
            TXB2CTRL = 0x50,
            TXB2SIDH = 0x51,
            TXB2SIDL = 0x52,
            TXB2EID8 = 0x53,
            TXB2EID0 = 0x54,
            TXB2DLC = 0x55,
            TXB2DATA = 0x56,
            RXB0CTRL = 0x60,
            RXB0SIDH = 0x61,
            RXB0SIDL = 0x62,
            RXB0EID8 = 0x63,
            RXB0EID0 = 0x64,
            RXB0DLC = 0x65,
            RXB0DATA = 0x66,
            RXB1CTRL = 0x70,
            RXB1SIDH = 0x71,
            RXB1SIDL = 0x72,
            RXB1EID8 = 0x73,
            RXB1EID0 = 0x74,
            RXB1DLC = 0x75,
            RXB1DATA = 0x76
        }

        private enum Command : byte
        {
            Write = 0x02,
            Read = 0x03,
            Bitmod = 0x05,
            LoadTX0 = 0x40,
            LoadTX1 = 0x42,
            LoadTX2 = 0x44,
            RTS_TX0 = 0x81,
            RTS_TX1 = 0x82,
            RTS_TX2 = 0x84,
            RTSALL = 0x87,
            ReadRX0 = 0x90,
            ReadRX1 = 0x94,
            ReadStatus = 0xA0,
            RX_Status = 0xB0,
            Reset = 0xC0
        }

        private enum RxBufferNumber
        {
            RXB0 = 0,
            RXB1 = 1,
        }

        private enum Mode : byte
        {
            Normal = 0x00,
            Sleep = 0x20,
            Loopback = 0x40,
            ListenOnly = 0x60,
            Configure = 0x80,
            PowerUp = 0xE0
        }

        private enum Control : byte
        {
            REQOP = 0xE0,
            ABAT = 0x10,
            OSM = 0x08,
            CLKEN = 0x04,
            CLKPRE = 0x03
        }

        [Flags]
        private enum Status : byte
        {
            NONE = 0,
            RX0IF = (1 << 0),
            RX1IF = (1 << 1)
        }

        private enum Result : byte
        {
            Ok = 0,
            Failed = 1,
            TransmitBusy = 2,
            FailToInit = 3,
            FailToSend = 4,
            NoMessage = 5
        }

        [Flags]
        private enum InterruptFlag : byte
        {
            RX0IF = 0x01,
            RX1IF = 0x02,
            TX0IF = 0x04,
            TX1IF = 0x08,
            TX2IF = 0x10,
            ERRIF = 0x20,
            WAKIF = 0x40,
            MERRF = 0x80
        }

        private const byte MCP_SIDH = 0;
        private const byte MCP_SIDL = 1;
        private const byte MCP_EID8 = 2;
        private const byte MCP_EID0 = 3;
        private const byte MCP_DLC = 4;
        private const byte MCP_DATA = 5;

        private const byte TXB_EXIDE_MASK = 0x08;
        private const byte DLC_MASK = 0x0F;
        private const byte RTR_MASK = 0x40;

        private const uint CAN_EFF_FLAG = 0x80000000;
        private const int CAN_RTR_FLAG = 0x40000000;
        private const int CAN_ERR_FLAG = 0x20000000;

        private const byte RXBnCTRL_RTR = 0x08;
    }
}
