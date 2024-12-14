using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders;

public partial class Sc16is7x2
{
    /// <summary>
    /// Encapsulates a channel of the SC16IS7x2 peripheral as an ISerialPort
    /// </summary>
    public class Sc16is7x2Channel : ISerialPort
    {
        /// <inheritdoc/>
        public event SerialDataReceivedEventHandler DataReceived = default!;

        /// <inheritdoc/>
        public event EventHandler BufferOverrun = default!;

        private int _baudRate;
        private int _dataBits;
        private Parity _parity;
        private StopBits _stopBits;

        /// <inheritdoc/>
        public int BytesToRead => (_irqReadBuffer == null) 
            ? GetReadHwFifoCount() 
            : GetReadHwFifoCount() + _irqReadBuffer.Count;

        /// <inheritdoc/>
        public bool IsOpen { get; private set; }

        /// <inheritdoc/>
        public string PortName { get; }

        /// <summary>
        /// Size of the receive buffer (FIFO).
        /// </summary>
        /// <remarks>This is fixed at 64-bytes for this hardware</remarks>
        public int ReceiveBufferSize => 64;

        /// <inheritdoc/>
        public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <inheritdoc/>
        public TimeSpan WriteTimeout { get; set; } = TimeSpan.FromSeconds(5);

        private readonly Sc16is7x2 _controller;
        private readonly Channels _channel;
        private readonly IDigitalInterruptPort? _irq;

        private readonly FifoBuffer? _irqReadBuffer;

        public bool IsIrqDriven => _irq != null;

        // Optimizations for fast read. Precalculated values for this channel.
        private byte _rhrAddress;
        private byte _lsrAddress;
        private byte _rxlvlAddress;
        private byte _txlvlAddress;
        private IByteCommunications _comms;

        /// <summary>
        /// This method is never called directly from user code.
        /// </summary>
        /// <param name="controller">The parent Sc16is7x2 controller.</param>
        /// <param name="portName">Firendly port name</param>
        /// <param name="channel">Channel port code</param>
        /// <param name="baudRate"></param>
        /// <param name="dataBits"></param>
        /// <param name="parity"></param>
        /// <param name="stopBits"></param>
        /// <param name="isRS485"></param>
        /// <param name="invertDE"></param>
        /// <param name="irq">The IDigitalInterruptPort connected to the IRQ pin on SC16IS7x2.</param>
        /// <param name="readBufferSize">An interrupt from SC16IS7x2 will not be reset until all bytes in the FIFO are read. So we need a local buffer.</param>
        internal Sc16is7x2Channel(Sc16is7x2 controller, string portName, Channels channel, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, bool isRS485 = false, bool invertDE = false, IDigitalInterruptPort? irq = null, int readBufferSize = 1024)
        {
            PortName = portName;
            _controller = controller;
            _channel = channel;

            _rhrAddress = CalculateChannelAddress((byte)Registers.RHR);
            _lsrAddress = CalculateChannelAddress((byte)Registers.LSR);
            _rxlvlAddress = CalculateChannelAddress((byte)Registers.RXLVL);
            _txlvlAddress = CalculateChannelAddress((byte)Registers.TXLVL);
            _comms = controller.Comms;

            InitChannel(baudRate, dataBits, parity, stopBits);

            if (isRS485)
            {
                EnableRS485(invertDE);
            }

            if (irq != null)
            {
                // Setting up IRQ read with a large software FIFO buffer to offload the hardware FIFO of only 64 bytes.
                _irq = irq;
                _irqReadBuffer = new FifoBuffer(readBufferSize);
                // 21.04.2024, KW: We'll try to move this to Open(). And we'll remove this handler in Close().
                //EnableReceiveInterrupts();
                //_irq.Changed += UartChannelInterruptHandler;
            }

            // https://github.com/WildernessLabs/Meadow_Issues/issues/74
            // For some reason (missing hot-paths?), the first interrupt is very slow. So we'll try to perform some dummy runs to pave the way.
            // Doesn't really work... problem seems to be before our interrupt handler is called.

            //OnInterruptLineChanged(null, new DigitalPortResult());
            //ReadByte();
            //Read(new byte[0], 0, 0);
            //ReadAll();
            //if (_controller._irq is VirtualDigitalInterruptPort a)
            //    a.RaiseInterrupt(this, new DigitalPortResult());
        }

