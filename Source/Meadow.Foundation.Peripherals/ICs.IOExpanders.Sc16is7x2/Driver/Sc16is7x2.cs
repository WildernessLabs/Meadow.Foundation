using System;
using Meadow.Hardware;
using Meadow.Units;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an Sc16is7x2 SPI/I2C dual UART (with 8 GPIO's)
    /// </summary>
    public abstract partial class Sc16is7x2 : ISerialController, IDigitalInputOutputController
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
        private bool _latchGpioInterrupt;

        /// <summary>
        /// 03.12.2023: Sc16is7x2 pin definitions for GPIO pins.
        /// </summary>
        public PinDefinitions Pins { get; }

        internal Sc16is7x2(Frequency oscillatorFrequency, IDigitalInterruptPort? irq, bool latchGpioInterrupt = false)
        {
            _oscillatorFrequency = oscillatorFrequency;
            _irq = irq;
            _latchGpioInterrupt = latchGpioInterrupt;

            Pins = new PinDefinitions(this);

            if (irq != null)
            {
                irq.Changed += GpioInterruptHandler;
            }
        }

        private IByteCommunications Comms
        {
            get
            {
                if (_i2cComms != null) return _i2cComms;
                if (_spiComms != null) return _spiComms;
                throw new Exception("No comms interface found");
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
        /// <param name="readBufferSize">The software FIFO read buffer size. (Not the 64 bytes on chip FIFO)</param>
        public ISerialPort CreateSerialPort(SerialPortName portName, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int readBufferSize = 1024)
        {
            //if (_irq != null && _irq.InterruptMode != InterruptMode.EdgeRising)
            //{
            //    throw new ArgumentException("If an interrupt port is provided, it must be a rising edge interrupt");
            //}

            switch (portName.SystemName)
            {
                case "A":
                    if (_channelA == null)
                    {
                        _channelA = new Sc16is7x2Channel(this, portName.FriendlyName, Channels.A, baudRate, dataBits, parity, stopBits, irq: _irq, readBufferSize: readBufferSize);
                        return _channelA;
                    }
                    throw new PortInUseException($"{portName.FriendlyName} already in use");
                case "B":
                    if (_channelB == null)
                    {
                        _channelB = new Sc16is7x2Channel(this, portName.FriendlyName, Channels.B, baudRate, dataBits, parity, stopBits, irq: _irq, readBufferSize: readBufferSize);
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

        internal bool IsTransmitHoldingRegisterEmpty(Channels channel)
        {
            var thr = ReadChannelRegister(Registers.LSR, channel);
            return (thr & RegisterBits.LSR_THR_EMPTY) == RegisterBits.LSR_THR_EMPTY;
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

        internal void Reset()
        {
            var value = ReadChannelRegister(Registers.IOControl, Channels.Both);
            value |= RegisterBits.IOCTL_RESET;
            try
            {
                WriteChannelRegister(Registers.IOControl, Channels.Both, value);
            }
            catch
            {
                // we expect to get a NACK on this.  Very confusing
            }
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
            byte v = Comms.ReadRegister(subaddress);
            if (register == Registers.IOState)
            {
                //Resolver.Log.Info($"ReadChannelRegister: {register} {channel} {v:X2}");
                if (v == 0)
                {
                    byte v2 = Comms.ReadRegister(subaddress);
                    if (v2 != v)
                    {
                        v = v2;     // Fix buggy value!
                        Resolver.Log.Info($"ReadChannelRegister: {register} {channel} {v2:X2} (Corrected value!)");
                    }
                }
            }
            return v;
        }

        private void WriteChannelRegister(Registers register, Channels channel, byte value)
        {
            // see page 40 of the data sheet for explanation of this
            var subaddress = (byte)(((byte)register << 3) | ((byte)channel << 1));
            //if (register == Registers.IOState)
            //{
            //    Resolver.Log.Info($"WriteChannelRegister: {register} {channel} {value:X2}");
            //}
            Comms.WriteRegister(subaddress, value);
        }

        private void SetChannelRegisterBits(Registers register, Channels channel, byte value)
        {
            byte currentValue = ReadChannelRegister(register, channel);
            currentValue |= value;          // Set the bits we're going to change
            WriteChannelRegister(register, channel, currentValue);
        }

        private void ClearChannelRegisterBits(Registers register, Channels channel, byte mask)
        {
            byte currentValue = ReadChannelRegister(register, channel);
            currentValue &= (byte)~mask;          // Flip all bits in value, then AND with currentValue
            WriteChannelRegister(register, channel, currentValue);
        }


        /********************* GPIO **********************/

        private bool initDone = false;
        private void InitGpio()
        {
            if (initDone) return;

            var a = ReadChannelRegister(Registers.IER, Channels.A);
            a &= unchecked((byte)~RegisterBits.IER_SLEEP_MODE_ENABLE);
            WriteChannelRegister(Registers.IER, Channels.A, a);
            var b = ReadChannelRegister(Registers.IER, Channels.B);
            b &= unchecked((byte)~RegisterBits.IER_SLEEP_MODE_ENABLE);
            WriteChannelRegister(Registers.IER, Channels.B, b);

            byte ioControlBefore = ReadGpioRegister(Registers.IOControl);
            if (_latchGpioInterrupt)
                SetGpioRegisterBit(Registers.IOControl, RegisterBits.IOCTL_IO_LATCH);
            else
                ClearGpioRegisterBits(Registers.IOControl, RegisterBits.IOCTL_IO_LATCH);
            ClearGpioRegisterBits(Registers.IOControl, RegisterBits.IOCTL_GPIO_7to4);
            ClearGpioRegisterBits(Registers.IOControl, RegisterBits.IOCTL_GPIO_3to0);
            byte ioControlAfter = ReadGpioRegister(Registers.IOControl);
            //Resolver.Log.Info($"ioControl: {ioControlBefore} -> {ioControlAfter}");

            // Set direction of all GPIO's to input
            WriteGpioRegister(Registers.IODir, ioDir);

            initDone = true;
        }

        int debugId = 0;
        byte lastInputState = 0;
        private void GpioInterruptHandler(object sender, DigitalPortResult e)
        {
            try
            {
                //lock(this)
                {
                    //int id = debugId++; 
                    //Resolver.Log.Info($"GpioInterruptHandler... {id}");

                    //// Prioritize reading the IRQ FIFO's
                    //if (_channelA?.IsOpen ?? false)
                    //    _channelA.OnInterruptLineChanged(sender, e);
                    //if (_channelB?.IsOpen ?? false)
                    //    _channelB.OnInterruptLineChanged(sender, e);

                    byte iir = GetInterruptSource();

                    //Resolver.Log.Info($"HandleGpioInterrupt. Interrupt pin state: {e.Old?.State} {e.New.State} {e.New.Time.ToString("hh:mm:ss.fffffff")} {_irq?.State}");
                    byte state = ReadGpioRegister(Registers.IOState);
                    byte dirMask = (byte)~ioDir;  // Only look at the input pins
                    byte masked = (byte)(state & dirMask);
                    //Resolver.Log.Info($"State: {state:X2} {dirMask:X2} {masked:X2}");
                    //Resolver.Log.Info($"LastState: {lastState} NewState: {state}");
                    if (masked == lastInputState) return;

                    byte diff = (byte)(masked ^ lastInputState);
                    //Resolver.Log.Info($"GPIO state: {lastState} -> {state}");
                    for (byte i = 0; i < 8; i++)
                    {
                        if ((diff & (1 << i)) == 0) continue;    // No change on this pin
                        if (gpioPorts[i] == null) continue;    // No input port defined for this pin
                        if (gpioPorts[i] is DigitalInputPort port)
                        {
                            var newState = BitHelpers.GetBitValue(state, i);
                            port.Update(newState);
                        }
                    }
                    lastInputState = masked;

                    //PrintAddressContent();
                    //byte irqTest = ReadGpioRegister(Registers.IOIntEna);
                    //if (irqTest != irqEna)
                    //{
                    //    Resolver.Log.Info($"irqTest: {irqTest} irqEna: {irqEna}");
                    //    WriteGpioRegister(Registers.IOIntEna, irqEna);
                    //}

                    //Resolver.Log.Info($"DONE. {id}");
                }
            }
            catch (Exception ex)
            {
                Resolver.Log.Info($"Error in GpioInterruptHandler: {ex.Message}");
            }
        }

        /****************** INPUT PORTS ******************/

        private IDigitalPort[] gpioPorts = new IDigitalPort[8];

        /// <summary>
        /// Create a digital input port on a SC16IS7x2 IO expander.
        /// </summary>
        /// <param name="pin">The GPIO pin to use.</param>
        /// <param name="resistorMode">SC16IS7x2 GPIO's does not support pullup or pulldown on inputs.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IDigitalInputPort CreateDigitalInputPort(
            IPin pin, ResistorMode resistorMode = ResistorMode.Disabled)
        {
            InitGpio();

            byte pinIndex = (byte)pin.Key;
            if (gpioPorts[pinIndex] != null)
            {
                throw new Exception($"Port {pin.Name} already in use");
            }

            if (IsValidPin(pin))
            {
                if (resistorMode != ResistorMode.Disabled)
                {
                    throw new Exception("Only ResistorMode.Disabled is supported. The GPIO ports of SC16IS7x2 does not need/support external pull-up or pull-down resistors.");
                }

                var state = ReadGpioRegister(Registers.IOState);
                var initState = BitHelpers.GetBitValue(state, pinIndex);
                var port = new DigitalInputPort(pin, initState);
                gpioPorts[pinIndex] = port;
                ConfigureInputPort(pin);

                byte inputMask = (byte)~ioDir;
                lastInputState = (byte)(state & inputMask);

                return port;
            }
            throw new Exception("Pin is out of range");
        }

        byte irqEna = 0;

        /// <summary>
        /// Configure the hardware port settings on the SC16IS7x2
        /// </summary>
        /// <param name="pin">The MCP pin associated with the port</param>
        /// <exception cref="Exception">Throw exception if pin is out of range</exception>
        private void ConfigureInputPort(IPin pin)
        {
            if (IsValidPin(pin))
            {
                byte bitIndex = (byte)pin.Key;

                // Set the port direction
                PreValidatedSetPortDirection(pin, PortDirectionType.Input);

                if (_irq != null)
                {   
                    irqEna = BitHelpers.SetBit(irqEna, bitIndex, true);
                    SetGpioRegisterBit(Registers.IOIntEna, bitIndex);
                }
                else
                {
                    ClearGpioRegisterBit(Registers.IOIntEna, bitIndex);
                }
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
        }

        private byte ioDir = 0;     // All GPIO's are inputs by default

        /// <summary>
        /// Sets the direction of a port using pre-cached information.
        /// This overload assumes the pin has been pre-verified as valid.
        /// </summary>
        private void PreValidatedSetPortDirection(IPin pin, PortDirectionType direction)
        {
            byte bitIndex = (byte)pin.Key;
            // On SC16IS7x2, 0/false = input, 1/true = output)
            byte newIoDir = BitHelpers.SetBit(ioDir, bitIndex, direction == PortDirectionType.Output);
            if (newIoDir == ioDir) return;

            //Resolver.Log.Info($"newIoDir: {newIoDir}");
            WriteGpioRegister(Registers.IODir, newIoDir);
            ioDir = newIoDir;
        }

        /// <summary>
        /// Sets the direction of a port
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="direction">The port direction (input or output)</param>
        public void SetPortDirection(IPin pin, PortDirectionType direction)
        {
            if (IsValidPin(pin))
            {
                PreValidatedSetPortDirection(pin, direction);
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
        }

        /// <summary>
        /// Gets the value of a particular port.
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public bool ReadPort(IPin pin)
        {
            if (!IsValidPin(pin))
                throw new Exception("Pin is out of range");

            bool portDir = BitHelpers.GetBitValue(ioDir, (byte)pin.Key);
            // On SC16IS7x2, 0/false = input, 1/true = output
            if (portDir == true)
                throw new Exception($"Cant read from port {pin.Name}. It is not configured as input");

            var gpio = ReadGpioRegister(Registers.IOState);

            // Return the value on that port
            return BitHelpers.GetBitValue(gpio, (byte)pin.Key);
        }


        /***************** OUTPUT PORTS ******************/

        /// <summary>
        /// Create a digital output port on a SC16IS7x2 IO expander.
        /// </summary>
        /// <param name="pin">The GPIO pin to use.</param>
        /// <param name="initialState">Initial state. Either true or false.</param>
        /// <param name="outputType">Always push/pull on SC16IS7x2.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType outputType = OutputType.PushPull)
        {
            InitGpio();

            byte pinIndex = (byte)pin.Key;
            if (gpioPorts[pinIndex] != null)
            {
                throw new Exception($"Port {pin.Name} already in use");
            }

            if (outputType != OutputType.PushPull)
            {
                throw new NotImplementedException("Only OutputType.PushPull is supported on SC16IS7x2");
            }

            ConfigureOutputPort(pin, initialState);
            var port = new Sc16is7x2.DigitalOutputPort(pin, initialState, outputType);
            byte bitIndex = (byte)pin.Key;
            port.SetPinState += (_pin, state) => WriteToGpioPort(pin, state);
            gpioPorts[pinIndex] = port;

            return port;
        }

        private void ConfigureOutputPort(IPin pin, bool initialState)
        {
            if (IsValidPin(pin))
            {
                // Set the port direction
                PreValidatedSetPortDirection(pin, PortDirectionType.Output);
                WriteToGpioPort(pin, initialState);
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
        }

        /// <summary>
        /// Sets a particular pin's value. If that pin is not 
        /// in output mode, this method will first set its 
        /// mode to output.
        /// 
        /// This overload takes in cached details that are assumed
        /// to be accurate for better performance.
        /// </summary>
        private void WriteToGpioPort(IPin pin, bool value)
        {
            byte bitIndex = (byte)pin.Key;
            if (value)
                SetGpioRegisterBit(Registers.IOState, bitIndex);
            else
                ClearGpioRegisterBit(Registers.IOState, bitIndex);
        }

        /*********** INPUT/OUTPUT PORT HELPERS ***********/

        /// <summary>
        /// Get the Interrupt Identification Register (IIR) value.
        /// </summary>
        /// <returns></returns>
        public byte GetInterruptSource()
        {
            byte iir = ReadGpioRegister(Registers.IIR);
            iir &= RegisterBits.IIR_Id0 +
                   RegisterBits.IIR_Id1 +
                   RegisterBits.IIR_Id2 +
                   RegisterBits.IIR_Id3 +
                   RegisterBits.IIR_Id4 +
                   RegisterBits.IIR_Id5;
            return iir;
        }

        /// <summary>
        /// Get the interrupt source type from the IIR register value. (Interrupt Identification Register)
        /// Ref. page 14 and 24 in datasheet. Mostly for debugging.
        /// Not sure if multiple sources can be active at the same time. If so, this will need to be modified.
        /// </summary>
        /// <param name="iir">The IIR register value. (Interrupt Identification Register)</param>
        /// <returns></returns>
        public InterruptSourceType GetInterruptSourceType(byte iir)
        {
            iir &= (RegisterBits.IIR_Id0 +
                RegisterBits.IIR_Id1 +
                RegisterBits.IIR_Id2 +
                RegisterBits.IIR_Id3 +
                RegisterBits.IIR_Id4 +
                RegisterBits.IIR_Id5);
            if (iir == RegisterBits.IIR_IdNone) return InterruptSourceType.None;
            if (iir == RegisterBits.IIR_IdReceiverLineStatus) return InterruptSourceType.ReceiverLineStatus;
            if (iir == RegisterBits.IIR_IdRxTimeout) return InterruptSourceType.RxTimeout;
            if (iir == RegisterBits.IIR_IdRHR) return InterruptSourceType.RHR;
            if (iir == RegisterBits.IIR_IdTHR) return InterruptSourceType.THR;
            if (iir == RegisterBits.IIR_IdModemStatus) return InterruptSourceType.ModemStatus;
            if (iir == RegisterBits.IIR_IdGpioPins) return InterruptSourceType.GpioPins;
            if (iir == RegisterBits.IIR_IdXoff) return InterruptSourceType.Xoff;
            if (iir == RegisterBits.IIR_IdCtsRts) return InterruptSourceType.CtsRts;
            Resolver.Log.Info($"UNKNOWN INTERRUPT SOURCE (or combination): {iir}");
            return InterruptSourceType.Unknown;
        }

        /// <summary>
        /// Is the pin valid for this device instance
        /// </summary>
        /// <param name="pin">The IPin to validate</param>
        /// <returns>True if pin is valid</returns>
        protected bool IsValidPin(IPin pin) => Pins.AllPins.Contains(pin);

        private byte ReadGpioRegister(Registers register)
        {
            return ReadChannelRegister(register, Channels.Both);
        }

        private void WriteGpioRegister(Registers register, byte value)
        {
            WriteChannelRegister(register, Channels.Both, value);
        }

        private void SetGpioRegisterBits(Registers register, byte value)
        {
            SetChannelRegisterBits(register, Channels.Both, value);
        }

        private void ClearGpioRegisterBits(Registers register, byte mask)
        {
            ClearChannelRegisterBits(register, Channels.Both, mask);
        }

        private void SetGpioRegisterBit(Registers register, int bitIndex)
        {
            SetChannelRegisterBit(register, Channels.Both, bitIndex);
        }

        private void SetChannelRegisterBit(Registers register, Channels channel, int bitIndex)
        {
            if (bitIndex > 7 || bitIndex < 0) { throw new ArgumentOutOfRangeException(); }

            var oldValue = ReadChannelRegister(register, channel);
            byte newValue = (byte)(oldValue | ((byte)(1 << bitIndex)));
            WriteChannelRegister(register, channel, newValue);
        }

        private void ClearGpioRegisterBit(Registers register, int bitIndex)
        {
            ClearChannelRegisterBit(register, Channels.Both, bitIndex);
        }

        private void ClearChannelRegisterBit(Registers register, Channels channel, int bitIndex)
        {
            if (bitIndex > 7 || bitIndex < 0) { throw new ArgumentOutOfRangeException(); }
            var oldValue = ReadChannelRegister(register, channel);
            var newValue = (byte)(oldValue & ((byte)~(1 << bitIndex)));
            WriteChannelRegister(register, channel, newValue);
            //var check = ReadChannelRegister(register, channel);
            //Resolver.Log.Info($"ClearChannelRegisterBit: Old={oldValue} New={newValue} Check={check}");
        }

        /// <summary>
        /// Print the content of the address space for debugging purposes.
        /// </summary>
        public void PrintAddressContent()
        {
            Resolver.Log.Info($"Register:  00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
            string a = $"Channel A: ";
            for (int r = 0; r < 16; r++)
                a += $"{ReadChannelRegister((Registers)r, Channels.A):X2} ";
            Resolver.Log.Info(a);
            string b = $"Channel B: ";
            for (int r = 0; r < 16; r++)
                b += $"{ReadChannelRegister((Registers)r, Channels.B):X2} ";
            Resolver.Log.Info(b);
        }

        public string ByteToBinaryString(byte b)
        {
            // Format the byte as a binary string and pad it with zeroes
            string binaryString = $"0b{Convert.ToString(b, 2).PadLeft(8, '0')}";
            return binaryString;
        }
    }
}