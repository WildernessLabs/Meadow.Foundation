
using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an Sc16is7x2 I2C UART
    /// </summary>
    public partial class Sc16is7x2 : ISerialController
    {
        /// <summary>
        /// The port name for Port A
        /// </summary>
        public Sc16SerialPortName PortA => new Sc16SerialPortName("PortA", "A", this);
        /// <summary>
        /// The port name for Port B
        /// </summary>
        public Sc16SerialPortName PortB => new Sc16SerialPortName("PortB", "B", this);

        private Sc16is7x2Channel? _channelA;
        private Sc16is7x2Channel? _channelB;
        private Frequency _oscillatorFrequency;
        private IDigitalInterruptPort? _irq;

        internal Sc16is7x2(Frequency oscillatorFrequency, IDigitalInterruptPort? irq)
        {
            _oscillatorFrequency = oscillatorFrequency;
            _irq = irq;
        }

        private IByteCommunications Comms
        {
            get
            {
                if (_i2cComms != null) return _i2cComms;
                if (_spiComms != null) return _spiComms;
                throw new System.Exception("No comms interface found");
            }
        }

        /// <summary>
        /// Creates an RS232 Serial Port
        /// </summary>
        /// <param name="portName">The Sc16SerialPortName name of the channel to create</param>
        /// <param name="baudRate">The baud rate used in communication</param>
        /// <param name="dataBits">The data bits used in communication</param>
        /// <param name="parity">The parity used in communication</param>
        /// <param name="stopBits">The stop bits used in communication</param>
        /// <param name="readBufferSize">The buffer read buffer size</param>
        public ISerialPort CreateSerialPort(SerialPortName portName, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int readBufferSize = 64)
        {
            if (_irq != null && _irq.InterruptMode != InterruptMode.EdgeRising)
            {
                throw new ArgumentException("If an interrupt port is provided, it must be a rising edge interrupt");
            }

            switch (portName.SystemName)
            {
                case "A":
                    if (_channelA == null)
                    {
                        _channelA = new Sc16is7x2Channel(this, portName.FriendlyName, Channels.A, baudRate, dataBits, parity, stopBits, irq: _irq);
                        return _channelA;
                    }
                    throw new PortInUseException($"{portName.FriendlyName} already in use");
                case "B":
                    if (_channelB == null)
                    {
                        _channelB = new Sc16is7x2Channel(this, portName.FriendlyName, Channels.B, baudRate, dataBits, parity, stopBits, irq: _irq);
                        return _channelB;
                    }
                    throw new PortInUseException($"{portName.FriendlyName} already in use");
            }

            throw new Exception("Unknown port");
        }

        /// <summary>
        /// Creates an RS485 Serial Port
        /// </summary>
        /// <param name="portName">The Sc16SerialPortName name of the channel to create</param>
        /// <param name="baudRate">The baud rate used in communication</param>
        /// <param name="dataBits">The data bits used in communication</param>
        /// <param name="parity">The parity used in communication</param>
        /// <param name="stopBits">The stop bits used in communication</param>
        /// <param name="invertDE">Set to true to invert the logic (active high) driver enable output signal</param>
        public ISerialPort CreateRs485SerialPort(Sc16SerialPortName portName, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, bool invertDE = false)
        {
            if (_irq != null && _irq.InterruptMode != InterruptMode.EdgeRising)
            {
                throw new ArgumentException("If an interrupt port is provided, it must be a rising edge interrupt");
            }

            switch (portName.SystemName)
            {
                case "A":
                    if (_channelA == null)
                    {
                        _channelA = new Sc16is7x2Channel(this, portName.FriendlyName, Channels.A, baudRate, dataBits, parity, stopBits, true, invertDE, _irq);
                        return _channelA;
                    }
                    throw new PortInUseException($"{portName.FriendlyName} already in use");
                case "B":
                    if (_channelB == null)
                    {
                        _channelB = new Sc16is7x2Channel(this, portName.FriendlyName, Channels.B, baudRate, dataBits, parity, stopBits, true, invertDE, _irq);
                        return _channelB;
                    }
                    throw new PortInUseException($"{portName.FriendlyName} already in use");
            }

            throw new Exception("Unknown port");
        }

        internal bool ReceiveInterruptPending(Channels channel)
        {
            // IIR[0] is 0 for any pending interrupt
            // RHR will be IIR[2] *exclusively*
            var iir = ReadChannelRegister(Registers.IIR, channel);
            return (iir & RegisterBits.IIR_RHR_INTERRUPT) == RegisterBits.IIR_RHR_INTERRUPT;
        }

        internal void EnableReceiveInterrupts(Channels channel)
        {
            var ier = ReadChannelRegister(Registers.IER, channel);
            ier |= RegisterBits.IER_RHR_ENABLE;
            WriteChannelRegister(Registers.IER, channel, ier);
        }

        internal void EnableRS485(Channels channel, bool invertDE)
        {
            var efcr = ReadChannelRegister(Registers.EFCR, channel);
            efcr |= RegisterBits.EFCR_9BITMODE | RegisterBits.EFCR_RTSCON;

            if (invertDE)
            {
                efcr |= RegisterBits.EFCR_RTSINVER;
            }
            else
            {
                efcr &= unchecked((byte)~RegisterBits.EFCR_RTSINVER);
            }

            WriteChannelRegister(Registers.EFCR, channel, efcr);
        }

        internal void WriteByte(Channels channel, byte data)
        {
            WriteChannelRegister(Registers.THR, channel, data);
        }

        /// <summary>
        /// Reads the empty space in the transmit fifo
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        internal int GetWriteFifoSpace(Channels channel)
        {
            return ReadChannelRegister(Registers.TXLVL, channel);
        }

        internal int GetReadFifoCount(Channels channel)
        {
            return ReadChannelRegister(Registers.RXLVL, channel);
        }

        internal void ResetReadFifo(Channels channel)
        {
            var fcr = ReadChannelRegister(Registers.FCR, channel);
            fcr |= RegisterBits.FCR_RX_FIFO_RESET;
            WriteChannelRegister(Registers.FCR, channel, fcr);
        }

        internal bool IsFifoDataAvailable(Channels channel)
        {
            return GetReadFifoCount(channel) > 0;
        }

        internal byte ReadByte(Channels channel)
        {
            return ReadChannelRegister(Registers.RHR, channel);
        }

        internal void SetLineSettings(Channels channel, int dataBits, Parity parity, StopBits stopBits)
        {
            var lcr = ReadChannelRegister(Registers.LCR, channel);
            lcr &= unchecked((byte)~0x3f); // clear all of the line setting bits for simplicity

            switch (dataBits)
            {
                case 5:
                    lcr |= RegisterBits.LCR_5_DATA_BITS;
                    break;
                case 6:
                    lcr |= RegisterBits.LCR_6_DATA_BITS;
                    break;
                case 7:
                    lcr |= RegisterBits.LCR_7_DATA_BITS;
                    break;
                case 8:
                    lcr |= RegisterBits.LCR_8_DATA_BITS;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataBits));

            }

            if (stopBits == StopBits.Two)
            {
                lcr |= RegisterBits.LCR_2_STOP_BITS;
            }

            switch (parity)
            {
                case Parity.None:
                    lcr |= RegisterBits.LCR_PARITY_NONE;
                    break;
                case Parity.Odd:
                    lcr |= RegisterBits.LCR_PARITY_ODD;
                    break;
                case Parity.Even:
                    lcr |= RegisterBits.LCR_PARITY_EVEN;
                    break;
                    // device supports mark and space, but Meadow doesn't have values for it
            }

            WriteChannelRegister(Registers.LCR, channel, lcr);
        }

        internal int SetBaudRate(Channels channel, int baudRate)
        {
            // the part baud rate is a division of the oscillator frequency, not necessarily the value requested
            var mcr = ReadChannelRegister(Registers.MCR, channel);
            var prescaler = ((mcr & RegisterBits.MCR_CLOCK_DIVISOR) == 0) ? 1 : 4;
            var divisor1 = _oscillatorFrequency.Hertz / prescaler;
            var divisor2 = baudRate * 16;

            if (divisor2 > divisor1) throw new ArgumentOutOfRangeException(nameof(baudRate), "Oscillator does not allow requested baud rate");

            var divisor = (ushort)Math.Ceiling(divisor1 / divisor2);

            // enable the divisor latch
            var lcr = ReadChannelRegister(Registers.LCR, channel);
            lcr |= RegisterBits.LCR_DIVISOR_LATCH_ENABLE;
            WriteChannelRegister(Registers.LCR, channel, lcr);

            // set the baud rate
            WriteChannelRegister(Registers.DLL, channel, (byte)(divisor & 0xff));
            WriteChannelRegister(Registers.DLH, channel, (byte)(divisor >> 8));

            // disable the divisor latch
            lcr &= unchecked((byte)~RegisterBits.LCR_DIVISOR_LATCH_ENABLE);
            WriteChannelRegister(Registers.LCR, channel, lcr);

            // return the actual baud rate achieved
            return (int)(divisor1 / divisor / 16);
        }

        internal void Reset(Channels channel)
        {
            var value = ReadChannelRegister(Registers.IOControl, channel);
            value |= RegisterBits.IOCTL_RESET;
            WriteChannelRegister(Registers.IOControl, channel, value);
        }

        internal void EnableFifo(Channels channel)
        {
            var fcr = ReadChannelRegister(Registers.FCR, channel);
            fcr |= RegisterBits.FCR_FIFO_ENABLE;
            WriteChannelRegister(Registers.FCR, channel, fcr);
        }

        private byte ReadChannelRegister(Registers register, Channels channel)
        {
            // see page 40 of the data sheet for explanation of this
            var subaddress = (byte)(((byte)register << 3) | ((byte)channel << 1));
            return Comms.ReadRegister(subaddress);
        }

        private void WriteChannelRegister(Registers register, Channels channel, byte value)
        {
            // see page 40 of the data sheet for explanation of this
            var subaddress = (byte)(((byte)register << 3) | ((byte)channel << 1));
            Comms.WriteRegister(subaddress, value);
        }
    }
}