        internal void UartChannelInterruptHandler(object sender, DigitalPortResult e)
        {
            // If the first message after reboot is longer than the FIFO buffer, we already have a buffer overrun at this point.
            // For any consecutive messages, it works fine.
            // Ref: https://github.com/WildernessLabs/Meadow_Issues/issues/74
            int count = GetReadHwFifoCount();
            //Resolver.Log.Info($"->UART interrupt. Channel={_channel} port HW FIFO: {count} bytes");

            if (count == 0) return;     // The IRQ was not for us.

            if (_irqReadBuffer == null)
            {
                if (ReceiveInterruptPending())
                    this.DataReceived?.Invoke(this, new SerialDataReceivedEventArgs(SerialDataType.Chars));
            }
            else
            { 
                // If this is IRQ reading, shortcut the user callback. Empty FIFO ASAP.
                ReadAllIrqFifo();              
                if (_irqReadBuffer.Count > 0)
                    this.DataReceived?.Invoke(this, new SerialDataReceivedEventArgs(SerialDataType.Chars));
            }
        }

        /// <inheritdoc/>
        public int BaudRate
        {
            get => _baudRate;
            set => _baudRate = SetBaudRate(value);
        }

        /// <inheritdoc/>
        public int DataBits
        {
            get => _dataBits;
            set
            {
                SetLineSettings(value, Parity, StopBits);
                _dataBits = value;
            }
        }

        /// <inheritdoc/>
        public Parity Parity
        {
            get => _parity;
            set
            {
                SetLineSettings(DataBits, value, StopBits);
                _parity = value;
            }
        }

        /// <inheritdoc/>
        public StopBits StopBits
        {
            get => _stopBits;
            set
            {
                SetLineSettings(DataBits, Parity, value);
                _stopBits = value;
            }
        }

        private void InitChannel(int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            //_controller.Reset();
            EnableFifo();
            _baudRate = SetBaudRate(baudRate);
            SetLineSettings(dataBits, parity, stopBits);
        }

        /// <summary>
        /// Try to read all bytes from the FIFO buffer.
        /// </summary>
        internal void ReadAllIrqFifo()
        {
            if (_irqReadBuffer == null) return;

            int totalRead = 0;
            int count = GetReadHwFifoCount();   // How may bytes to read.
            //int count = _comms.ReadRegister(_rxlvlAddress);     // How may bytes to read.
            while (count > 0)
            {                
                for (int i = 0; i < count; i++)
                {
                    byte b = _comms.ReadRegister(_rhrAddress);
                    try
                    {
                        _irqReadBuffer.Write(b);
                    }
                    catch (Exception ex)
                    {
                        Resolver.Log.Error($"!!!!!---> ReadAllIrqFifo: Channel={_channel} Exception: {ex.Message}");
                        BufferOverrun?.Invoke(this, new ThreadExceptionEventArgs(ex));
                    }
                }
                totalRead += count;
                //Resolver.Log.Info($"---> ReadAllIrqFifo: Channel={_channel} Read {count}/{totalRead}");

                //byte lsr = _controller.ReadChannelRegister(Registers.LSR, _channel);
                byte lsr = _comms.ReadRegister(_lsrAddress);
                if ((lsr & RegisterBits.LSR_OVERRUN_ERROR) > 0)
                {
                    _irqReadBuffer.WriteString("[OVERRUN]");     // Not sure to keep this, but nice when debugging.
                    BufferOverrun?.Invoke(this, EventArgs.Empty);
                }

                count = GetReadHwFifoCount();   // Check that we're all done. To make sure IRQ is reset.
                //count = _comms.ReadRegister(_rxlvlAddress);     // Check that we're all done. To make sure IRQ is reset.
            }
            if (totalRead > 0)
            {
                //Resolver.Log.Info($"---> ReadAllIrqFifo: Channel={_channel} Done {count}/{totalRead}");
            }
        }

