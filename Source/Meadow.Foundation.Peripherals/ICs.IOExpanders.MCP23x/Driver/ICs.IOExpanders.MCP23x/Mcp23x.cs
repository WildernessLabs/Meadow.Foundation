using System;
using System.Collections.Generic;
using System.Linq;
using Meadow.Foundation.ICs.IOExpanders.Device;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract class Mcp23x : IIODevice, IMcp23x
    {
        // Use this lock whenever accessing GPPU, IOCON, IODIR, GPINTEN
        protected readonly object ConfigurationLock = new object();
        protected readonly byte[] GpioState;
        protected readonly byte[] GppuState;
        protected readonly IList<IDigitalInputPort> Interrupts;

        protected readonly bool InterruptSupported;
        protected readonly byte[] IodirState;
        protected readonly bool MirroredInterrupts;
        protected readonly byte[] OlatState;

        private readonly BankConfiguration _bank;

        // Use this lock whenever reading/writing _bank
        private readonly object _bankLock = new object();
        private readonly IMcpDeviceComms _mcpDevice;
        private readonly IMcp23RegisterMap _registerMap;
        protected byte IoconState;

        public event EventHandler<IOExpanderMultiPortInputChangedEventArgs> InputChanged = delegate { };


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
                throw new ArgumentNullException(nameof(ports), "Ports cannot be null");
            }

            if (interrupts.Any(i => i == null))
            {
                throw new ArgumentNullException(nameof(interrupts), "Interrupts cannot be null.");
            }

            if (interrupts.Any(i => i.InterruptMode != InterruptMode.EdgeRising))
            {
                throw new ArgumentException(
                    "Interrupt ports must use an interrupt mode of EdgeRising",
                    nameof(interrupts));
            }

            if (interrupts.Count > 1 && interrupts.Count != ports.Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(interrupts),
                    interrupts.Count,
                    "There can only be 0, 1 or N interrupts, where N is the number of GPIO Ports.");
            }


            // Bank is initialized at 0 for all devices that support it.
            _bank = BankConfiguration.Paired;

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
            GpioState = InitState(0x00);
            OlatState = InitState(0x00);
            GppuState = InitState(0x00);
            IoconState = 0x00;

            // Don't allow interrupt configurations if no interrupt ports are provided.
            InterruptSupported = interrupts.Count > 0;
            // Interrupts on all GPIO ports are mirrored if only one interrupt port is provided,
            // This has no effect on MCP23x08 devices which only have one GPIO port.
            MirroredInterrupts = interrupts.Count == 1;

            // TODO: more interrupt stuff to solve
            // at a minimum, we need to doc in constructor
            // what we expect from an interrupt port.
            //this._interruptPort.InterruptMode = InterruptMode.EdgeRising;
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
                        HandleChangedInterruptForPort(sender, e, port);
                    }

                    interrupt.Changed += CurryChangedInterruptForPort;
                }
            }

            // Handle init
            Initialize();
        }

        /// <inheritdoc />
        public IMcpGpioPorts Ports { get; }

        /// <inheritdoc cref="IMcp23x.ConfigureInputPort" />
        public void ConfigureInputPort(
            IPin pin,
            bool enablePullUp = false,
            InterruptMode interruptMode = InterruptMode.None)
        {
            if (!InterruptSupported && interruptMode != InterruptMode.None)
            {
                throw new ArgumentException(
                    "Interrupts are not supported as no interrupt port is configured.",
                    nameof(interruptMode));
            }

            // Will throw if pin is not valid for this device.
            var port = Ports.GetPortIndexOfPin(pin);
            var pinKey = (byte) pin.Key;

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

                if (interruptMode == InterruptMode.None)
                {
                    return;
                }

                // interrupt on change (whether or not we want to raise an interrupt on the interrupt pin on change)
                var gpinten = ReadRegister(Mcp23PortRegister.InterruptOnChangeRegister, port);
                gpinten = BitHelpers.SetBit(gpinten, pinKey, true);
                WriteRegister(Mcp23PortRegister.InterruptOnChangeRegister, port, gpinten);

                // Set the default value for the pin for interrupts.
                var interruptValue = interruptMode == InterruptMode.EdgeFalling;
                var defVal = ReadRegister(Mcp23PortRegister.DefaultComparisonValueRegister, port);
                defVal = BitHelpers.SetBit(defVal, pinKey, interruptValue);
                WriteRegister(Mcp23PortRegister.DefaultComparisonValueRegister, port, defVal);

                // Set the input polarity of the pin. Basically if its normally high, we want to flip the polarity.
                var pol = ReadRegister(Mcp23PortRegister.InputPolarityRegister, port);
                pol = BitHelpers.SetBit(pol, pinKey, !interruptValue);
                WriteRegister(Mcp23PortRegister.InputPolarityRegister, port, pol);

                // interrupt control register; whether or not the change is based 
                // on default comparison value, or if a change from previous. We 
                // want to raise on change, so we set it to 0, always.
                var interruptControl = interruptMode != InterruptMode.EdgeBoth;
                var intCon = ReadRegister(Mcp23PortRegister.InterruptControlRegister, port);
                intCon = BitHelpers.SetBit(intCon, pinKey, interruptControl);
                WriteRegister(Mcp23PortRegister.InterruptControlRegister, port, intCon);
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

            if (resistorMode == ResistorMode.PullDown)
            {
                Console.WriteLine("Pull-down resistor mode is not supported.");
                throw new Exception("Pull-down resistor mode is not supported.");
            }

            var enablePullUp = resistorMode == ResistorMode.PullUp;
            ConfigureInputPort(pin, enablePullUp, interruptMode);
            var inputPort = new McpDigitalInputPort(this, pin, interruptMode);

            // TODO: Determine if this is needed
            // _inputPorts.Add(pin, port);

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
            return new McpDigitalOutputPort(this, pin, port, initialState, outputType);
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
            // flush all settings to expected defaults
            // WARNING: this will not work if IOCON.BANK was set to 1 prior to calling Initialize().
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
                    // Set to 1.
                    // Do not modify as interrupts in this class are handled with the assumption that IOCON.Intpol is 1.
                    const bool interruptPolarity = true;

                    IoconState = BitHelpers.SetBit(IoconState, 0x07, (byte) bank);
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
        protected void SetBankConfigurationInternal(BankConfiguration bank)
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
                }
            }
        }

        private void HandleChangedInterruptForPort(object sender, DigitalInputPortEventArgs e, int port)
        {
            try
            {
                var portRead = ReadRegisters(
                    port,
                    Mcp23PortRegister.InterruptFlagRegister,
                    Mcp23PortRegister.InterruptCaptureRegister);

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
                Console.WriteLine(ex.ToString());
            }
        }

        private void HandleChangedInterrupts(object sender, DigitalInputPortEventArgs e)
        {
            try
            {
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
                Console.WriteLine(ex.ToString());
            }
        }

        private bool ReadPin(int port, byte pinKey)
        {
            // if the pin isn't set for input, configure it
            SetPortDirection(port, pinKey, PortDirectionType.Input);

            // update our GPIO values
            GpioState[port] = ReadRegister(Mcp23PortRegister.GPIORegister, port);

            // return the value on that port
            return BitHelpers.GetBitValue(GpioState[port], pinKey);
        }

        private void ResetPin(int port, byte pinKey)
        {
            SetPortDirection(port, pinKey, PortDirectionType.Input);
        }

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

        private void WritePin(int port, byte pinKey, bool value)
        {
            // if the pin isn't set for output, configure it
            SetPortDirection(port, pinKey, PortDirectionType.Output);

            // update our GPIO values
            OlatState[port] = BitHelpers.SetBit(OlatState[port], pinKey, value);
            WriteRegister(Mcp23PortRegister.OutputLatchRegister, port, OlatState[port]);
        }

        #region Register operations

        protected byte[] ReadFromAllPorts(Mcp23PortRegister register)
        {
            return ReadRegisters(Enumerable.Range(0, Ports.Count).Select(port => (register, port)).ToArray());
        }

        /// <summary>
        /// Read a collection of registers from all ports in the most efficient way possible.
        /// Results are split by port, then by register.
        /// </summary>
        /// <param name="registers">The registers to read.</param>
        /// <returns>An array of ports read, containing arrays of registers read</returns>
        /// <remarks>For optimal use, read the registers in the order they are defined in <see cref="Mcp23PortRegister" />.</remarks>
        protected byte[][] ReadFromAllPorts(params Mcp23PortRegister[] registers)
        {
            var result = new byte[Ports.Count][];
            if (registers == null || registers.Length == 0)
            {
                for (var i = 0; i < Ports.Count; i++)
                {
                    result[i] = new byte[0];
                }

                return result;
            }

            for (var i = 0; i < Ports.Count; i++)
            {
                result[1] = new byte[registers.Length];
            }

            lock (_bankLock)
            {
                switch (_bank)
                {
                    case BankConfiguration.Paired:
                    {
                        var addresses = Enumerable.Range(0, registers.Length * Ports.Count)
                            .Select(i => (register: registers[i / Ports.Count], port: i % Ports.Count))
                            .ToArray();
                        var read = ReadRegisters(addresses);
                        for (var i = 0; i < read.Length; i++)
                        {
                            result[i % Ports.Count][i / Ports.Count] = read[i];
                        }
                    }
                        break;
                    case BankConfiguration.Segregated:
                    {
                        var addresses = Enumerable.Range(0, registers.Length * Ports.Count)
                            .Select(i => (register: registers[i % registers.Length], port: i / registers.Length))
                            .ToArray();
                        var read = ReadRegisters(addresses);
                        for (var i = 0; i < read.Length; i++)
                        {
                            result[i / registers.Length][i % registers.Length] = read[i];
                        }
                    }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }


        protected byte ReadRegister(Mcp23PortRegister register, int port)
        {
            lock (_bankLock)
            {
                return _mcpDevice.ReadRegister(_registerMap.GetAddress(register, port, _bank));
            }
        }

        protected byte[] ReadRegisters((Mcp23PortRegister register, int port)[] addresses)
        {
            var result = new byte[addresses.Length];

            lock (_bankLock)
            {
                byte dataStartAddress = 0x00;
                ushort dataStartIndex = 0;
                for (ushort i = 0; i < addresses.Length; i++)
                {
                    var nextAddress = _registerMap.GetAddress(addresses[i].register, addresses[i].port, _bank);
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


        protected byte[] ReadRegisters(int port, params Mcp23PortRegister[] registers)
        {
            var result = new byte[registers.Length];

            lock (_bankLock)
            {
                byte dataStartAddress = 0x00;
                ushort dataStartIndex = 0;
                for (ushort i = 0; i < registers.Length; i++)
                {
                    var nextAddress = _registerMap.GetAddress(registers[i], port, _bank);
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

        protected void WriteRegister(Mcp23PortRegister register, int port, byte value)
        {
            lock (_bankLock)
            {
                _mcpDevice.WriteRegister(_registerMap.GetAddress(register, port, _bank), value);
            }
        }

        protected void WriteRegisters(params (Mcp23PortRegister register, int port, byte value)[] writeOps)
        {
            lock (_bankLock)
            {
                byte dataStartAddress = 0x00;
                ushort dataStartIndex = 0;
                for (ushort i = 0; i < writeOps.Length; i++)
                {
                    var nextAddress = _registerMap.GetAddress(writeOps[i].register, writeOps[i].port, _bank);
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

        protected void WriteRegisters(int port, params (Mcp23PortRegister register, byte value)[] writeOps)
        {
            lock (_bankLock)
            {
                byte dataStartAddress = 0x00;
                ushort dataStartIndex = 0;
                for (ushort i = 0; i < writeOps.Length; i++)
                {
                    var nextAddress = _registerMap.GetAddress(writeOps[i].register, port, _bank);
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

        protected void WriteToAllPorts(Mcp23PortRegister register, byte value)
        {
            WriteRegisters(Enumerable.Range(0, Ports.Count).Select(port => (register, port, value)).ToArray());
        }

        protected void WriteToAllPorts(params (Mcp23PortRegister register, byte value)[] writeOps)
        {
            if (writeOps == null || writeOps.Length == 0)
            {
                return;
            }

            lock (_bankLock)
            {
                switch (_bank)
                {
                    case BankConfiguration.Paired:
                    {
                        var addresses = Enumerable.Range(0, writeOps.Length * Ports.Count)
                            .Select(
                                i => (writeOps[i / Ports.Count].register, port: i % Ports.Count,
                                    writeOps[i / Ports.Count].value))
                            .ToArray();
                        WriteRegisters(addresses);
                        return;
                    }
                    case BankConfiguration.Segregated:
                    {
                        var addresses = Enumerable.Range(0, writeOps.Length * Ports.Count)
                            .Select(
                                i => (writeOps[i % writeOps.Length].register, port: i / writeOps.Length,
                                    writeOps[i % writeOps.Length].value))
                            .ToArray();
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
        public IAnalogInputPort CreateAnalogInputPort(IPin pin, float voltageReference = 3.3f)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        public IPwmPort CreatePwmPort(IPin pin, float frequency = 100, float dutyCycle = 0.5f, bool invert = false)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
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
        public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, SpiClockConfiguration config)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, long speedkHz = 375)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        public II2cBus CreateI2cBus(IPin[] pins, int frequencyHz = 100000)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        public II2cBus CreateI2cBus(IPin clock, IPin data, int frequencyHz = 100000)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        public void SetClock(DateTime dateTime)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Not implemented, do not use.
        /// </summary>
        public DeviceCapabilities Capabilities => throw new NotImplementedException();

        #endregion
    }
}
