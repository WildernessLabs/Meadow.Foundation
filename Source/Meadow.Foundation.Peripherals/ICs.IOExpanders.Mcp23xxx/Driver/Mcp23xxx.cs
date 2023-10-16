using Meadow.Hardware;
using Meadow.Units;
using Meadow.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a MCP2xxx port expander
    /// </summary>
    public abstract partial class Mcp23xxx : IDigitalInputOutputController, ISpiPeripheral, II2cPeripheral, IDigitalInterruptController
    {
        /// <summary> 
        /// Raised when the value of any pin configured for input interrupts changes
        /// This provides raw port state data from the MCP23xxx
        /// It's highly recommended to prefer using the events exposed on the digital input ports instead.
        /// </summary>
        public event EventHandler<IOExpanderInputChangedEventArgs>? InputChanged = null;

        /// <summary>
        /// The number of IO pins available on the device
        /// </summary>
        public abstract int NumberOfPins { get; }

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new(375, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => (mcpDevice as ISpiCommunications).BusSpeed;
            set => (mcpDevice as ISpiCommunications).BusSpeed = value;
        }

        /// <summary>
        /// The default SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

        /// <summary>
        /// The SPI bus mode for the device
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode
        {
            get => (mcpDevice as ISpiCommunications).BusMode;
            set => (mcpDevice as ISpiCommunications).BusMode = value;
        }

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        private readonly IByteCommunications mcpDevice;
        private IDigitalOutputPort resetPort;
        private IDigitalInterruptPort? interruptPort;

        private IDictionary<IPin, DigitalInterruptPort> interruptPorts;

        private byte ioDirA, ioDirB;
        private byte olatA, olatB;

        /// <summary>
        /// object for using lock() to do thread sync
        /// </summary>
        protected object _lock = new();

        /// <summary>
        /// Mcpxxx base class constructor
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="interruptPort">Optional interrupt port, needed for input interrupts (pins 1-8)</param>
        /// <param name="resetPort">Optional Meadow output port used to reset the mcp expander</param>
        protected Mcp23xxx(II2cBus i2cBus, byte address,
            IDigitalInterruptPort? interruptPort = null, IDigitalOutputPort? resetPort = null)
        {
            mcpDevice = new I2cCommunications(i2cBus, address);
            Initialize(interruptPort, resetPort);
        }

        /// <summary>
        /// Mcpxxx base class constructor
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        /// <param name="interruptPort">Optional interrupt port, needed for input interrupts (pins 1-8)</param>
        /// <param name="resetPort">Optional Meadow output port used to reset the mcp expander</param>
        protected Mcp23xxx(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalInterruptPort? interruptPort = null,
            IDigitalOutputPort? resetPort = null)
        {
            mcpDevice = new SpiCommunications(spiBus, chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);
            Initialize(interruptPort, resetPort);
        }

        /// <summary>
        /// Initializes the Mcp23xxx
        /// </summary>
        /// <param name="interruptPort">optional interrupt port, needed for input interrupts (pins 1-8)</param>
        /// <param name="resetPort">Optional Meadow output port used to reset the mcp expander</param>
        private void Initialize(IDigitalInterruptPort? interruptPort = null,
                        IDigitalOutputPort? resetPort = null)
        {
            if (resetPort != null)
            {
                this.resetPort = resetPort;
                ResetMcp();
            }

            this.interruptPort = interruptPort;

            interruptPorts = new Dictionary<IPin, DigitalInterruptPort>();

            mcpDevice.WriteRegister(MapRegister(Registers.IODIR_IODirection, PortBank.A), 0xFF);
            mcpDevice.WriteRegister(MapRegister(Registers.GPIO, PortBank.A), 0x00);
            mcpDevice.WriteRegister(MapRegister(Registers.IPOL_InputPolarity, PortBank.A), 0x00);
            mcpDevice.WriteRegister(MapRegister(Registers.GPINTEN_InterruptOnChange), 0x00);

            if (NumberOfPins == 16)
            {   //write the following registers as well (assume device is interleaving)
                mcpDevice.WriteRegister(MapRegister(Registers.IODIR_IODirection, PortBank.B), 0xFF);
                mcpDevice.WriteRegister(MapRegister(Registers.GPIO, PortBank.B), 0x00);
                mcpDevice.WriteRegister(MapRegister(Registers.IPOL_InputPolarity, PortBank.B), 0x00);
                mcpDevice.WriteRegister(MapRegister(Registers.GPINTEN_InterruptOnChange, PortBank.B), 0x00);
            }

            // save our state
            ioDirA = ioDirB = 0xFF;
            olatA = olatB = 0x00;

            if (interruptPort != null)
            {
                bool intHigh = true;

                if (interruptPort.Resistor == ResistorMode.InternalPullUp ||
                   interruptPort.Resistor == ResistorMode.ExternalPullUp)
                {
                    intHigh = false;
                }

                byte iocon = 0x00;
                iocon = BitHelpers.SetBit(iocon, 0x01, intHigh); //set interrupt pin to active high (true), low (false)
                iocon = BitHelpers.SetBit(iocon, 0x02, false); //don't set interrupt to open drain (should be the default)

                mcpDevice.WriteRegister(MapRegister(Registers.IOCON_IOConfiguration), iocon);

                if (NumberOfPins == 16)
                {
                    mcpDevice.WriteRegister(MapRegister(Registers.IOCON_IOConfiguration, PortBank.B), iocon);
                }
            }

            // TODO: more interrupt 
            // check the interrupt mode and make sure it's correct
            // raise an exception if not. also, doc in constructor what we expect from an interrupt port
            if (interruptPort != null)
            {
                interruptPort.Changed += InterruptPortChanged;
            }
        }

        /// <summary>
        /// Reset the MCPxxxx expander
        /// Requires using a reset port
        /// </summary>
        public void ResetMcp()
        {
            if (resetPort == null)
            {
                throw new Exception("You must provide a reset port to reset the MCPxxxx");
            }
            resetPort.State = false;
            Thread.Sleep(10);
            resetPort.State = true;
        }

        private void InterruptPortChanged(object sender, DigitalPortResult e)
        {
            if (interruptPorts.Count == 0 && InputChanged == null)
            {
                return;
            }

            // determine which pin caused the interrupt
            byte interruptFlag = mcpDevice.ReadRegister(MapRegister(Registers.INTF_InterruptFlag, PortBank.A));
            byte currentStates = mcpDevice.ReadRegister(MapRegister(Registers.GPIO, PortBank.A));
            byte currentStatesB = 0;

            if (NumberOfPins == 16)
            {
                currentStatesB = mcpDevice.ReadRegister(MapRegister(Registers.GPIO, PortBank.B));
            }

            bool state;

            foreach (var port in interruptPorts)
            {   //looks ugly but it's correct
                if (GetPortBankForPin(port.Key) == PortBank.A)
                {
                    state = BitHelpers.GetBitValue(currentStates, (byte)port.Key.Key);
                }
                else
                {
                    state = BitHelpers.GetBitValue(currentStatesB, (byte)((byte)port.Key.Key - 8));
                }
                port.Value.Update(state);
            }

            InputChanged?.Invoke(this, new IOExpanderInputChangedEventArgs(interruptFlag, (ushort)((currentStatesB << 8) | currentStates)));
        }

        /// <summary>
        /// Checks if a pin exists on the Mcpxxxxx
        /// </summary>
        protected abstract bool IsValidPin(IPin pin);

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="initialState">Whether the pin is initially high or low</param>
        /// <param name="outputType">The output type</param>
        /// <returns>IDigitalOutputPort</returns>
        public virtual IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType outputType = OutputType.OpenDrain)
        {
            if (IsValidPin(pin))
            {
                var portBank = GetPortBankForPin(pin);
                var bitIndex = (byte)((byte)pin.Key % 8);

                // setup the port on the device for output
                PreValidatedSetPortDirection(pin, portBank, bitIndex, PortDirectionType.Output);

                // create the port model object
                var port = new DigitalOutputPort(pin, initialState);

                port.SetPinState += (_pin, state) => PreValidatedWriteToPort(pin, portBank, bitIndex, state);

                return port;
            }
            throw new Exception("Pin is out of range");
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="resistorMode">The port resistor mode</param>
        /// <returns>IDigitalInputPort</returns>
        public IDigitalInputPort CreateDigitalInputPort(IPin pin, ResistorMode resistorMode = ResistorMode.Disabled)
        {
            if (IsValidPin(pin))
            {
                var port = new DigitalInputPort(pin, resistorMode);
                ConfigureMcpInputPort(pin, resistorMode == ResistorMode.InternalPullUp, InterruptMode.None);
                return port;
            }
            throw new Exception("Pin is out of range");
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="interruptMode">The port interrupt mode</param>
        /// <param name="resistorMode">The port resistor mode</param>
        /// <returns>IDigitalInterruptPort</returns>
        public IDigitalInterruptPort CreateDigitalInterruptPort(
            IPin pin,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled)
        {
            return CreateDigitalInterruptPort(pin, interruptMode, resistorMode, TimeSpan.Zero, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="interruptMode">The port interrupt mode</param>
        /// <param name="resistorMode">The port resistor mode</param>
        /// <param name="debounceDuration">The debounce duration</param>
        /// <returns>IDigitalInterruptPort</returns>
        public IDigitalInterruptPort CreateDigitalInterruptPort(
            IPin pin,
            InterruptMode interruptMode,
            ResistorMode resistorMode,
            TimeSpan debounceDuration)
        {
            return CreateDigitalInterruptPort(pin, interruptMode, resistorMode, debounceDuration, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="interruptMode">The port interrupt mode</param>
        /// <param name="resistorMode">The port resistor mode</param>
        /// <param name="debounceDuration">The debounce duration</param>
        /// <param name="glitchDuration">The glitch duration - not configurable on Mcpxxxx</param>
        /// <returns>IDigitalInterruptPort</returns>
        public IDigitalInterruptPort CreateDigitalInterruptPort(
            IPin pin,
            InterruptMode interruptMode,
            ResistorMode resistorMode,
            TimeSpan debounceDuration,
            TimeSpan glitchDuration)
        {
            if (interruptPort == null)
            {
                throw new Exception("Must provide an interrupt pin for the Mcp23xxx to create digital interrupt ports");
            }

            if (IsValidPin(pin))
            {
                if (resistorMode == ResistorMode.InternalPullDown)
                {
                    throw new Exception("Pull-down resistor mode is not supported");
                }

                ConfigureMcpInputPort(pin, resistorMode == ResistorMode.InternalPullUp, interruptMode);

                var port = new DigitalInterruptPort(pin, interruptMode, resistorMode)
                {
                    DebounceDuration = debounceDuration,
                };

                interruptPorts.Add(pin, port);
                return port;
            }
            throw new Exception("Pin is out of range");
        }

        private void SetRegisterBit(byte register, int bit)
        {
            if (bit > 7 || bit < 0) { throw new ArgumentOutOfRangeException(); }

            var value = mcpDevice.ReadRegister(register);
            value |= (byte)(1 << bit);
            mcpDevice.WriteRegister(register, value);
        }

        private void ClearRegisterBit(byte register, int bit)
        {
            if (bit > 7 || bit < 0) { throw new ArgumentOutOfRangeException(); }
            var value = mcpDevice.ReadRegister(register);
            value &= (byte)~(1 << bit);
            mcpDevice.WriteRegister(register, value);
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
                var portBank = GetPortBankForPin(pin);
                var bitIndex = (byte)(((byte)pin.Key) % 8);
                PreValidatedSetPortDirection(pin, portBank, bitIndex, direction);
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
        }

        /// <summary>
        /// Sets the direction of a port using pre-cached information. This overload
        /// assumes the pin has been pre-verified as valid.
        /// </summary>
        private void PreValidatedSetPortDirection(IPin pin, PortBank portBank, byte bitIndex, PortDirectionType direction)
        {
            // if it's already configured, return (1 = input, 0 = output)
            var ioDir = portBank == PortBank.A ? ioDirA : ioDirB;
            var register = MapRegister(Registers.IODIR_IODirection, portBank);

            if (direction == PortDirectionType.Input)
            {
                if (BitHelpers.GetBitValue(ioDir, (byte)pin.Key)) { return; }
            }
            else
            {
                if (!BitHelpers.GetBitValue(ioDir, (byte)pin.Key)) { return; }
            }

            ref var ioDirLatch = ref GetIoDirLatch(portBank);
            ioDirLatch = BitHelpers.SetBit(ioDirLatch, bitIndex, (byte)direction);
            mcpDevice.WriteRegister(register, ioDirLatch);
        }

        /// <summary>
        /// Configure the hardware port settings on the MCP23xxx
        /// </summary>
        /// <param name="pin">The MCP pin associated with the port</param>
        /// <param name="enablePullUp">Enable the internal pull-up if true</param>
        /// <param name="interruptMode">Interrupt mode of port</param>
        /// <exception cref="Exception">Throw exception if pin is out of range</exception>
        private void ConfigureMcpInputPort(IPin pin, bool enablePullUp = false, InterruptMode interruptMode = InterruptMode.None)
        {
            if (IsValidPin(pin))
            {
                // set the port direction
                SetPortDirection(pin, PortDirectionType.Input);

                var bank = GetPortBankForPin(pin);
                byte bitIndex = (byte)(((byte)pin.Key) % 8);

                var gppu = mcpDevice.ReadRegister(MapRegister(Registers.GPPU_PullupResistorConfiguration, bank));
                gppu = BitHelpers.SetBit(gppu, bitIndex, enablePullUp);
                mcpDevice.WriteRegister(MapRegister(Registers.GPPU_PullupResistorConfiguration, bank), gppu);

                if (interruptMode != InterruptMode.None)
                {   // we don't set DEFVAL or INTCON because we want interrupts raised for both directions
                    // interrupt on change (raise an interrupt on the interrupt pin on state change)
                    SetRegisterBit(MapRegister(Registers.GPINTEN_InterruptOnChange, bank), bitIndex);
                }
                else
                {
                    ClearRegisterBit(MapRegister(Registers.GPINTEN_InterruptOnChange, bank), bitIndex);
                }
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
        /// </summary>
        /// <param name="pin">The pin to write to.</param>
        /// <param name="value">The value to write. True for high, false for low.</param>
        public void WriteToPort(IPin pin, bool value)
        {
            if (!IsValidPin(pin))
            {
                throw new Exception("Pin is out of range");
            }

            var bank = GetPortBankForPin(pin);
            var bitIndex = (byte)(((byte)pin.Key) % 8);
            PreValidatedWriteToPort(pin, bank, bitIndex, value);
        }

        /// <summary>
        /// Gets the value of a particular port. If the port is currently configured
        /// as an output, this will change the configuration.
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        public bool ReadPort(IPin pin)
        {
            if (IsValidPin(pin))
            {
                // if the pin isn't set for input, configure it
                SetPortDirection(pin, PortDirectionType.Input);

                var bank = GetPortBankForPin(pin);

                // update our GPIO values
                var gpio = mcpDevice.ReadRegister(MapRegister(Registers.GPIO, bank));

                // return the value on that port
                return BitHelpers.GetBitValue(gpio, (byte)pin.Key);
            }
            throw new Exception("Pin is out of range");
        }

        /// <summary>
        /// Outputs a byte value across all of the pins by writing directly 
        /// to the output latch (OLAT) register
        /// </summary>
        /// <param name="mask"></param>
        public void WriteToPorts(byte mask)
        {   // set all IO to output
            if (ioDirA != 0)
            {
                ioDirA = 0;
                mcpDevice.WriteRegister(MapRegister(Registers.IODIR_IODirection, PortBank.A), ioDirA);
            }
            // write the output
            olatA = mask;
            mcpDevice.WriteRegister(MapRegister(Registers.OutputLatch, PortBank.A), olatA);

            if (NumberOfPins == 16)
            {
                if (ioDirB != 0)
                {
                    ioDirB = 0;
                    mcpDevice.WriteRegister(MapRegister(Registers.IODIR_IODirection, PortBank.B), ioDirB);
                }
                // write the output
                olatB = mask;
                mcpDevice.WriteRegister(MapRegister(Registers.OutputLatch, PortBank.B), olatB);
            }
        }

        /// <summary>
        /// Reads a byte value from all of the pins. little-endian; the least
        /// significant bit is the value of GP0. So a byte value of 0x60, or
        /// 0110 0000, means that pins GP5 and GP6 are high.
        /// </summary>
        /// <returns>A little-endian byte mask of the pin values.</returns>
        public byte ReadFromPorts(PortBank bank = PortBank.A)
        {
            byte ioDir;
            if (bank == PortBank.A)
            {   // set all IO to input
                if (ioDirA != 1) { ioDirA = 1; }
                ioDir = ioDirA;
            }
            else
            {   // set all IO to input
                if (ioDirB != 1) { ioDirB = 1; }
                ioDir = ioDirB;
            }
            mcpDevice.WriteRegister(MapRegister(Registers.IODIR_IODirection, bank), ioDir);

            // read the input
            var gpio = mcpDevice.ReadRegister(MapRegister(Registers.GPIO, bank));
            return gpio;
        }

        /// <summary>
        /// Sets the pin back to an input
        /// </summary>
        /// <param name="pin"></param>
        protected void ResetPin(IPin pin) => SetPortDirection(pin, PortDirectionType.Input);

        /// <summary>
        /// Get Pin by name
        /// </summary>
        /// <param name="pinName">The pin name</param>
        /// <returns>IPin object if found</returns>
        public abstract IPin GetPin(string pinName);

        /// <summary>
        /// Gets the mapped address for a register.
        /// </summary>
        /// <param name="address">The register address</param>
        /// <param name="port">The bank of I/O ports used with the register</param>
        /// <param name="bankStyle">The bank style that determines how the register addresses are grouped</param>
        /// <returns>The address of the register</returns>
        private byte MapRegister(byte address, PortBank port = PortBank.A, PortBankType bankStyle = PortBankType.Segregated)
        {
            // There is no mapping for 8 pin io expanders
            if (NumberOfPins == 8) { return address; }

            if (bankStyle == PortBankType.Segregated)
            {   // Registers for each bank are interleaved 
                // (IODIRA = 0x00, IODIRB = 0x01, IPOLA = 0x02, IPOLB = 0x03, ...)
                address *= 2; //double the address
                return port == PortBank.A ? address : ++address; //add 1 for PortB
            }
            // Registers for each bank are separated
            // (IODIRA = 0x00, ... OLATA = 0x0A, IODIRB = 0x10, ... OLATB = 0x1A)
            return port == PortBank.A ? address : address += 0x10;
        }

        /// <summary>
        /// Sets a particular pin's value. If that pin is not 
        /// in output mode, this method will first set its 
        /// mode to output.
        /// 
        /// This overload takes in cached details that are assumed
        /// to be accurate for better performance.
        /// </summary>
        private void PreValidatedWriteToPort(IPin pin, PortBank portBank, byte bitIndex, bool value)
        {
            lock (_lock)
            {
                PreValidatedSetPortDirection(pin, portBank, bitIndex, PortDirectionType.Output);
                var register = MapRegister(Registers.OutputLatch, portBank);
                ref var latch = ref GetOutputLatch(portBank);

                // update the output latch
                latch = BitHelpers.SetBit(latch, bitIndex, value);

                // write to the output latch (actually does the output setting)
                mcpDevice.WriteRegister(register, latch);
            }
        }

        private ref byte GetOutputLatch(PortBank portBank)
        {
            switch (portBank)
            {
                case PortBank.A: return ref olatA;
                case PortBank.B: return ref olatB;
                default:
                    throw new NotSupportedException(portBank.ToString());
            }
        }

        private ref byte GetIoDirLatch(PortBank portBank)
        {
            switch (portBank)
            {
                case PortBank.A: return ref ioDirA;
                case PortBank.B: return ref ioDirB;
                default:
                    throw new NotSupportedException(portBank.ToString());
            }
        }

        private PortBank GetPortBankForPin(IPin pin)
        {   //hard coded ... verify in Mcp23x1x.PinDefinitions.cs
            if ((byte)pin.Key >= 8)
            {
                return PortBank.B;
            }
            return PortBank.A;
        }
    }
}