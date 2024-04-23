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
            ? _controller.GetReadFifoCount(_channel) 
            : _controller.GetReadFifoCount(_channel) + _irqReadBuffer.Count;

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

            Initialize(baudRate, dataBits, parity, stopBits);
            if (isRS485)
            {
                InitializeRS485(invertDE);
            }

            if (irq != null)
            {
                // Setting up IRQ read with a large software FIFO buffer to offload the hardware FIFO of only 64 bytes.
                _irq = irq;
                _irqReadBuffer = new FifoBuffer(readBufferSize);
                _controller.EnableReceiveInterrupts(_channel);
                _irq.Changed += OnInterruptLineChanged;
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

        internal void OnInterruptLineChanged(object sender, DigitalPortResult e)
        {
            // If the first message after reboot is longer than the FIFO buffer, we already have a buffer overrun at this point.
            // For any consecutive messages, it works fine.
            // Ref: https://github.com/WildernessLabs/Meadow_Issues/issues/74
            //int count = _controller.GetReadFifoCount(_channel);
            //Resolver.Log.Info($"Channel {_channel} port FIFO: {count} bytes");

            if (_irqReadBuffer == null)
            {
                if (_controller.ReceiveInterruptPending(_channel))
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
            set => _baudRate = _controller.SetBaudRate(_channel, value);
        }

        /// <inheritdoc/>
        public int DataBits
        {
            get => _dataBits;
            set
            {
                _controller.SetLineSettings(_channel, value, Parity, StopBits);
                _dataBits = value;
            }
        }

        /// <inheritdoc/>
        public Parity Parity
        {
            get => _parity;
            set
            {
                _controller.SetLineSettings(_channel, DataBits, value, StopBits);
                _parity = value;
            }
        }

        /// <inheritdoc/>
        public StopBits StopBits
        {
            get => _stopBits;
            set
            {
                _controller.SetLineSettings(_channel, DataBits, Parity, value);
                _stopBits = value;
            }
        }

        private void Initialize(int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            _controller.Reset();
            _controller.EnableFifo(_channel);
            _baudRate = _controller.SetBaudRate(_channel, baudRate);
            _controller.SetLineSettings(_channel, dataBits, parity, stopBits);
        }

        private void InitializeRS485(bool invertDE)
        {
            _controller.EnableRS485(_channel, invertDE);
        }

        /// <summary>
        /// Try to read all bytes from the FIFO buffer.
        /// </summary>
        internal void ReadAllIrqFifo()
        {
            if (_irqReadBuffer == null) return;
            int totalRead = 0;
            int count = _controller.GetReadFifoCount(_channel);     // How may bytes to read.
            while (count > 0)
            {
                for (int i = 0; i < count; i++)
                    _irqReadBuffer.Write((byte)_controller.ReadByte(_channel));
                totalRead += count;
                count = _controller.GetReadFifoCount(_channel);     // Check that we're all done. To make sure IRQ is reset.

                byte lsr = _controller.ReadChannelRegister(Registers.LSR, _channel);
                if ((lsr & RegisterBits.LSR_OVERRUN_ERROR) > 0)
                {
                    _irqReadBuffer.WriteString("[BUFFER OVERRUN]");     // Not sure to keep this, but nice when debugging.
                    BufferOverrun?.Invoke(this, EventArgs.Empty);
                }
            }
            if (totalRead > 0)
            {
                Resolver.Log.Info($"---> ReadAllIrqFifo: Channel {_channel} port read {totalRead} bytes");
            }
        }

        /// <inheritdoc/>
        public int ReadByte()
        {
            if (_irqReadBuffer == null)
            {
                // The normal way...
                // check if data is available
                if (!_controller.IsFifoDataAvailable(_channel))
                {
                    return -1;
                }

                // read the data
                return _controller.ReadByte(_channel);
            }
            else
            {
                // IRQ fast read....
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
                var available = _controller.GetReadFifoCount(_channel);

                // read either the available or count, whichever is less, unless available is 0, in which case we wait until timeout
                while (available == 0)
                {
                    Thread.Sleep(10);
                    available = _controller.GetReadFifoCount(_channel);

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
                    buffer[i + offset] = _controller.ReadByte(_channel);
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
                var available = _controller.GetReadFifoCount(_channel);
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
                for (int i = 0; i < available; i++)
                {
                    buffer[i] = _irqReadBuffer.Read();
                }
                return buffer;
            }
        }

        /// <inheritdoc/>
        public void ClearReceiveBuffer()
        {
            _controller.ResetReadFifo(_channel);
            if (_irqReadBuffer != null)
                _irqReadBuffer.Clear();
        }

        /// <inheritdoc/>
        public void Close()
        {
            IsOpen = false;
            if (_irqReadBuffer != null)
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
            IsOpen = true;
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
            while (!_controller.IsTransmitHoldingRegisterEmpty(_channel))
            {
                Thread.Sleep(10);
            }

            // write until we're either written all or the THR is full
            var available = _controller.GetWriteFifoSpace(_channel);

            while (remaining > 0)
            {
                // make sure THR is empty
                while (available <= 0)
                {
                    Thread.Sleep(10);
                    available = _controller.GetWriteFifoSpace(_channel);

                    if (timeout > 0)
                    {
                        if (Environment.TickCount > timeout)
                        {
                            throw new TimeoutException();
                        }
                    }
                }

                _controller.WriteByte(_channel, buffer[index]);
                index++;
                available--;
                remaining--;

                if (available == 0)
                {
                    available = _controller.GetWriteFifoSpace(_channel);
                }
            }

            return count;
        }
    }
}