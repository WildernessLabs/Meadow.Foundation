using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a MCP23008 port expander.
    /// 
    /// Note: this class is not yet implemented.
    /// </summary>
    public partial class Mcp23x08 : IIODevice
    {
        /// <summary>
        /// Raised when the value of a pin configured for input changes. Use in
        /// conjunction with parallel port reads via ReadFromPorts(). When using
        /// individual `DigitalInputPort` objects, each one will have their own
        /// `Changed` event
        /// </summary>
        // TODO: make a custom event args that has the pin that triggered
        public event EventHandler InputChanged = delegate { };

        private readonly IMcpDeviceComms _mcpDevice;
        private readonly IDigitalInputPort _interruptPort;

        public PinDefinitions Pins { get; } = new PinDefinitions();

        // state
        byte _iodir;
        byte _gpio;
        byte _olat;
        byte _gppu;

        /// <summary>
        ///     object for using lock() to do thread synch
        /// </summary>
        protected object _lock = new object();

        public DeviceCapabilities Capabilities => throw new NotImplementedException();

        protected Mcp23x08()
        { }

        /// <summary>
        /// Instantiates an Mcp23008 on the specified I2C bus using the appropriate
        /// peripheral address based on the pin settings. Use this method if you
        /// don't want to calculate the address.
        /// </summary>
        /// <param name="i2cBus"></param>
        /// <param name="pinA0">Whether or not Address0 pin is pulled high.</param>
        /// <param name="pinA1">Whether or not Address1 pin is pulled high.</param>
        /// <param name="pinA2">Whether or not Address2 pin is pulled high.</param>
        /// <param name="interruptPort">Optional IDigitalInputPort used to support
        /// interrupts. The MCP will notify a single port for an interrupt on
        /// any input configured pin. The driver takes care of looking up which
        /// pin the interrupt occurred on, and will raise it on that port, if a port
        /// is used.</param>
        public Mcp23x08(II2cBus i2cBus, bool pinA0, bool pinA1, bool pinA2,
            IDigitalInputPort interruptPort = null)
            : this(i2cBus, McpAddressTable.GetAddressFromPins(pinA0, pinA1, pinA2))
        {
            // nothing goes here
        }

        /// <summary>
        /// Instantiates an Mcp23008 on the specified I2C bus, with the specified
        /// peripheral address.
        /// </summary>
        /// <param name="i2cBus"></param>
        /// <param name="address"></param>
        public Mcp23x08(II2cBus i2cBus, byte address = 0x20,
            IDigitalInputPort interruptPort = null) :
            // use the internal constructor that takes an IMcpDeviceComms
            this (new I2cMcpDeviceComms(i2cBus, address), interruptPort)
        {
            // nothing goes here
        }

        public Mcp23x08(ISpiBus spiBus, IDigitalOutputPort chipSelect,
            bool pinA0, bool pinA1,
            IDigitalInputPort interruptPort = null) :
            this(new SpiMcpDeviceComms(spiBus, chipSelect,
                McpAddressTable.GetAddressFromPins(pinA0, pinA1, false)),
                interruptPort)
        {
            // nothing goes here
        }

        public Mcp23x08(ISpiBus spiBus, IDigitalOutputPort chipSelect,
            byte address = 0x20, // all low 
            IDigitalInputPort interruptPort = null) :
            this (new SpiMcpDeviceComms(spiBus, chipSelect, address), interruptPort)
        {
            // nothing goes here
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="interruptPort"></param>
        internal Mcp23x08(
            IMcpDeviceComms device,
            IDigitalInputPort interruptPort = null)
        {
            // save our interrupt pin
            this._interruptPort = interruptPort;
            // TODO: more interrupt stuff to solve
            // at a minimum, we need to check the interrupt mode and make sure
            // it's correct, raise an exception if not. also, doc in constructor
            // what we expect from an interrupt port.
            //this._interruptPort.InterruptMode = InterruptMode.EdgeRising;
            if (this._interruptPort != null) {
                this._interruptPort.Changed += HandleChangedInterrupt;
            }

            _mcpDevice = device;
            Initialize();
        }

        protected void HandleChangedInterrupt(object sender, EventArgs e) {

            Console.WriteLine("Interrupt triggered.");

            this.RaiseInputChanged();

            // sus out which pin fired
            var intcap = this._mcpDevice.ReadRegister(RegisterAddresses.InterruptCaptureRegister);

            Console.WriteLine($"Input mask: { BitConverter.ToString(new byte[] { intcap }).Replace("-", " ") }");

            // raise the event

        }

        protected void RaiseInputChanged() {
            // TODO: return the value of the inputs as a byte
            this.InputChanged(this, new EventArgs());
        }


        /// <summary>
        /// Initializes the chip for use:
        ///  * Puts all IOs into an input state
        ///  * zeros out all setting and state registers
        /// </summary>
        protected void Initialize()
        {
            Console.WriteLine("Mcp23x08.Initialize()");

            byte[] buffers = new byte[10];

            // IO Direction
            buffers[0] = 0xFF; //all input `11111111`

            // set all the other registers to zeros (we skip the last one, output latch)
            for (int i = 1; i < 10; i++ ) {
                buffers[i] = 0x00; //all zero'd out `00000000`
            }

            Console.WriteLine("here 1");

            // the chip will automatically write all registers sequentially.
            _mcpDevice.WriteRegisters(RegisterAddresses.IODirectionRegister, buffers);

            Console.WriteLine("here 2");

            // save our state
            _iodir = buffers[0];
            _gpio = 0x00;
            _olat = 0x00;
            _gppu = 0x00;

            // read in the initial state of the chip
            _iodir = _mcpDevice.ReadRegister(RegisterAddresses.IODirectionRegister);
            // tried some sleeping, but also has no effect on its reliability
            Console.WriteLine($"IODIR: {_iodir.ToString("X")}");
            _gpio = _mcpDevice.ReadRegister(RegisterAddresses.GPIORegister);
            Console.WriteLine($"GPIO: {_gpio.ToString("X")}");
            _olat = _mcpDevice.ReadRegister(RegisterAddresses.OutputLatchRegister);
            Console.WriteLine($"OLAT: {_olat.ToString("X")}");

        }

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state.
        /// </summary>
        /// <param name="pin">The pin number to create the port on.</param>
        /// <param name="initialState">Whether the pin is initially high or low.</param>
        /// <returns></returns>
        public IDigitalOutputPort CreateDigitalOutputPort(
            IPin pin, bool initialState = false)
        {
            if (IsValidPin(pin)) {
                // setup the port internally for output
                this.SetPortDirection(pin, PortDirectionType.Output);

                // create the convenience class
                return new DigitalOutputPort(this, pin, initialState);
            }

            throw new Exception("Pin is out of range");
        }

        public IDigitalInputPort CreateDigitalInputPort(
            IPin pin,
            //TODO the MCP basically supports yes/no for this. so we may need to
            // revisit the IIODevice interface here
            InterruptMode interruptMode = InterruptMode.None, 
            ResistorMode resistorMode = ResistorMode.Disabled,
            //TODO, also we probably don't want glitchFIlter, but we can support
            // debounce. 
            int debounceDuration = 0,
            int glitchFilterCycleCount = 0
            )
        {
            if (IsValidPin(pin)) {
                this.ConfigureInputPort(
                    pin,
                    resistorMode == ResistorMode.PullUp ? true : false,
                    interruptMode != InterruptMode.None ? true : false
                    );
                return new DigitalInputPort(this, pin, interruptMode);
            }
            throw new Exception("Pin is out of range");
        }

        /// <summary>
        /// Sets the direction of a particulare port.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="direction"></param>
        public void SetPortDirection(IPin pin, PortDirectionType direction)
        {
            if (IsValidPin(pin)) {
                // if it's already configured, get out. (1 = input, 0 = output)
                if (direction == PortDirectionType.Input) {
                    if (BitHelpers.GetBitValue(_iodir, (byte)pin.Key)) return;
                    //if ((_iodir & (byte)(1 << pin)) != 0) return;
                } else {
                    if (!BitHelpers.GetBitValue(_iodir, (byte)pin.Key)) return;
                    //if ((_iodir & (byte)(1 << pin)) == 0) return;
                }

                // set the IODIR bit and write the setting
                _iodir = BitHelpers.SetBit(_iodir, (byte)pin.Key, (byte)direction);
                _mcpDevice.WriteRegister(RegisterAddresses.IODirectionRegister, _iodir);
            }
            else {
                throw new Exception("Pin is out of range");
            }
        }

        public void ConfigureInputPort(IPin pin, bool enablePullUp = false, bool enableInterrupt = true)
        {
            if (IsValidPin(pin)) {
                // set the port direction
                this.SetPortDirection(pin, PortDirectionType.Input);

                // refresh out pull up state
                // TODO: do away with this and trust internal state?
                _gppu = _mcpDevice.ReadRegister(RegisterAddresses.PullupResistorConfigurationRegister);

                _gppu = BitHelpers.SetBit(_gppu, (byte)pin.Key, enablePullUp);

                _mcpDevice.WriteRegister(RegisterAddresses.PullupResistorConfigurationRegister, _gppu);

                if (enableInterrupt) {
                    Console.WriteLine($"Enabling interrupt");
                    // interrupt on change (whether or not we want to raise an interrupt on the interrupt pin on change)
                    byte gpinten = _mcpDevice.ReadRegister(RegisterAddresses.InterruptOnChangeRegister);
                    gpinten = BitHelpers.SetBit(gpinten, (byte)pin.Key, true);

                    // interrupt control register; whether or not the change is based 
                    // on default comparison value, or if a change from previous. We 
                    // want to raise on change, so we set it to 0, always.
                    byte interruptControl = _mcpDevice.ReadRegister(RegisterAddresses.InterruptControlRegister);
                    interruptControl = BitHelpers.SetBit(interruptControl, (byte)pin.Key, false);
                }
            } else {
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
            if (IsValidPin(pin)) {
                // if the pin isn't configured for output, configure it
                this.SetPortDirection(pin, PortDirectionType.Output);

                // update our output latch 
                _olat = BitHelpers.SetBit(_olat, (byte)pin.Key, value);

                // write to the output latch (actually does the output setting)
                _mcpDevice.WriteRegister(RegisterAddresses.OutputLatchRegister, _olat);
            }
            else {
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
            if (IsValidPin(pin)) {
                // if the pin isn't set for input, configure it
                this.SetPortDirection(pin, PortDirectionType.Input);

                // update our GPIO values
                _gpio = _mcpDevice.ReadRegister(RegisterAddresses.GPIORegister);

                // return the value on that port
                return BitHelpers.GetBitValue(_gpio, (byte)pin.Key);
            }

            throw new Exception("Pin is out of range");
        }

        /// <summary>
        /// Outputs a byte value across all of the pins by writing directly 
        /// to the output latch (OLAT) register.
        /// </summary>
        /// <param name="mask"></param>
        public void WriteToPorts(byte mask)
        {
            // set all IO to output
            if (_iodir != 0) {
                _iodir = 0;
                _mcpDevice.WriteRegister(RegisterAddresses.IODirectionRegister, _iodir);
            }
            // write the output
            _olat = mask;
            _mcpDevice.WriteRegister(RegisterAddresses.OutputLatchRegister, _olat);
        }

        /// <summary>
        /// Reads a byte value from all of the pins. little-endian; the least
        /// significant bit is the value of GP0. So a byte value of 0x60, or
        /// 0110 0000, means that pins GP5 and GP6 are high.
        /// </summary>
        /// <returns>A little-endian byte mask of the pin values.</returns>
        public byte ReadFromPorts()
        {
            // set all IO to input
            if (_iodir != 1) {
                _iodir = 1;
                _mcpDevice.WriteRegister(RegisterAddresses.IODirectionRegister, _iodir);
            }
            // read the input
            _gpio = _mcpDevice.ReadRegister(RegisterAddresses.GPIORegister);
            return _gpio;
        }

        /// <summary>
        /// Checks whether or not the pin passed in exists on the chip.
        /// </summary>
        protected bool IsValidPin(IPin pin)
        {
            var contains = this.Pins.AllPins.Contains(pin);
            return (this.Pins.AllPins.Contains(pin));
        }

        /// <summary>
        /// Sets the pin back to an input
        /// </summary>
        /// <param name="pin"></param>
        protected void ResetPin(IPin pin)
        {
            this.SetPortDirection(pin, PortDirectionType.Input);
        }


        // TODO: all these can go away when we get interface implementation 
        // support from C# 8 into the Meadow.Core project. It won't work today,
        // even though it's set to C# 8 because the project references the
        // .NET 4.7.2 runtime. After the latest Mono rebase we'll be able to
        // move it to Core 3.

        public IBiDirectionalPort CreateBiDirectionalPort(IPin pin, bool initialState = false, bool glitchFilter = false, InterruptMode interruptMode = InterruptMode.None, ResistorMode resistorMode = ResistorMode.Disabled, PortDirectionType initialDirection = PortDirectionType.Input)
        {
            throw new NotImplementedException();
        }

        public IAnalogInputPort CreateAnalogInputPort(IPin pin, float voltageReference = 3.3F)
        {
            throw new NotImplementedException();
        }

        public ISerialPort CreateSerialPort(SerialPortName portName, int baudRate, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int readBufferSize = 4096)
        {
            throw new NotImplementedException();
        }

        public ISpiBus CreateSpiBus(IPin[] pins, long speed)
        {
            throw new NotImplementedException();
        }

        public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, long speed)
        {
            throw new NotImplementedException();
        }

        public II2cBus CreateI2cBus(IPin[] pins, ushort speed = 100)
        {
            throw new NotImplementedException();
        }

        public II2cBus CreateI2cBus(IPin clock, IPin data, ushort speed = 100)
        {
            throw new NotImplementedException();
        }

        public IPwmPort CreatePwmPort(IPin pin, float frequency = 100, float dutyCycle = 0.5F, bool invert = false)
        {
            throw new NotImplementedException();
        }
    }
}