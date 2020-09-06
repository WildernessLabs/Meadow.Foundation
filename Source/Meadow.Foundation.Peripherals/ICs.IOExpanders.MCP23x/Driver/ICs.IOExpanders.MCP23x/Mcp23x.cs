using System;
using System.Collections.Generic;
using System.Linq;
using Meadow.Foundation.ICs.IOExpanders.Device;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Abstract implementation of <see cref="IMcp23x" />.
    /// Intended to be inherited by concrete implementations.
    /// </summary>
    public abstract class Mcp23x : IIODevice, IMcp23x
    {
        /// <summary>
        /// Use this lock whenever accessing GPPU, IOCON, IODIR, GPINTEN
        /// </summary>
        protected readonly object ConfigurationLock = new object();

        /// <summary>
        /// Currently configured state of GPIO Pull-up resistors (GPPU)
        /// </summary>
        protected readonly byte[] GppuState;

        /// <summary>
        /// List of supported interrupts.
        /// </summary>
        protected readonly IList<IDigitalInputPort> Interrupts;

        /// <summary>
        /// Whether or not interrupts are supported.
        /// </summary>
        protected readonly bool InterruptSupported;

        /// <summary>
        /// Currently configured IO direction (IODIR)
        /// </summary>
        protected readonly byte[] IodirState;

        /// <summary>
        /// Whether interrupts have been configured in mirrored mode. This means a single interrupt is used for all GPIO ports.
        /// </summary>
        /// <remarks>
        /// Interrupts are mirrored unless there is more than 1 interrupt provided.
        /// </remarks>
        protected readonly bool MirroredInterrupts;

        /// <summary>
        /// Currently configured value of output latches (OLAT)
        /// </summary>
        protected readonly byte[] OlatState;

        /// <summary>
        /// Use this lock whenever reading/writing BankConfiguration
        /// </summary>
        private readonly object _bankLock = new object();

        /// <summary>
        /// The currently configured interrupt mode
        /// </summary>
        private readonly InterruptMode _interruptMode;
        
        /// <summary>
        /// The communication interface to the McpDevice
        /// </summary>
        private readonly IMcpDeviceComms _mcpDevice;

        /// <summary>
        /// Mapping of register addresses
        /// </summary>
        private readonly IMcp23RegisterMap _registerMap;

        /// <inheritdoc />
        public event EventHandler<IOExpanderMultiPortInputChangedEventArgs> InputChanged = delegate { };

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="mcpDeviceComms">The communication interface to the McpDevice</param>
        /// <param name="ports">List of ports attached to this device</param>
        /// <param name="registerMap">Mapping of register addresses</param>
        /// <param name="interrupts">
        /// List of supported interrupts. There should be either 0, 1 or N interrupts where N is the
        /// number of ports.
        /// </param>
        protected Mcp23x(
            IMcpDeviceComms mcpDeviceComms,
            IMcpGpioPorts ports,
            IMcp23RegisterMap registerMap,
            IList<IDigitalInputPort> interrupts)
        {
            Interrupts = interrupts ?? throw new ArgumentNullException(nameof(interrupts));
            Ports = ports ?? throw new ArgumentNullException(nameof(ports));
            _mcpDevice = mcpDeviceComms ?? throw new ArgumentNullException(nameof(mcpDeviceComms));
            _registerMap = registerMap ?? throw new ArgumentNullException(nameof(registerMap));

            if (!ports.Any() || ports.Any(p => p == null))
            {
                throw new ArgumentNullException(nameof(ports), "Ports cannot be null or empty.");
            }

            if (interrupts.Any(i => i == null))
            {
                throw new ArgumentNullException(nameof(interrupts), "Interrupts cannot be null.");
            }

            if (interrupts.Any())
            {
                var interruptMode = interrupts[0].InterruptMode;
                if (interruptMode == InterruptMode.None)
                {
                    throw new ArgumentException(
                        "Interrupt ports must use an InterruptMode",
                        nameof(interrupts));
                }

                if (interrupts.Any(i => i.InterruptMode != interruptMode))
                {
                    throw new ArgumentException(
                        "Interrupt ports must all use the same interrupt mode.",
                        nameof(interrupts));
                }

                _interruptMode = interruptMode;
            }
            else
            {
                _interruptMode = InterruptMode.None;
            }

            if (interrupts.Count > 1 && interrupts.Count != ports.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(interrupts),
                    interrupts.Count,
                    "There can only be 0, 1 or N interrupts, where N is the number of GPIO Ports.");
            }

            // initialize all state tracking arrays with a length equal to the number of ports
            byte[] InitState(byte initialValue)
            {
                var array = new byte[ports.Count];
                for (var i = 0; i < array.Length; i++)
                {
                    array[i] = initialValue;
                }

                return array;
            }

            // Populate initial state 
            IodirState = InitState(0xFF);
            // GpioState = InitState(0x00); // never read
            OlatState = InitState(0x00);
            GppuState = InitState(0x00);
            IoconState = 0x00;

            // Don't allow interrupt configurations if no interrupt ports are provided.
            InterruptSupported = interrupts.Count > 0;
            // Interrupts on all GPIO ports are mirrored if only one interrupt port is provided,
            // This has no effect on MCP23x08 devices which only have one GPIO port.
            MirroredInterrupts = interrupts.Count == 1;

            // Handle init
            Initialize();

            // map interrupts
            if (MirroredInterrupts)
            {
                interrupts[0].Changed += HandleChangedInterrupts;
            }
            else
            {
                for (var i = 0; i < interrupts.Count; i++)
                {
                    var interrupt = interrupts[i];

                    // needed for scoping
                    var port = i;

                    void CurryChangedInterruptForPort(object sender, DigitalInputPortEventArgs e)
                    {
                        HandleChangedInterruptForPort(port, e);
                    }

                    interrupt.Changed += CurryChangedInterruptForPort;
                }
            }
        }

        /// <summary>
        /// Current state of the configuration register (IOCON)
        /// </summary>
        protected byte IoconState { get; private set; }

        /// <summary>
        /// The currently configured bank mode.
        /// </summary>
        /// <remarks>
        /// Use <see cref="SetBankConfiguration" /> to change this value.
        /// </remarks>
        protected BankConfiguration BankConfiguration { get; private set; }

        /// <inheritdoc />
        public IMcpGpioPorts Ports { get; }

        /// <inheritdoc cref="IMcp23x.ConfigureInputPort" />
        public void ConfigureInputPort(
            IPin pin,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled)
        {
            if (!InterruptSupported && interruptMode != InterruptMode.None)
            {
                throw new ArgumentException(
                    "Interrupts are not supported as no interrupt port is configured.",
                    nameof(interruptMode));
            }

            if (resistorMode == ResistorMode.PullDown)
            {
                throw new Exception("Pull-down resistor mode is not supported.");
            }

            var enablePullUp = resistorMode == ResistorMode.PullUp;

            // Will throw if pin is not valid for this device.
            var port = Ports.GetPortIndexOfPin(pin);
            var pinKey = (byte) pin.Key;

            McpLogger.DebugOut.WriteLine($"Configuring input for port: {port}, pin: {pinKey}");

            // set the port direction
            SetPortDirection(port, pinKey, PortDirectionType.Input);

            // TODO: The previous implementation didn't trust the local copy of state, always pulling it from register. Why?
            // TODO: Do we even need local state?
            lock (ConfigurationLock)
            {
                if (BitHelpers.GetBitValue(GppuState[port], pinKey) != enablePullUp)
                {
                    GppuState[port] = BitHelpers.SetBit(GppuState[port], pinKey, enablePullUp);
                    WriteRegister(Mcp23PortRegister.PullupResistorConfigurationRegister, port, GppuState[port]);
                }
                McpLogger.DebugOut.WriteLine($"gppu    {Convert.ToString(GppuState[port], 2).PadLeft(8, '0')}");

                if (interruptMode == InterruptMode.None)
                {
                    return;
                }

                var existingValues = ReadRegisters(
                    port,
                    Mcp23PortRegister.InterruptOnChangeRegister,
                    Mcp23PortRegister.DefaultComparisonValueRegister,
                    Mcp23PortRegister.InterruptControlRegister);

                // interrupt on change (whether or not we want to raise an interrupt on the interrupt pin on change)
                var gpinten = existingValues[0];
                gpinten = BitHelpers.SetBit(gpinten, pinKey, true);

                // Set the default value for the pin for interrupts.
                var interruptValue = interruptMode == InterruptMode.EdgeFalling;
                var defVal = existingValues[1];
                defVal = BitHelpers.SetBit(defVal, pinKey, interruptValue);

                // interrupt control register; whether or not the change is based 
                // on default comparison value, or if a change from previous.
                var interruptControl = interruptMode != InterruptMode.EdgeBoth;
                var intCon = existingValues[2];
                intCon = BitHelpers.SetBit(intCon, pinKey, interruptControl);

                McpLogger.DebugOut.WriteLine($"gpinten {Convert.ToString(gpinten, 2).PadLeft(8, '0')}");
                McpLogger.DebugOut.WriteLine($"defVal  {Convert.ToString(defVal, 2).PadLeft(8, '0')}");
                McpLogger.DebugOut.WriteLine($"intCon  {Convert.ToString(intCon, 2).PadLeft(8, '0')}");
                WriteRegisters(
                    port, 
                    (Mcp23PortRegister.InterruptOnChangeRegister, gpinten),
                    (Mcp23PortRegister.DefaultComparisonValueRegister, defVal),
                    (Mcp23PortRegister.InterruptControlRegister, intCon));
            }
        }

        /// <inheritdoc cref="IMcp23x.CreateDigitalInputPort" />
        public IDigitalInputPort CreateDigitalInputPort(
            IPin pin,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled,
            double debounceDuration = 0,
            double glitchDuration = 0)
        {
            // Will throw if pin is not valid for this device.
            Ports.GetPortIndexOfPin(pin);

            ConfigureInputPort(pin, interruptMode, resistorMode);
            var inputPort = new McpDigitalInputPort(this, pin, interruptMode)
            {
                DebounceDuration = debounceDuration,
                GlitchDuration = glitchDuration
            };

            return inputPort;
        }

        /// <inheritdoc cref="IMcp23x.CreateDigitalOutputPort" />
        public IDigitalOutputPort CreateDigitalOutputPort(
            IPin pin,
            bool initialState = false,
            OutputType outputType = OutputType.OpenDrain)
        {
            // Will throw if pin is not valid for this device.
            var port = Ports.GetPortIndexOfPin(pin);

            // setup the port internally for output
            SetPortDirection(port, (byte) pin.Key, PortDirectionType.Output);

            // create the convenience class
            return new McpDigitalOutputPort(this, pin, initialState, outputType);
        }


        /// <inheritdoc />
        public bool ReadPin(IPin pin)
        {
            // Will throw if pin is not valid for this device.
            var port = Ports.GetPortIndexOfPin(pin);
            return ReadPin(port, (byte) pin.Key);
        }

        /// <inheritdoc />
        public void ResetPin(IPin pin)
        {
            // Will throw if pin is not valid for this device.
            var port = Ports.GetPortIndexOfPin(pin);
            ResetPin(port, (byte) pin.Key);
        }

        /// <inheritdoc />
        public void SetPortDirection(IPin pin, PortDirectionType direction)
        {
            // Will throw if pin is not valid for this device.
            var port = Ports.GetPortIndexOfPin(pin);

            SetPortDirection(port, (byte) pin.Key, direction);
        }

        /// <inheritdoc />
        public void WritePin(IPin pin, bool value)
        {
            // Will throw if pin is not valid for this device.
            var port = Ports.GetPortIndexOfPin(pin);
            WritePin(port, (byte) pin.Key, value);
        }

        /// <summary>
        /// Apply initial configuration to an unconfigured peripheral.
        /// </summary>
        /// <remarks>
        /// Should not be called except by the constructor of <see cref="Mcp23x" />.
        /// Can be overridden if additional configuration is required.
        /// </remarks>
        protected void Initialize()
        {
            lock (ConfigurationLock)
            {
                lock (_bankLock)
                {
                    // The BANK bit changes how the registers are mapped (see Tables 3-4 and3-5 for more details).
                    // • If BANK = 1, the registers associated with each port are segregated. Registers associated with
                    //   PORTA are mapped from address 00h - 0Ah and registers associated with PORTB are mapped
                    //   from 10h - 1Ah.
                    // • If BANK = 0, the A/B registers are paired. For example, IODIRA is mapped to address 00h and
                    //   IODIRB is mapped to the next address (address 01h). The mapping for all registers is from 00h
                    //   -15h.
                    // It is important to take care when changing the BANK bit as the address mapping changes after the byte is
                    // clocked into the device. The address pointer may point to an invalid location after the bit is modified.
                    // For example, if the device is configured to automatically increment its internal Address Pointer,
                    // the following scenario would occur:
                    // • BANK = 0
                    // • Write 80h to address 0Ah (IOCON) to set the BANK bit
                    // • Once the write completes, the internal address now points to 0Bh which is an invalid address
                    //   when the BANK bit is set.
                    // For this reason, when changing the BANK bit, it is advised to only perform byte writes to this register.
                    BankConfiguration bank;
                    // determine best setting for IOCON.Bank
                    // the correct setting will minimize the number of calls required to retrieve data
                    // this may not be perfect, so it may be recommended to expose a way to change this
                    if (Ports.Count == 1)
                    {
                        // for devices with a single GPIO port, leave default.
                        bank = BankConfiguration.Paired;
                    }
                    else if (Interrupts.Count == 0)
                    {
                        // when no interrupts are configured, it is likely that ports are read one at a time
                        // segregated is better in this scenario
                        // **This is a huge guess**
                        bank = BankConfiguration.Segregated;
                    }
                    else if (Interrupts.Count == 1)
                    {
                        // a single interrupt means that the interrupt will be shared across all GPIO ports
                        // paired is more efficient for this
                        bank = BankConfiguration.Paired;
                    }
                    else
                    {
                        // There is an interrupt for every GPIO port.
                        // segregated is more efficient for this
                        bank = BankConfiguration.Segregated;
                    }

                    // IOCON.Mirror, whether the interrupt ports should be logically OR'ed
                    // Determined by the number of GPIO ports
                    var mirror = MirroredInterrupts;

                    // IOCON.Seqop, should the address pointer be auto incremented 
                    // This is useful for rapid reads (polling) or rapid writes to the same GPIO port
                    // This scenario is not supported by this class (would make sense to build a dedicated class for it)
                    // Default to 0
                    const bool sequentialOperation = false;

                    // IOCON.Disslw, controls the slew rate function on the SDA pin. If enabled, the SDA slew rate will
                    // be controlled when driving from a high to low.
                    // No need to change this.
                    // Default to 0
                    const bool slewRate = false;

                    // IOCON.Haen The Hardware Address Enable bit enables/disables hardware addressing on the MCP23S17
                    // only. The address pins (A2, A1 and A0) must be externally biased, regardless of the HAEN bit value.
                    // If enabled (HAEN = 1), the device’s hardware address matches the address pins.
                    // If disabled (HAEN = 0), the device’s hardware address is A2 = A1 = A0 = 0.
                    // Not supporting hardware address enabled.
                    // Default to 0
                    const bool hardwareAddressEnable = false;

                    // IOCON.Odr The Open-Drain control bit enables/disables the INT pin for open-drain configuration.
                    // Setting this bit overrides the INTPOL bit.
                    // Not supporting Open drain mode
                    // Default to 0
                    const bool openDrain = false;

                    // IOCON.Intpol. The Interrupt Polarity sets the polarity of the
                    // INT pin. This bit is functional only when the ODR bit is
                    // cleared, configuring the INT pin as active push-pull.
                    // 1 = Active-high.
                    // 0 = Active-low.
                    // Set to 0.
                    // Supports any interrupt type, but given the choice default to Active-low.
                    var interruptPolarity = _interruptMode == InterruptMode.EdgeRising;

                    BankConfiguration = bank;
                    IoconState = BitHelpers.SetBit(IoconState, 0x07, (byte) bank == 0x01);
                    IoconState = BitHelpers.SetBit(IoconState, 0x06, mirror);
                    IoconState = BitHelpers.SetBit(IoconState, 0x05, sequentialOperation);
                    IoconState = BitHelpers.SetBit(IoconState, 0x04, slewRate);
                    IoconState = BitHelpers.SetBit(IoconState, 0x03, hardwareAddressEnable);
                    IoconState = BitHelpers.SetBit(IoconState, 0x02, openDrain);
                    IoconState = BitHelpers.SetBit(IoconState, 0x01, interruptPolarity);

                    // All ports share the same IOCON, so just call port 0.
                    WriteRegister(Mcp23PortRegister.IOConfigurationRegister, 0, IoconState);
                }
            }

            // flush all settings to expected defaults
            WriteToAllPorts(
                (Mcp23PortRegister.IODirectionRegister, 0xFF),
                (Mcp23PortRegister.InputPolarityRegister, 0x00),
                (Mcp23PortRegister.InterruptOnChangeRegister, 0x00),
                (Mcp23PortRegister.DefaultComparisonValueRegister, 0x00),
                (Mcp23PortRegister.InterruptControlRegister, 0x00),
                (Mcp23PortRegister.IOConfigurationRegister, 0x00),
                (Mcp23PortRegister.PullupResistorConfigurationRegister, 0x00),
                //(Mcp23PortRegister.InterruptFlagRegister, 0x00), // readonly
                //(Mcp23PortRegister.InterruptCaptureRegister, 0x00), // readonly
                (Mcp23PortRegister.GPIORegister, 0x00));

            // finally, read the input. this will clear any unprocessed interrupts
            ReadFromAllPorts(Mcp23PortRegister.InterruptFlagRegister, Mcp23PortRegister.GPIORegister);
        }

        /// <summary>
        /// Checks whether or not the pin passed in exists on the chip.
        /// </summary>
        protected bool IsValidPin(IPin pin)
        {
            return Ports.AllPins.Contains(pin);
        }

        /// <summary>
        /// Change the configuration of IOCON.Bank
        /// </summary>
        /// <param name="bank">The new setting for IOCON.Bank</param>
        /// <remarks>
        /// Different configurations can have different effects on performance.
        /// When creating this instance an educated guess is made to set the optimal configuration
        /// based on how many interrupt ports were provided.
        /// </remarks>
        protected void SetBankConfiguration(BankConfiguration bank)
        {
            lock (ConfigurationLock)
            {
                lock (_bankLock)
                {
                    // just to be safe
                    IoconState = ReadRegister(Mcp23PortRegister.IOConfigurationRegister, 0);
                    IoconState = BitHelpers.SetBit(IoconState, 0x07, (byte) bank);

                    // All ports share the same IOCON, so just call port 0.
                    WriteRegister(Mcp23PortRegister.IOConfigurationRegister, 0, IoconState);
                    BankConfiguration = bank;
                }
            }
        }

        /// <summary>
        /// Interrupt event handler for a specific port
        /// </summary>
        /// <param name="port">Index of the GPIO port that has been interrupted</param>
        private void HandleChangedInterruptForPort(int port, DigitalInputPortEventArgs e)
        {
            // if interrupt mode is both, we only trigger on EdgeFalling (Active-low)
            if (_interruptMode == InterruptMode.EdgeBoth && e.Value == true)
            {
                return;
            }

            try
            {
                // Important: Reading from GPIO and not INTCAP
                // INTCAP captures all values of the port at the time that the interrupt was triggered
                // This means it will not update if a different pin changes state before we get to read INTCAP
                // For this reason, we read from GPIO
                // To summarise the tradeoff:
                // GPIO - Get the latest values of all pins, possibly missing the value that initially triggered the interrupt
                // INTCAP - Always get the value that triggered the interrupt, but possibly miss all value changes between interrupt and read
                var portRead = ReadRegisters(
                    port,
                    Mcp23PortRegister.InterruptFlagRegister,
                    Mcp23PortRegister.GPIORegister);

                var intFlag = portRead[0];
                var intValue = portRead[1];

                // were there any interrupts for this port?
                if (intFlag == 0x00)
                {
                    return;
                }

                Ports[port].InvokeInputChanged(new IOExpanderPortInputChangedEventArgs(intFlag, intValue));

                var intFlags = new byte[Ports.Count];
                var intValues = new byte[Ports.Count];

                intFlags[port] = intFlag;
                intValues[port] = intValue;

                InputChanged?.Invoke(this, new IOExpanderMultiPortInputChangedEventArgs(intFlags, intValues));
            }
            catch (Exception ex)
            {
                McpLogger.ErrorOut.WriteLine(ex.ToString());
            }
        }


        /// <summary>
        /// Interrupt event handler for mirrored interrupts that trigger for any/all ports.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event</param>
        private void HandleChangedInterrupts(object sender, DigitalInputPortEventArgs e)
        {
            // if interrupt mode is both, we only trigger on EdgeFalling (Active-low)
            if (_interruptMode == InterruptMode.EdgeBoth && e.Value == true)
            {
                return;
            }

            try
            {
                // Important: Reading from GPIO and not INTCAP
                // INTCAP captures all values of the port at the time that the interrupt was triggered
                // This means it will not update if a different pin changes state before we get to read INTCAP
                // For this reason, we read from GPIO
                // To summarise the tradeoff:
                // GPIO - Get the latest values of all pins, possibly missing the value that initially triggered the interrupt
                // INTCAP - Always get the value that triggered the interrupt, but possibly miss all value changes between interrupt and read
                var portReads = ReadFromAllPorts(
                    Mcp23PortRegister.InterruptFlagRegister,
                    Mcp23PortRegister.InterruptCaptureRegister);

                // fire per-port events
                var anyInterrupts = false;
                var intFlags = new byte[Ports.Count];
                var intValues = new byte[Ports.Count];

                for (var i = 0; i < Ports.Count; i++)
                {
                    var intFlag = portReads[i][0];
                    var intValue = portReads[i][1];

                    intFlags[i] = intFlag;
                    intValues[i] = intValue;

                    // were there any interrupts for this port?
                    if (intFlag > 0x00)
                    {
                        anyInterrupts = true;
                        Ports[i].InvokeInputChanged(new IOExpanderPortInputChangedEventArgs(intFlag, intValue));
                    }
                }

                if (!anyInterrupts)
                {
                    return;
                }

                InputChanged?.Invoke(this, new IOExpanderMultiPortInputChangedEventArgs(intFlags, intValues));
            }
            catch (Exception ex)
            {
                McpLogger.ErrorOut.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Read the specified pin.
        /// </summary>
        /// <param name="port">The port of the pin.</param>
        /// <param name="pinKey">The pin key / bit index of the pin on the port.</param>
        /// <returns>ASDDSA</returns>
        private bool ReadPin(int port, byte pinKey)
        {
            // if the pin isn't set for input, configure it
            SetPortDirection(port, pinKey, PortDirectionType.Input);

            // update our GPIO values
            var gpioState = ReadRegister(Mcp23PortRegister.GPIORegister, port);

            // return the value on that port
            return BitHelpers.GetBitValue(gpioState, pinKey);
        }

        /// <summary>
        /// Reset a pin to it's initial input state.
        /// </summary>
        /// <param name="port">The port of the pin.</param>
        /// <param name="pinKey">The pin key / bit index of the pin on the port.</param>
        private void ResetPin(int port, byte pinKey)
        {
            SetPortDirection(port, pinKey, PortDirectionType.Input);
        }

        /// <summary>
        /// Set the direction of a port as input or output.
        /// </summary>
        /// <param name="port">The port of the pin.</param>
        /// <param name="pinKey">The pin key / bit index of the pin on the port.</param>
        /// <param name="direction">The direction to set.</param>
        private void SetPortDirection(int port, byte pinKey, PortDirectionType direction)
        {
            // if it's already configured, get out. (1 = input, 0 = output)
            if (BitHelpers.GetBitValue(IodirState[port], pinKey) == (direction == PortDirectionType.Input))
            {
                return;
            }

            lock (ConfigurationLock)
            {
                // set the IODIR bit and write the setting
                IodirState[port] = BitHelpers.SetBit(IodirState[port], pinKey, (byte) direction);
                WriteRegister(Mcp23PortRegister.IODirectionRegister, port, IodirState[port]);
            }
        }

        /// <summary>
        /// Write a value to the specified value.
        /// </summary>
        /// <param name="port">The port of the pin.</param>
        /// <param name="pinKey">The pin key / bit index of the pin on the port.</param>
        /// <param name="value">The value to write.</param>
        private void WritePin(int port, byte pinKey, bool value)
        {
            // if the pin isn't set for output, configure it
            SetPortDirection(port, pinKey, PortDirectionType.Output);

            // update our GPIO values
            OlatState[port] = BitHelpers.SetBit(OlatState[port], pinKey, value);
            WriteRegister(Mcp23PortRegister.OutputLatchRegister, port, OlatState[port]);
        }

        #region Register operations

        /// <summary>
        /// Read a register from all ports.
        /// </summary>
        /// <param name="register">The register to read.</param>
        /// <returns>An array of bytes, one byte for each port, ordered by port.</returns>
        protected byte[] ReadFromAllPorts(Mcp23PortRegister register)
        {
            if (Ports.Count == 1)
            {
                return new[] { ReadRegister(register, 0) };
            }

            var addresses = new (Mcp23PortRegister register, int port)[Ports.Count];
            for (var i = 0; i < addresses.Length; i++)
            {
                addresses[i] = (register, port: i);
            }

            return ReadRegisters(addresses);
        }

        /// <summary>
        /// Read a collection of registers from all ports in the most efficient way possible.
        /// Results are split by port, then by register.
        /// </summary>
        /// <param name="registers">The registers to read.</param>
        /// <returns>
        /// An array of ports read, containing arrays of registers read. Ports are ordered by port index, registers are
        /// ordered by passed in order.
        /// </returns>
        /// <remarks>For optimal use, read the registers in the order they are defined in <see cref="Mcp23PortRegister" />.</remarks>
        protected byte[][] ReadFromAllPorts(params Mcp23PortRegister[] registers)
        {
            var result = new byte[Ports.Count][];
            // short circuit if no registers are provided
            if (registers == null || registers.Length == 0)
            {
                for (var i = 0; i < Ports.Count; i++)
                {
                    result[i] = new byte[0];
                }

                return result;
            }

            // build the result array size
            for (var i = 0; i < Ports.Count; i++)
            {
                result[i] = new byte[registers.Length];
            }

            // lock on bank, so it doesn't get changed while we are querying.
            lock (_bankLock)
            {
                switch (BankConfiguration)
                {
                    // In paired mode, sort the addresses by register then port
                    case BankConfiguration.Paired:
                    {
                        var addresses = new (Mcp23PortRegister register, int port)[registers.Length * Ports.Count];
                        for (var i = 0; i < addresses.Length; i++)
                        {
                            addresses[i] = (register: registers[i / Ports.Count], port: i % Ports.Count);
                        }

                        var read = ReadRegisters(addresses);
                        for (var i = 0; i < read.Length; i++)
                        {
                            result[i % Ports.Count][i / Ports.Count] = read[i];
                        }
                    }
                        break;
                    // In segregated mode, sort the addresses by port then register
                    case BankConfiguration.Segregated:
                    {
                        var addresses = new (Mcp23PortRegister register, int port)[registers.Length * Ports.Count];
                        for (var i = 0; i < addresses.Length; i++)
                        {
                            addresses[i] = (register: registers[i % registers.Length], port: i / registers.Length);
                        }

                        var read = ReadRegisters(addresses);
                        for (var i = 0; i < read.Length; i++)
                        {
                            result[i / registers.Length][i % registers.Length] = read[i];
                        }
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(BankConfiguration));
                }
            }

            return result;
        }

        /// <summary>
        /// Read a register for a single port.
        /// </summary>
        /// <param name="register">The register to read.</param>
        /// <param name="port">The port index to read.</param>
        /// <returns>The value of the register.</returns>
        protected byte ReadRegister(Mcp23PortRegister register, int port)
        {
            // lock on bank, so it doesn't get changed while we are querying.
            lock (_bankLock)
            {
                return _mcpDevice.ReadRegister(_registerMap.GetAddress(register, port, BankConfiguration));
            }
        }

        /// <summary>
        /// Read multiple addresses for a series of ports.
        /// </summary>
        /// <param name="addresses">The register and port combinations to read.</param>
        /// <returns>Values from all read registers</returns>
        /// <remarks>
        /// Attempts to optimize calls by reading multiple registers at once wherever possible.
        /// </remarks>
        protected byte[] ReadRegisters(params (Mcp23PortRegister register, int port)[] addresses)
        {
            if (addresses == null || addresses.Length == 0)
            {
                return new byte[0];
            }

            var result = new byte[addresses.Length];

            // lock on bank, so it doesn't get changed while we are querying.
            lock (_bankLock)
            {
                byte dataStartAddress = 0x00;
                ushort dataStartIndex = 0;
                for (ushort i = 0; i < addresses.Length; i++)
                {
                    if (addresses[i].port < 0 || addresses[i].port >= Ports.Count)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(addresses),
                            $"Port {addresses[i].port} is out of range");
                    }

                    var nextAddress = _registerMap.GetAddress(
                        addresses[i].register,
                        addresses[i].port,
                        BankConfiguration);
                    if (i == 0)
                    {
                        dataStartAddress = nextAddress;
                        continue;
                    }

                    if (nextAddress == dataStartAddress + (i - dataStartIndex))
                    {
                        continue;
                    }

                    _mcpDevice.ReadRegisters(dataStartAddress, (ushort) (i - dataStartIndex))
                        .CopyTo(result, dataStartIndex);

                    dataStartAddress = nextAddress;
                    dataStartIndex = i;
                }

                _mcpDevice.ReadRegisters(dataStartAddress, (ushort) (addresses.Length - dataStartIndex))
                    .CopyTo(result, dataStartIndex);
            }

            return result;
        }

        /// <summary>
        /// Read multiple addresses for a single port.
        /// </summary>
        /// <param name="port">The port index to read</param>
        /// <param name="registers">The registers to read</param>
        /// <returns>Values from all read registers</returns>
        /// <remarks>
        /// Attempts to optimize calls by reading multiple registers at once wherever possible.
        /// </remarks>
        protected byte[] ReadRegisters(int port, params Mcp23PortRegister[] registers)
        {
            if (port < 0 || port >= Ports.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(port), $"Port {port} is out of range");
            }

            if (registers == null || registers.Length == 0)
            {
                return new byte[0];
            }

            var result = new byte[registers.Length];

            // lock on bank, so it doesn't get changed while we are querying.
            lock (_bankLock)
            {
                byte dataStartAddress = 0x00;
                ushort dataStartIndex = 0;
                for (ushort i = 0; i < registers.Length; i++)
                {
                    var nextAddress = _registerMap.GetAddress(registers[i], port, BankConfiguration);
                    if (i == 0)
                    {
                        dataStartAddress = nextAddress;
                        continue;
                    }

                    if (nextAddress == dataStartAddress + (i - dataStartIndex))
                    {
                        continue;
                    }

                    _mcpDevice.ReadRegisters(dataStartAddress, (ushort) (i - dataStartIndex))
                        .CopyTo(result, dataStartIndex);

                    dataStartAddress = nextAddress;
                    dataStartIndex = i;
                }

                _mcpDevice.ReadRegisters(dataStartAddress, (ushort) (registers.Length - dataStartIndex))
                    .CopyTo(result, dataStartIndex);
            }

            return result;
        }

        /// <summary>
        /// Write a value to a register.
        /// </summary>
        /// <param name="register">The register to write to</param>
        /// <param name="port">The port index to write to</param>
        /// <param name="value">The value to write</param>
        protected void WriteRegister(Mcp23PortRegister register, int port, byte value)
        {
            //Mcp23Logger.DebugOut.WriteLine($"({register}, {port}, {Convert.ToString(value, 2).PadLeft(8, '0')})");
            // lock on bank, so it doesn't get changed while we are querying.
            lock (_bankLock)
            {
                _mcpDevice.WriteRegister(_registerMap.GetAddress(register, port, BankConfiguration), value);
            }
        }

        /// <summary>
        /// Write to multiple registers
        /// </summary>
        /// <param name="writeOps">The write operations to perform</param>
        /// <remarks>
        /// Attempts to optimize calls by writing multiple registers at once wherever possible.
        /// </remarks>
        protected void WriteRegisters(params (Mcp23PortRegister register, int port, byte value)[] writeOps)
        {
            //Mcp23Logger.DebugOut.WriteLine(string.Join("\n    ", writeOps.Select(x => $"({x.register}, {x.port}, {Convert.ToString(x.value, 2).PadLeft(8, '0')})")));
            // lock on bank, so it doesn't get changed while we are querying.
            lock (_bankLock)
            {
                byte dataStartAddress = 0x00;
                ushort dataStartIndex = 0;
                for (ushort i = 0; i < writeOps.Length; i++)
                {
                    var nextAddress = _registerMap.GetAddress(
                        writeOps[i].register,
                        writeOps[i].port,
                        BankConfiguration);
                    if (i == 0)
                    {
                        dataStartAddress = nextAddress;
                        continue;
                    }

                    if (nextAddress == dataStartAddress + (i - dataStartIndex))
                    {
                        continue;
                    }

                    _mcpDevice.WriteRegisters(
                        dataStartAddress,
                        writeOps.Skip(dataStartIndex).Take(i - dataStartIndex).Select(a => a.value).ToArray());

                    dataStartAddress = nextAddress;
                    dataStartIndex = i;
                }

                _mcpDevice.WriteRegisters(
                    dataStartAddress,
                    writeOps.Skip(dataStartIndex)
                        .Take(writeOps.Length - dataStartIndex)
                        .Select(a => a.value)
                        .ToArray());
            }
        }

        /// <summary>
        /// Write to multiple registers
        /// </summary>
        /// <param name="port">The port to write to</param>
        /// <param name="writeOps">The write operations to perform</param>
        /// <remarks>
        /// Attempts to optimize calls by writing multiple registers at once wherever possible.
        /// </remarks>
        protected void WriteRegisters(int port, params (Mcp23PortRegister register, byte value)[] writeOps)
        {
            //Mcp23Logger.DebugOut.WriteLine($"port {port}, " + string.Join("\n    ", writeOps.Select(x => $"({x.register}, {port}, {Convert.ToString(x.value, 2).PadLeft(8, '0')})")));
            // lock on bank, so it doesn't get changed while we are querying.
            lock (_bankLock)
            {
                // TODO: a lot ot unit testing
                byte dataStartAddress = 0x00;
                ushort dataStartIndex = 0;
                for (ushort i = 0; i < writeOps.Length; i++)
                {
                    var nextAddress = _registerMap.GetAddress(writeOps[i].register, port, BankConfiguration);
                    if (i == 0)
                    {
                        dataStartAddress = nextAddress;
                        continue;
                    }

                    if (nextAddress == dataStartAddress + (i - dataStartIndex))
                    {
                        continue;
                    }

                    _mcpDevice.WriteRegisters(
                        dataStartAddress,
                        writeOps.Skip(dataStartIndex).Take(i - dataStartIndex).Select(a => a.value).ToArray());

                    dataStartAddress = nextAddress;
                    dataStartIndex = i;
                }

                _mcpDevice.WriteRegisters(
                    dataStartAddress,
                    writeOps.Skip(dataStartIndex)
                        .Take(writeOps.Length - dataStartIndex)
                        .Select(a => a.value)
                        .ToArray());
            }
        }

        /// <summary>
        /// Write a single value to a register for every port.
        /// </summary>
        /// <param name="register">The register to write to</param>
        /// <param name="value">The value to write</param>
        protected void WriteToAllPorts(Mcp23PortRegister register, byte value)
        {
            if (Ports.Count == 1)
            {
                WriteRegister(register, 0, value);
                return;
            }

            var writeOps = new (Mcp23PortRegister register, int port, byte value)[Ports.Count];
            for (var i = 0; i < writeOps.Length; i++)
            {
                writeOps[i] = (register, port: i, value);
            }

            WriteRegisters(writeOps);
        }

        /// <summary>
        /// Write to a series of registers across all ports.
        /// </summary>
        /// <param name="writeOps">The list of registers and values to write</param>
        protected void WriteToAllPorts(params (Mcp23PortRegister register, byte value)[] writeOps)
        {
            if (writeOps == null || writeOps.Length == 0)
            {
                return;
            }

            // lock on bank, so it doesn't get changed while we are querying.
            lock (_bankLock)
            {
                switch (BankConfiguration)
                {
                    // In paired mode, sort the addresses by register then port
                    case BankConfiguration.Paired:
                    {
                        var addresses =
                            new (Mcp23PortRegister register, int port, byte value)[writeOps.Length * Ports.Count];
                        for (var i = 0; i < addresses.Length; i++)
                        {
                            addresses[i] = (writeOps[i / Ports.Count].register, port: i % Ports.Count,
                                writeOps[i / Ports.Count].value);
                        }

                        WriteRegisters(addresses);
                        return;
                    }
                    // In segregated mode, sort the addresses by port then register
                    case BankConfiguration.Segregated:
                    {
                        var addresses =
                            new (Mcp23PortRegister register, int port, byte value)[writeOps.Length * Ports.Count];
                        for (var i = 0; i < addresses.Length; i++)
                        {
                            addresses[i] = (writeOps[i % writeOps.Length].register, port: i / writeOps.Length,
                                writeOps[i % writeOps.Length].value);
                        }

                        WriteRegisters(addresses);
                        return;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        #endregion

        #region Unsupported Implementations

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public IBiDirectionalPort CreateBiDirectionalPort(
            IPin pin,
            bool initialState = false,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled,
            PortDirectionType initialDirection = PortDirectionType.Input,
            double debounceDuration = 0,
            double glitchDuration = 0,
            OutputType output = OutputType.PushPull)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public IAnalogInputPort CreateAnalogInputPort(IPin pin, float voltageReference = 3.3f)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public IPwmPort CreatePwmPort(IPin pin, float frequency = 100, float dutyCycle = 0.5f, bool invert = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public ISerialPort CreateSerialPort(
            SerialPortName portName,
            int baudRate,
            int dataBits = 8,
            Parity parity = Parity.None,
            StopBits stopBits = StopBits.One,
            int readBufferSize = 4096)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public ISerialMessagePort CreateSerialMessagePort(
            SerialPortName portName,
            byte[] suffixDelimiter,
            bool preserveDelimiter,
            int baudRate = 9600,
            int dataBits = 8,
            Parity parity = Parity.None,
            StopBits stopBits = StopBits.One,
            int readBufferSize = 512)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public ISerialMessagePort CreateSerialMessagePort(
            SerialPortName portName,
            byte[] prefixDelimiter,
            bool preserveDelimiter,
            int messageLength,
            int baudRate = 9600,
            int dataBits = 8,
            Parity parity = Parity.None,
            StopBits stopBits = StopBits.One,
            int readBufferSize = 512)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, SpiClockConfiguration config)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, long speedkHz = 375)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public II2cBus CreateI2cBus(IPin[] pins, int frequencyHz = 100000)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public II2cBus CreateI2cBus(IPin clock, IPin data, int frequencyHz = 100000)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public void SetClock(DateTime dateTime)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        [Obsolete("Not implemented, do not use")]
        public DeviceCapabilities Capabilities => throw new NotImplementedException();

        #endregion
    }
}
