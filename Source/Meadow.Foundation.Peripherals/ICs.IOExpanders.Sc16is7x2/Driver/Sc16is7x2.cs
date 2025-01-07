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
        public Frequency OscillatorFrequency { get; private set; }
        private IDigitalInterruptPort? _irq;
        private bool _latchGpioInterrupt;

        /// <summary>
        /// 03.12.2023: Sc16is7x2 pin definitions for GPIO pins.
        /// </summary>
        public PinDefinitions Pins { get; }

        internal Sc16is7x2(Frequency oscillatorFrequency, IDigitalInterruptPort? irq, bool latchGpioInterrupt = false)
        {
            OscillatorFrequency = oscillatorFrequency;
            _irq = irq;
            _latchGpioInterrupt = latchGpioInterrupt;

            Pins = new PinDefinitions(this);

            // This has to move to GPIO init. We don't know yet if comms init will succeed.
            //if (irq != null)
            //{
            //    irq.Changed += GpioInterruptHandler;
            //}
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
            if (_irq != null && _irq.InterruptMode != InterruptMode.EdgeFalling)
            {
                Resolver.Log.Warn($"Warning: You have specified InterruptMode={_irq.InterruptMode}. SC16IS7x2 have a falling edge IRQ signal.");
            }

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

        /// <summary>
        /// Reset the device
        /// </summary>
        internal void Reset()
        {
            var value = ReadRegister(Registers.IOControl);
            value |= RegisterBits.IOCTL_RESET;
            try
            {
                WriteRegister(Registers.IOControl, value);
            }
            catch (Exception ex)
            {
                // we expect to get a NACK on this.  Very confusing
                // 06.10.2024, KW: Ignore this error. We get it even when the device works fine. ("Error code 70")
                Resolver.Log.Trace($"Ignoring SC16IS7x2 error on reset: {ex.Message}");
            }
        }

        /********************* GPIO **********************/

        private IDigitalPort[] gpioPorts = new IDigitalPort[8];

        /// <summary>
        /// Is the pin valid for this device instance
        /// </summary>
        /// <param name="pin">The IPin to validate</param>
        /// <returns>True if pin is valid</returns>
        protected bool IsValidPin(IPin pin) => Pins.AllPins.Contains(pin);

        private bool initGpioDone = false;
        private void InitGpio()
        {
            if (initGpioDone) return;

            if (_latchGpioInterrupt)
                SetRegisterBit(Registers.IOControl, RegisterBits.IOCTL_IO_LATCH);
            else
                ClearRegisterBit(Registers.IOControl, RegisterBits.IOCTL_IO_LATCH);
            ClearRegisterBit(Registers.IOControl, RegisterBits.IOCTL_GPIO_7to4);
            ClearRegisterBit(Registers.IOControl, RegisterBits.IOCTL_GPIO_3to0);

            // Set direction of all GPIO's to input
            WriteRegister(Registers.IODir, ioDir);

            // 22.04.2024, KW: This has been moved here from the constructor. Add the handler _after_ comms init is OK.
            if (_irq != null)
            {
                _irq.Changed += GpioInterruptHandler;
            }

            initGpioDone = true;
        }

        /// <summary>
        /// Get the Interrupt Identification Register (IIR) value.
        /// </summary>
        /// <returns></returns>
        public byte GetInterruptSource()
        {
            byte iir = ReadRegister(Registers.IIR);
            iir &= RegisterBits.IIR_Id0 + RegisterBits.IIR_Id1 + RegisterBits.IIR_Id2 +
                   RegisterBits.IIR_Id3 + RegisterBits.IIR_Id4 + RegisterBits.IIR_Id5;
            return iir;
        }

        byte lastInputState = 0;
        private void GpioInterruptHandler(object sender, DigitalPortResult e)
        {
            try
            {
                //byte iir = GetInterruptSource();

                //Resolver.Log.Info($"HandleGpioInterrupt. Interrupt pin state: {e.Old?.State} {e.New.State} {e.New.Time.ToString("hh:mm:ss.fffffff")} {_irq?.State}");
                byte state = ReadIoState();
                byte dirMask = (byte)~ioDir;    // Only look at the input pins
                byte masked = (byte)(state & dirMask);
                //Resolver.Log.Info($"State: {state:X2} {dirMask:X2} {masked:X2}");
                //Resolver.Log.Info($"LastState: {lastState} NewState: {state}");
                if (masked == lastInputState) return;

                byte diff = (byte)(masked ^ lastInputState);
                //Resolver.Log.Info($"GPIO state: {lastInputState} -> {state}");
                for (byte i = 0; i < 8; i++)
                {
                    if ((diff & (1 << i)) == 0) continue;   // No change on this pin
                    if (gpioPorts[i] == null) continue;     // No port defined for this pin
                    if (gpioPorts[i] is DigitalInputPort port)
                    {
                        var newState = BitHelpers.GetBitValue(state, i);
                        //Resolver.Log.Info($"Pin {i} new state: {newState}");
                        port.Update(newState);
                    }
                }
                lastInputState = masked;
            }
            catch (Exception ex)
            {
                Resolver.Log.Info($"Error in GpioInterruptHandler: {ex.Message}");
            }
        }


        /****************** INPUT PORTS ******************/

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

                var state = ReadIoState();
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
                    SetRegisterBit(Registers.IOIntEna, bitIndex);
                }
                else
                {
                    ClearRegisterBit(Registers.IOIntEna, bitIndex);
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
            WriteRegister(Registers.IODir, newIoDir);
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

            var gpio = ReadIoState();

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
                SetIoStateBit(bitIndex);
            else
                ClearIoStateBit(bitIndex);
        }


        /*********** INPUT/OUTPUT PORT HELPERS ***********/

        private void SetRegisterBit(Registers register, int bitIndex)
        {
            if (bitIndex > 7 || bitIndex < 0) { throw new ArgumentOutOfRangeException(); }

            var oldValue = ReadRegister(register);
            byte newValue = (byte)(oldValue | ((byte)(1 << bitIndex)));
            WriteRegister(register, newValue);
        }

        private void ClearRegisterBit(Registers register, int bitIndex)
        {
            if (bitIndex > 7 || bitIndex < 0) { throw new ArgumentOutOfRangeException(); }
            var oldValue = ReadRegister(register);
            var newValue = (byte)(oldValue & ((byte)~(1 << bitIndex)));
            WriteRegister(register, newValue);
        }

        // Read a register that is not channel specific.
        private byte ReadRegister(Registers register)
        {
            byte v = Comms.ReadRegister((byte)((byte)register << 3));
            return v;
        }

        // Write to a register that is not channel specific.
        private void WriteRegister(Registers register, byte value)
        {
            Comms.WriteRegister((byte)((byte)register << 3), value);
        }


        /********************* OPTIMIZATIONS **********************/

        /// <summary>
        /// Specific method to read the IOState register. This is an optimization.
        /// We don't want to slow down all other register reads with the validation
        /// for this specific register.
        /// </summary>
        /// <returns></returns>
        private byte ReadIoState()
        {
            // See page 40 of the data sheet for explanation of this
            byte v = Comms.ReadRegister((byte)Registers.IOState << 3);
            if (v == 0)
            {
                byte v2 = Comms.ReadRegister((byte)Registers.IOState << 3);
                if (v2 != v)
                {
                    v = v2;     // Fix buggy value!
                    Resolver.Log.Info($"ReadIoState: {v2:X2} (Corrected value!)");
                }
            }
            return v;
        }

        private void WriteIoState(byte value)
        {
            // see page 40 of the data sheet for explanation of this
            Comms.WriteRegister((byte)Registers.IOState << 3, value);
        }

        // This is an optimization for the GPIO ports.
        private void ClearIoStateBit(int bitIndex)
        {
            if (bitIndex > 7 || bitIndex < 0) { throw new ArgumentOutOfRangeException(); }

            var oldValue = ReadIoState();
            byte newValue = (byte)(oldValue & ((byte)~(1 << bitIndex)));
            if (oldValue != newValue)
                WriteIoState(newValue);
        }

        // This is an optimization for the IOState register.
        private void SetIoStateBit(int bitIndex)
        {
            if (bitIndex > 7 || bitIndex < 0) { throw new ArgumentOutOfRangeException(); }

            var oldValue = ReadIoState();
            byte newValue = (byte)(oldValue | ((byte)(1 << bitIndex)));
            if (oldValue != newValue)
                WriteIoState(newValue);
        }

        /*********************** NICE TO HAVE METHODS ***********************/

        private byte DebugReadChannelRegister(Registers register, Channels channel)
        {
            // see page 40 of the data sheet for explanation of this
            var subaddress = (byte)(((byte)register << 3) | ((byte)channel << 1));
            byte v = Comms.ReadRegister(subaddress);
            return v;
        }

        /// <summary>
        /// Print the content of the address space for debugging purposes.
        /// </summary>
        public void PrintAddressContent()
        {
            Resolver.Log.Info($"Register:  00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
            string a = $"Channel A: ";
            for (int r = 0; r < 16; r++)
                a += $"{DebugReadChannelRegister((Registers)r, Channels.A):X2} ";
            Resolver.Log.Info(a);
            string b = $"Channel B: ";
            for (int r = 0; r < 16; r++)
                b += $"{DebugReadChannelRegister((Registers)r, Channels.B):X2} ";
            Resolver.Log.Info(b);
        }

        /// <summary>
        /// Nice-to-have conversion from byte to binary string.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public string ByteToBinaryString(byte b)
        {
            // Format the byte as a binary string and pad it with zeroes
            string binaryString = $"0b{Convert.ToString(b, 2).PadLeft(8, '0')}";
            return binaryString;
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
    }
}
