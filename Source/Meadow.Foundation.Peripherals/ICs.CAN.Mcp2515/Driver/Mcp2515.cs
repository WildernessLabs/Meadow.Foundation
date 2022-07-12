using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.CAN
{
    /// <summary>
    /// Encapsulation for the Microchip MCP2515 CAN controller
    /// </summary>
    public partial class Mcp2515
    {
        public const SpiClockConfiguration.Mode DefaultSpiMode = SpiClockConfiguration.Mode.Mode0;

        private ISpiBus Bus { get; }
        private IDigitalOutputPort ChipSelect { get; }

        public Mcp2515(ISpiBus bus, IDigitalOutputPort chipSelect)
        {
            Bus = bus;
            ChipSelect = chipSelect;
        }

        public void Reset()
        {
            Span<byte> tx = stackalloc byte[1];
            Span<byte> rx = stackalloc byte[1];

            tx[0] = (byte)Command.Reset;

            Bus.Exchange(ChipSelect, tx, rx);

            // TODO: clear filters, etc.
        }

        private void SetMode(Mode mode)
        {
            ModifyRegister(Register.CANCTRL, (byte)Control.REQOP, (byte)mode);
        }

        private Status GetStatus()
        {
            Span<byte> tx = stackalloc byte[2];
            Span<byte> rx = stackalloc byte[2];

            tx[0] = (byte)Command.ReadStatus;
            tx[1] = 0;

            Bus.Exchange(ChipSelect, tx, rx);

            return (Status)rx[1];
        }

        private byte ReadRegister(Register register)
        {
            Span<byte> tx = stackalloc byte[3];
            Span<byte> rx = stackalloc byte[3];

            tx[0] = (byte)Command.Read;
            tx[1] = (byte)register;
            tx[2] = 0;

            Bus.Exchange(ChipSelect, tx, rx);

            return rx[2];
        }

        private byte[] ReadRegister(Register register, byte length)
        {
            Span<byte> tx = stackalloc byte[2 + length];
            Span<byte> rx = stackalloc byte[2 + length];

            tx[0] = (byte)Command.Read;
            tx[1] = (byte)register;

            Bus.Exchange(ChipSelect, tx, rx);

            return rx.Slice(2).ToArray();
        }

        private void ModifyRegister(Register register, byte mask, byte value)
        {
            Span<byte> tx = stackalloc byte[4];
            Span<byte> rx = stackalloc byte[4];

            tx[0] = (byte)Command.Bitmod;
            tx[1] = (byte)register;
            tx[2] = mask;
            tx[2] = value;

            Bus.Exchange(ChipSelect, tx, rx);
        }

        public bool IsFrameAvailable()
        {
            var status = GetStatus();

            if ((status & Status.RX0IF) == Status.RX0IF)
            {
                return true;
            }
            else if ((status & Status.RX1IF) == Status.RX1IF)
            {
                return true;
            }

            return false;
        }

        private Frame ReadFrame(RxBufferNumber bufferNumber)
        {
            var sidh_reg = bufferNumber == RxBufferNumber.RXB0 ? Register.RXB0SIDH : Register.RXB1SIDH;
            var ctrl_reg = bufferNumber == RxBufferNumber.RXB0 ? Register.RXB0CTRL : Register.RXB1CTRL;
            var data_reg = bufferNumber == RxBufferNumber.RXB0 ? Register.RXB0DATA : Register.RXB1DATA;
            var int_flag = bufferNumber == RxBufferNumber.RXB0 ? InterruptFlag.RX0IF : InterruptFlag.RX1IF;

            // read 5 bytes
            var buffer = ReadRegister(sidh_reg, 5);

            uint id = (uint)(buffer[MCP_SIDH << 3] + (buffer[MCP_SIDL] >> 5));

            // check to see if it's an extended ID
            if ((buffer[MCP_SIDL] & TXB_EXIDE_MASK) == TXB_EXIDE_MASK)
            {
                id = (uint)((id << 2) + (buffer[MCP_SIDL] & 0x03));
                id = (id << 8) + buffer[MCP_EID8];
                id = (id << 8) + buffer[MCP_EID0];
                id |= CAN_EFF_FLAG;
            }

            var dlc = buffer[MCP_DLC] & DLC_MASK;
            if (dlc > 8) throw new Exception("DLC is > 8 bytes");

            // see if it's a remote transmission request
            var ctrl = ReadRegister(ctrl_reg);
            if ((ctrl & RXBnCTRL_RTR) == RXBnCTRL_RTR)
            {
                id |= CAN_RTR_FLAG;
            }

            // create the frame
            var frame = new Frame
            {
                ID = id,
                PayloadLength = (byte)dlc
            };

            // read the frame data
            frame.Payload = ReadRegister(data_reg, frame.PayloadLength);

            // clear the interrupt flag
            ModifyRegister(Register.CANINTF, (byte)int_flag, 0);

            return frame;
        }

        public Frame? ReadFrame()
        {
            var status = GetStatus();

            if ((status & Status.RX0IF) == Status.RX0IF)
            { // message in buffer 0
                return ReadFrame(RxBufferNumber.RXB0);
            }
            else if ((status & Status.RX1IF) == Status.RX1IF)
            { // message in buffer 1
                return ReadFrame(RxBufferNumber.RXB1);
            }
            else
            { // no messages available
                return null;
            }
        }
    }
}