        /// <inheritdoc/>
        public int ReadByte()
        {
            if (_irqReadBuffer == null)
            {
                // The normal way...
                // check if data is available
                if (!IsHwFifoDataAvailable())
                {
                    return -1;
                }

                // read the data
                return ReadHwFifoByte();
            }
            else
            {
                // IRQ fast read from software FIFO....
                ReadAllIrqFifo();
                if (_irqReadBuffer.Count == 0)
                    return -1;
                else
                    return _irqReadBuffer.Read();
            }
        }

        /// <inheritdoc/>
        public int Read(byte[] buffer, int offset, int count)
        {
            var timeout = -1;
            if (ReadTimeout.TotalMilliseconds > 0)
            {
                timeout = Environment.TickCount + (int)ReadTimeout.TotalMilliseconds;
            }

            if (_irqReadBuffer == null)
            {
                // The normal way...
                var available = GetReadHwFifoCount();

                // read either the available or count, whichever is less, unless available is 0, in which case we wait until timeout
                while (available == 0)
                {
                    Thread.Sleep(10);
                    available = GetReadHwFifoCount();

                    if (timeout > 0)
                    {
                        if (Environment.TickCount >= timeout)
                        {
                            throw new TimeoutException();
                        }
                    }
                }

                var toRead = available <= count ? available : count;

                for (var i = 0; i < toRead; i++)
                {
                    buffer[i + offset] = ReadHwFifoByte();
                }

                return toRead;
            }
            else
            {
                // IRQ fast read....
                ReadAllIrqFifo();  // Always read whatever is in the FIFO.

                // Read either the available or count, whichever is less, unless available is 0, in which case we wait until timeout
                // Do we have to wait for some data?
                while (_irqReadBuffer.Count == 0)
                {
                    Thread.Sleep(10);

                    ReadAllIrqFifo();
                    if (_irqReadBuffer.Count > 0) break;

                    if (timeout > 0)
                    {
                        if (Environment.TickCount >= timeout)
                        {
                            throw new TimeoutException();
                        }
                    }
                }

                // Read the requested count bytes, or all available, whichever is less
                var toRead = _irqReadBuffer.Count <= count ? _irqReadBuffer.Count : count;
                for (var i = 0; i < toRead; i++)
                {
                    buffer[i + offset] = _irqReadBuffer.Read();
                }
                return toRead;
            }
        }

        /// <inheritdoc/>
        public byte[] ReadAll()
        {
            if (_irqReadBuffer == null)
            {
                // The normal way...
                var available = GetReadHwFifoCount();
                var buffer = new byte[available];
                Read(buffer, 0, available);
                return buffer;
            }
            else
            {
                // IRQ fast read....
                ReadAllIrqFifo();  // Always read whatever is in the FIFO.
                var available = _irqReadBuffer.Count;
                var buffer = new byte[available];
                //for (int i = 0; i < available; i++)
                //{
                //    buffer[i] = _irqReadBuffer.Read();
                //}
                _irqReadBuffer.MoveItemsTo(buffer, 0, available);
                return buffer;
            }
        }

        /// <inheritdoc/>
        public void ClearReceiveBuffer()
        {
            ResetReadHwFifo();
            if (IsIrqDriven)
                _irqReadBuffer.Clear();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // nop
        }

        /// <inheritdoc/>
        public void Open()
        {
            if (!IsOpen)
            {
                IsOpen = true;
                ResetWriteHwFifo();
                ClearReceiveBuffer();
                if (IsIrqDriven)
                    _irq.Changed += UartChannelInterruptHandler;
                EnableReceiveInterrupts();
            }
        }

        /// <inheritdoc/>
        public void Close()
        {
            if (IsOpen)
            {
                IsOpen = false;
                DisableReceiveInterrupts();
                if (IsIrqDriven)
                    _irq.Changed -= UartChannelInterruptHandler;
            }
        }

