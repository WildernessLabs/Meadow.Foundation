using Meadow.Hardware;
using Meadow.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a MCP2xxx port expander
    /// </summary>
    abstract partial class Mcp23xxx : IDigitalInputOutputController
    {
        /// <summary> 
        /// Raised when the value of any pin configured for input interrupts changes
        /// This provides raw port state data from the MCP23xxx
        /// It's highly recommended to prefer using the events exposed on the digital input ports instead.
        /// </summary>
        public event EventHandler<IOExpanderInputChangedEventArgs> InputChanged = delegate { };

        /// <summary>
        /// The number of IO pins avaliable on the device
        /// </summary>
        public abstract int NumberOfPins { get; }

        private readonly IMcpDeviceComms mcpDevice;
        private readonly IDigitalInputPort interruptPort;
        private readonly IDigitalOutputPort resetPort;
        private readonly IDictionary<IPin, DigitalInputPort> inputPorts;

        // state
        byte ioDirA, ioDirB;
        byte olatA, olatB;

        /// <summary>
        /// object for using lock() to do thread sync
        /// </summary>
        protected object _lock = new object();

        /// <summary>
        /// Mcpxxx base class contructor
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="interruptPort">Optional interupt port, needed for input interrupts (pins 1-8)</param>
        /// <param name="resetPort">Optional Meadow output port used to reset the mcp expander</param>
        protected Mcp23xxx(II2cBus i2cBus, byte address,
            IDigitalInputPort interruptPort = null, IDigitalOutputPort resetPort = null) :
            this(new I2cMcpDeviceComms(i2cBus, address), interruptPort, resetPort) // use the internal constructor that takes an IMcpDeviceComms
        { }

        /// <summary>
        /// Mcpxxx base class contructor
        /// </summary>
        /// <param name="spiBus">The SPI bus</param>
        /// <param name="chipSelectPort">Chip select port</param>
        /// <param name="interruptPort">Optional interupt port, needed for input interrupts (pins 1-8)</param>
        /// <param name="resetPort">Optional Meadow output port used to reset the mcp expander</param>
        protected Mcp23xxx(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalInputPort interruptPort = null,
            IDigitalOutputPort resetPort = null) :
            this(new SpiMcpDeviceComms(spiBus, chipSelectPort), interruptPort, resetPort) // use the internal constructor that takes an IMcpDeviceComms
        { }

        /// <summary>
        /// Mcp23xxx base class
        /// </summary>
        /// <param name="device"></param>
        /// <param name="interruptPort">optional interupt port, needed for input interrupts (pins 1-8)</param>
        /// <param name="resetPort">Optional Meadow output port used to reset the mcp expander</param>
        internal Mcp23xxx(IMcpDeviceComms device,
                          IDigitalInputPort interruptPort = null,
                          IDigitalOutputPort resetPort = null)
        {
            if (resetPort != null)
            {   //disable and enable the mcp before initializing
                this.resetPort = resetPort;
                ResetMcp();
            }

            // TODO: more interrupt 
            // check the interrupt mode and make sure it's correct
            // raise an exception if not. also, doc in constructor what we expect from an interrupt port
            if (interruptPort != null)
            {
                this.interruptPort = interruptPort;
                interruptPort.Changed += InterruptPortChanged;
            }

            inputPorts = new Dictionary<IPin, DigitalInputPort>();
            mcpDevice = device;

            Initialize();
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
            // determine which pin caused the interrupt
            byte interruptFlag = mcpDevice.ReadRegister(MapRegister(Registers.INTF_InterruptFlag, PortBank.A));
            byte currentStates = mcpDevice.ReadRegister(MapRegister(Registers.GPIO, PortBank.A));
            byte currentStatesB = 0;

            if (NumberOfPins == 16)
            {
                currentStatesB = mcpDevice.ReadRegister(MapRegister(Registers.GPIO, PortBank.B));
            }
            bool state;

            foreach (var port in inputPorts)
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
            InputChanged?.Invoke(this, new IOExpanderInputChangedEventArgs(interruptFlag, (ushort)(currentStatesB << 8 | currentStates)));
        }

        /// <summary>
        /// Initializes the Mcp23xxx
        /// </summary>
        protected virtual void Initialize()
        {
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
            {   // setup the port on the device for output
                SetPortDirection(pin, PortDirectionType.Output);
                // create the port model object
                var port = new DigitalOutputPort(pin, initialState);

                port.SetPinState += (pin, state) => WriteToPort(pin, state);

                return port;
            }
            throw new Exception("Pin is out of range");
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <returns>IDigitalInputPort</returns>
        public IDigitalInputPort CreateDigitalInputPort(IPin pin)
        {
            return CreateDigitalInputPort(pin, InterruptMode.None, ResistorMode.Disabled, TimeSpan.Zero, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="interruptMode">The port interrupt mode</param>
        /// <param name="resistorMode">The port resistor mode</param>
        /// <returns>IDigitalInputPort</returns>
        public IDigitalInputPort CreateDigitalInputPort(
            IPin pin,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled)
        {
            return CreateDigitalInputPort(pin, interruptMode, resistorMode, TimeSpan.Zero, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="interruptMode">The port interrupt mode</param>
        /// <param name="resistorMode">The port resistor mode</param>
        /// <param name="debounceDuration">The debounce duration</param>
        /// <returns>IDigitalInputPort</returns>
        public IDigitalInputPort CreateDigitalInputPort(
            IPin pin,
            InterruptMode interruptMode,
            ResistorMode resistorMode,
            TimeSpan debounceDuration)
        {
            return CreateDigitalInputPort(pin, interruptMode, resistorMode, debounceDuration, TimeSpan.Zero);
        }

        /// <summary>
        /// Creates a new DigitalInputPort using the specified pin
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="interruptMode">The port interrupt mode</param>
        /// <param name="resistorMode">The port resistor mode</param>
        /// <param name="debounceDuration">The debounce duration</param>
        /// <param name="glitchDuration">The clitch duration - not configurable on Mcpxxxx</param>
        /// <returns>IDigitalInputPort</returns>
        public IDigitalInputPort CreateDigitalInputPort(
            IPin pin,
            InterruptMode interruptMode,
            ResistorMode resistorMode,
            TimeSpan debounceDuration,
            TimeSpan glitchDuration)
        {
            if (IsValidPin(pin))
            {
                var portBank = GetPortBankForPin(pin);
                byte bitIndex = (byte)(((byte)pin.Key) % 8);

                if (resistorMode == ResistorMode.InternalPullDown)
                {
                    throw new Exception("Pull-down resistor mode is not supported.");
                }

                ConfigureMcpInputPort(pin, resistorMode == ResistorMode.InternalPullUp, interruptMode);

                var port = new DigitalInputPort(pin, interruptMode, resistorMode)
                {
                    DebounceDuration = debounceDuration,
                };

                if (interruptPort == null)
                {   //no interrupts so we'll pull the state 
                    port.UpdateState = (pin) => ReadPort(pin);
                    // INTEN = 0
                    ClearRegisterBit(MapRegister(Registers.GPINTEN_InterruptOnChange, portBank), bitIndex);
                }
                else
                {
                    switch (port.InterruptMode)
                    {
                        case InterruptMode.EdgeRising:
                            // INTCON = 1
                            // DEFVAL = 0
                            SetRegisterBit(MapRegister(Registers.INTCON_InterruptControl, portBank), bitIndex);
                            ClearRegisterBit(MapRegister(Registers.DEFVAL_DefaultComparisonValue, portBank), bitIndex);
                            break;
                        case InterruptMode.EdgeFalling:
                            // INTCON = 1
                            // DEFVAL = 1
                            SetRegisterBit(MapRegister(Registers.INTCON_InterruptControl, portBank), bitIndex);
                            SetRegisterBit(MapRegister(Registers.DEFVAL_DefaultComparisonValue, portBank), bitIndex);
                            break;
                        case InterruptMode.EdgeBoth:
                            // INTCON = 0
                            ClearRegisterBit(MapRegister(Registers.INTCON_InterruptControl, portBank), bitIndex);
                            break;
                    }

                    // GPINTEN = 1
                    SetRegisterBit(MapRegister(Registers.GPINTEN_InterruptOnChange, portBank), bitIndex);
                }

                inputPorts.Add(pin, port);
                return port;
            }
            throw new Exception("Pin is out of range");
        }

        private void SetRegisterBit(byte register, int bit)
        {
            if (bit >= 7 || bit < 0) { throw new ArgumentOutOfRangeException(); }

            var value = mcpDevice.ReadRegister(register);
            value |= (byte)(1 << bit);
            mcpDevice.WriteRegister(register, value);
        }

        private void ClearRegisterBit(byte register, int bit)
        {
            if (bit >= 7 || bit < 0) { throw new ArgumentOutOfRangeException(); }
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
            {   // if it's already configured, return (1 = input, 0 = output)
                byte ioDir = GetPortBankForPin(pin) == PortBank.A ? ioDirA : ioDirB;

                if (direction == PortDirectionType.Input)
                {
                    if (BitHelpers.GetBitValue(ioDir, (byte)pin.Key)) { return; }
                }
                else
                {
                    if (!BitHelpers.GetBitValue(ioDir, (byte)pin.Key)) { return; }
                }

                byte bitIndex = (byte)(((byte)pin.Key) % 8);

                // set the IODIR bit and write the setting
                if (GetPortBankForPin(pin) == PortBank.A)
                {
                    ioDirA = BitHelpers.SetBit(ioDirA, bitIndex, (byte)direction);
                    mcpDevice.WriteRegister(MapRegister(Registers.IODIR_IODirection, PortBank.A), ioDirA);
                }
                else
                {
                    ioDirB = BitHelpers.SetBit(ioDirB, bitIndex, (byte)direction);
                    mcpDevice.WriteRegister(MapRegister(Registers.IODIR_IODirection, PortBank.B), ioDirB);
                }
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
        }

        /// <summary>
        /// Configure the hardware port settings on the MCP23xxx
        /// </summary>
        /// <param name="pin">The MCP pin associated with the port</param>
        /// <param name="enablePullUp">Enable the internal pullup if true</param>
        /// <param name="interruptMode">Interrupt mode of port</param>
        /// <exception cref="Exception">Throw execption if pin is out of range</exception>
        void ConfigureMcpInputPort(IPin pin, bool enablePullUp = false, InterruptMode interruptMode = InterruptMode.None)
        {
            if (IsValidPin(pin))
            {
                // set the port direction
                SetPortDirection(pin, PortDirectionType.Input);

                var bank = GetPortBankForPin(pin);

                var gppu = mcpDevice.ReadRegister(MapRegister(Registers.GPPU_PullupResistorConfiguration, bank));

                byte bitIndex = (byte)(((byte)pin.Key) % 8);
                gppu = BitHelpers.SetBit(gppu, bitIndex, enablePullUp);
                mcpDevice.WriteRegister(MapRegister(Registers.GPPU_PullupResistorConfiguration, bank), gppu);

                if (interruptMode != InterruptMode.None)
                {   // we don't set DEFVAL or INTCON because we want interrupts raised for both directions
                    // interrupt on change (raise an interrupt on the interrupt pin on state change)
                    byte gpinten = mcpDevice.ReadRegister(MapRegister(Registers.GPINTEN_InterruptOnChange, bank));

                    gpinten = BitHelpers.SetBit(gpinten, bitIndex, true);
                    mcpDevice.WriteRegister(MapRegister(Registers.GPINTEN_InterruptOnChange, bank), gpinten);
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
            if (IsValidPin(pin))
            {
                lock (_lock)
                {
                    // if the pin isn't configured for output, configure it
                    SetPortDirection(pin, PortDirectionType.Output);

                    var bank = GetPortBankForPin(pin);
                    byte bitIndex = (byte)(((byte)pin.Key) % 8);

                    if (bank == PortBank.A)
                    {   // update our output latch 
                        olatA = BitHelpers.SetBit(olatA, bitIndex, value);
                        // write to the output latch (actually does the output setting)
                        mcpDevice.WriteRegister(MapRegister(Registers.OutputLatch, PortBank.A), olatA);
                    }
                    else
                    {   // update our output latch 
                        olatB = BitHelpers.SetBit(olatB, bitIndex, value);
                        // write to the output latch (actually does the output setting)
                        mcpDevice.WriteRegister(MapRegister(Registers.OutputLatch, PortBank.B), olatB);
                    }
                }
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
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
        byte MapRegister(byte address, PortBank port = PortBank.A, PortBankType bankStyle = PortBankType.Segregated)
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

        PortBank GetPortBankForPin(IPin pin)
        {   //hard coded ... verify in Mcp23x1x.PinDefinitions.cs
            if (pin.Name.StartsWith("GPB"))
            {
                return PortBank.B;
            }
            return PortBank.A;
        }
    }
}