        /// <inheritdoc/>
        public int Peek()
        {
            // TODO: read and buffer the 1 byte for the next actual read
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public int Write(byte[] buffer)
        {
            return Write(buffer, 0, buffer.Length);
        }

        /// <inheritdoc/>
        public int Write(byte[] buffer, int offset, int count)
        {
            var timeout = -1;
            var index = offset;
            var remaining = count;

            if (ReadTimeout.TotalMilliseconds > 0)
            {
                timeout = Environment.TickCount + (int)ReadTimeout.TotalMilliseconds;
            }
            // wait for THR to be empty
            while (!IsTransmitHoldingRegisterEmpty())
            {
                Thread.Sleep(10);
            }

            // write until we're either written all or the THR is full
            var available = GetWriteHwFifoSpace();

            while (remaining > 0)
            {
                // make sure THR is empty
                while (available <= 0)
                {
                    Thread.Sleep(10);
                    available = GetWriteHwFifoSpace();

                    if (timeout > 0)
                    {
                        if (Environment.TickCount > timeout)
                        {
                            throw new TimeoutException();
                        }
                    }
                }

                WriteHwFifoByte(buffer[index]);
                index++;
                available--;
                remaining--;

                if (available == 0)
                {
                    available = GetWriteHwFifoSpace();
                }
            }

            return count;
        }


        // ******************* Hardware communication methods *******************
        // Moved here from Sc16is7x2.cs so we can ommit the channel parameter
        // and unnecessary calls to parent class. (We keep calls local)

        internal void EnableFifo()
        {
            var fcr = ReadChannelRegister(Registers.FCR);
            fcr |= RegisterBits.FCR_FIFO_ENABLE;
            WriteChannelRegister(Registers.FCR, fcr);
        }

        internal void EnableReceiveInterrupts()
        {
            //var ier = ReadChannelRegister(Registers.IER);
            //ier |= RegisterBits.IER_RHR_ENABLE;
            //WriteChannelRegister(Registers.IER, ier);
            SetChannelRegisterBits(Registers.IER, RegisterBits.IER_RHR_ENABLE);
        }

        internal void DisableReceiveInterrupts()
        {
            //var ier = ReadChannelRegister(Registers.IER);
            //ier &= (byte)~(RegisterBits.IER_RHR_ENABLE);
            //WriteChannelRegister(Registers.IER, ier);
            ClearChannelRegisterBits(Registers.IER, RegisterBits.IER_RHR_ENABLE);
        }

        internal bool ReceiveInterruptPending()
        {
            // IIR[0] is 0 for any pending interrupt
            // RHR will be IIR[2] *exclusively*
            var iir = ReadChannelRegister(Registers.IIR);
            return (iir & RegisterBits.IIR_RHR_INTERRUPT) == RegisterBits.IIR_RHR_INTERRUPT;
        }

        internal void SetLineSettings(int dataBits, Parity parity, StopBits stopBits)
        {
            var lcr = ReadChannelRegister(Registers.LCR);
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

            WriteChannelRegister(Registers.LCR, lcr);
        }

        internal int SetBaudRate(int baudRate)
        {
            // the part baud rate is a division of the oscillator frequency, not necessarily the value requested
            var mcr = ReadChannelRegister(Registers.MCR);
            var prescaler = ((mcr & RegisterBits.MCR_CLOCK_DIVISOR) == 0) ? 1 : 4;
            var divisor1 = _controller.OscillatorFrequency.Hertz / prescaler;
            var divisor2 = baudRate * 16;

            if (divisor2 > divisor1) throw new ArgumentOutOfRangeException(nameof(baudRate), "Oscillator does not allow requested baud rate");

            var divisor = (ushort)Math.Ceiling(divisor1 / divisor2);

            // enable the divisor latch
            var lcr = ReadChannelRegister(Registers.LCR);
            lcr |= RegisterBits.LCR_DIVISOR_LATCH_ENABLE;
            WriteChannelRegister(Registers.LCR, lcr);

            // set the baud rate
            WriteChannelRegister(Registers.DLL, (byte)(divisor & 0xff));
            WriteChannelRegister(Registers.DLH, (byte)(divisor >> 8));

            // disable the divisor latch
            lcr &= unchecked((byte)~RegisterBits.LCR_DIVISOR_LATCH_ENABLE);
            WriteChannelRegister(Registers.LCR, lcr);

            // return the actual baud rate achieved
            return (int)(divisor1 / divisor / 16);
        }

        internal void EnableRS485(bool invertDE)
        {
            var efcr = ReadChannelRegister(Registers.EFCR);
            efcr |= RegisterBits.EFCR_9BITMODE | RegisterBits.EFCR_RTSCON;

            if (invertDE)
            {
                efcr |= RegisterBits.EFCR_RTSINVER;
            }
            else
            {
                efcr &= unchecked((byte)~RegisterBits.EFCR_RTSINVER);
            }

            WriteChannelRegister(Registers.EFCR, efcr);
        }


        // ******************* UART read methods *******************

        /// <summary>
        /// Returns the number of bytes in the receive FIFO.
        /// </summary>
        /// <returns></returns>
        internal int GetReadHwFifoCount()
        {
            return _comms.ReadRegister(_rxlvlAddress);
        }

        /// <summary>
        /// Returns true if there is data available in the receive FIFO.
        /// </summary>
        /// <returns></returns>
        internal bool IsHwFifoDataAvailable()
        {
            return GetReadHwFifoCount() > 0;
        }

        internal void ResetReadHwFifo()
        {
            var fcr = ReadChannelRegister(Registers.FCR);
            fcr |= RegisterBits.FCR_RX_FIFO_RESET;
            WriteChannelRegister(Registers.FCR, fcr);
        }

        internal byte ReadHwFifoByte()
        {
            return ReadChannelRegister(Registers.RHR);
        }


        // ******************* UART write methods *******************

        /// <summary>
        /// Returns the empty space in the transmit FIFO.
        /// </summary>
        /// <returns></returns>
        internal int GetWriteHwFifoSpace()
        {
            return _comms.ReadRegister(_txlvlAddress);
        }

        /// <summary>
        /// Reading status from the THR bit in the LSR register.
        /// </summary>
        /// <returns></returns>
        internal bool IsTransmitHoldingRegisterEmpty()
        {
            var lsr = ReadChannelRegister(Registers.LSR);
            return (lsr & RegisterBits.LSR_THR_EMPTY) == RegisterBits.LSR_THR_EMPTY;
        }

        internal void ResetWriteHwFifo()
        {
            var fcr = ReadChannelRegister(Registers.FCR);
            fcr |= RegisterBits.FCR_TX_FIFO_RESET;
            WriteChannelRegister(Registers.FCR, fcr);
        }

        internal void WriteHwFifoByte(byte data)
        {
            WriteChannelRegister(Registers.THR, data);
        }


        // ******************* Channel communication methods *******************

        private byte CalculateChannelAddress(byte register)
        {
            // see page 40 of the data sheet for explanation of this
            var subaddress = (byte)(((byte)register << 3) | ((byte)_channel << 1));
            return subaddress;
        }

        private byte ReadChannelRegister(Registers register)
        {
            // see page 40 of the data sheet for explanation of this
            var subaddress = (byte)(((byte)register << 3) | ((byte)_channel << 1));
            byte v = _comms.ReadRegister(subaddress);
            return v;
        }

        private void WriteChannelRegister(Registers register, byte value)
        {
            // see page 40 of the data sheet for explanation of this
            var subaddress = (byte)(((byte)register << 3) | ((byte)_channel << 1));
            _comms.WriteRegister(subaddress, value);

            //int b = GetWriteHwFifoSpace();
            //int c = GetReadHwFifoCount();
            //Resolver.Log.Info($"->UART write. Channel={_channel} port HW FIFO: {b} bytes Read FIFO: {c}");
        }

        /// <summary>
        /// Sets bits in the register if the corresponding bit in the value parameter is set.
        /// </summary>
        /// <param name="register"></param>
        /// <param name="value"></param>
        private void SetChannelRegisterBits(Registers register, byte value)
        {
            byte currentValue = ReadChannelRegister(register);
            currentValue |= value;          // Set the bits we're going to change
            WriteChannelRegister(register, currentValue);
        }

        /// <summary>
        /// Clears bits in the register if the corresponding bit in the mask parameter is set.
        /// </summary>
        /// <param name="register"></param>
        /// <param name="mask"></param>
        private void ClearChannelRegisterBits(Registers register, byte mask)
        {
            byte currentValue = ReadChannelRegister(register);
            currentValue &= (byte)~mask;          // Flip all bits in mask, then AND with currentValue
            WriteChannelRegister(register, currentValue);
        }
    }
}
