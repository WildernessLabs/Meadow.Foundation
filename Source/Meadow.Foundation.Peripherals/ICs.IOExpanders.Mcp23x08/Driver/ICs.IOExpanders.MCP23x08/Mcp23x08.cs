using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a MCP23008 port expander.
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
        public event EventHandler<IOExpanderInputChangedEventArgs> InputChanged = delegate { };

        private readonly IMcpDeviceComms _mcpDevice;
        private readonly IDigitalInputPort _interruptPort;
        private IDictionary<IPin, DigitalInputPort> _inputPorts;

        public PinDefinitions Pins { get; } = new PinDefinitions();

        // state
        byte _iodir;
        byte _gpio;
        byte _olat;
        byte _gppu;
        byte _iocon;

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
            this(new I2cMcpDeviceComms(i2cBus, address), interruptPort)
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
            if (this._interruptPort != null)
            {
                this._interruptPort.Changed += HandleChangedInterrupt;
            }
            _inputPorts = new Dictionary<IPin, DigitalInputPort>();
            _mcpDevice = device;
            Initialize();
        }

        protected void HandleChangedInterrupt(object sender, EventArgs e)
        {
            try
            {
                // sus out which pin fired
                byte intflag = this._mcpDevice.ReadRegister(RegisterAddresses.InterruptFlagRegister);
                byte currentValues = this._mcpDevice.ReadRegister(RegisterAddresses.GPIORegister);

                //Console.WriteLine($"Input flag:          {intflag:X2}");
                //Console.WriteLine($"Input Current Value: {currentValues:X2}");

                this.RaiseInputChanged(intflag, currentValues);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        protected void RaiseInputChanged(byte interruptPins, byte currentStates)
        {
            InputChanged?.Invoke(this, new IOExpanderInputChangedEventArgs(interruptPins, currentStates));
        }

        /// <summary>
        /// Initializes the chip for use:
        ///  * Puts all IOs into an input state
        ///  * zeros out all setting and state registers
        /// </summary>
        protected void Initialize()
        {
            byte[] buffers = new byte[10];

            // IO Direction
            buffers[0] = 0xFF; //all input `11111111`

            // set all the other registers to zeros (we skip the last one, output latch)
            for (int i = 1; i < 10; i++)
            {
                buffers[i] = 0x00; //all zero'd out `00000000`
            }

            // the chip will automatically write all registers sequentially.
            _mcpDevice.WriteRegisters(RegisterAddresses.IODirectionRegister, buffers);

            // save our state
            _iodir = buffers[0];
            _gpio = 0x00;
            _olat = 0x00;
            _gppu = 0x00;
            _iocon = 0x00;

            _iodir = _mcpDevice.ReadRegister(RegisterAddresses.IODirectionRegister);
            _gpio = _mcpDevice.ReadRegister(RegisterAddresses.GPIORegister);
            _olat = _mcpDevice.ReadRegister(RegisterAddresses.OutputLatchRegister);

            _iocon = BitHelpers.SetBit(_iocon, (byte)0x01, true);
            _iocon = BitHelpers.SetBit(_iocon, (byte)0x02, false);
            _mcpDevice.WriteRegister(RegisterAddresses.IOConfigurationRegister, _iocon);

            // Clear out I/O Settings
            _mcpDevice.WriteRegister(RegisterAddresses.DefaultComparisonValueRegister, 0x00);
            _mcpDevice.WriteRegister(RegisterAddresses.InterruptOnChangeRegister, 0x00);
            _mcpDevice.WriteRegister(RegisterAddresses.InterruptControlRegister, 0x00);
            _mcpDevice.WriteRegister(RegisterAddresses.InputPolarityRegister, 0x00);
        }

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state.
        /// </summary>
        /// <param name="pin">The pin number to create the port on.</param>
        /// <param name="initialState">Whether the pin is initially high or low.</param>
        /// <returns></returns>
        public IDigitalOutputPort CreateDigitalOutputPort(
            IPin pin, bool initialState = false, OutputType outputType = OutputType.OpenDrain)
        {
            if (IsValidPin(pin))
            {
                // setup the port internally for output
                this.SetPortDirection(pin, PortDirectionType.Output);

                // create the convenience class
                return new DigitalOutputPort(this, pin, initialState);
            }

            throw new Exception("Pin is out of range");
        }

        public IDigitalInputPort CreateDigitalInputPort(
            IPin pin,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled,
            double debounceDuration = 0,
            double glitchFilterCycleCount = 0)
        {
            if (IsValidPin(pin))
            {
                if (resistorMode == ResistorMode.InternalPullDown)
                {
                    Console.WriteLine("Pull-down resistor mode is not supported.");
                    throw new Exception("Pull-down resistor mode is not supported.");
                }
                var enablePullUp = resistorMode == ResistorMode.InternalPullUp ? true : false;
                this.ConfigureInputPort(pin, enablePullUp, interruptMode);
                var port = new DigitalInputPort(this, pin, interruptMode);
                _inputPorts.Add(pin, port);
                return port;
            }
            throw new Exception("Pin is out of range");
        }

        /// <summary>
        /// Sets the direction of a particular port.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="direction"></param>
        public void SetPortDirection(IPin pin, PortDirectionType direction)
        {
            if (IsValidPin(pin))
            {
                // if it's already configured, get out. (1 = input, 0 = output)
                if (direction == PortDirectionType.Input)
                {
                    if (BitHelpers.GetBitValue(_iodir, (byte)pin.Key)) return;
                    //if ((_iodir & (byte)(1 << pin)) != 0) return;
                }
                else
                {
                    if (!BitHelpers.GetBitValue(_iodir, (byte)pin.Key)) return;
                    //if ((_iodir & (byte)(1 << pin)) == 0) return;
                }

                // set the IODIR bit and write the setting
                _iodir = BitHelpers.SetBit(_iodir, (byte)pin.Key, (byte)direction);
                _mcpDevice.WriteRegister(RegisterAddresses.IODirectionRegister, _iodir);
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
        }

        public void ConfigureInputPort(IPin pin, bool enablePullUp = false, InterruptMode interruptMode = InterruptMode.None)
        {
            if (IsValidPin(pin))
            {
                // set the port direction
                this.SetPortDirection(pin, PortDirectionType.Input);

                _gppu = _mcpDevice.ReadRegister(RegisterAddresses.PullupResistorConfigurationRegister);
                _gppu = BitHelpers.SetBit(_gppu, (byte)pin.Key, enablePullUp);
                _mcpDevice.WriteRegister(RegisterAddresses.PullupResistorConfigurationRegister, _gppu);

                if (interruptMode != InterruptMode.None)
                {
                    // interrupt on change (whether or not we want to raise an interrupt on the interrupt pin on change)
                    byte gpinten = _mcpDevice.ReadRegister(RegisterAddresses.InterruptOnChangeRegister);
                    gpinten = BitHelpers.SetBit(gpinten, (byte)pin.Key, true);
                    _mcpDevice.WriteRegister(RegisterAddresses.InterruptOnChangeRegister, gpinten);

                    // Set the default value for the pin for interrupts.
                    var interruptValue = interruptMode == InterruptMode.EdgeFalling;
                    byte defVal = _mcpDevice.ReadRegister(RegisterAddresses.DefaultComparisonValueRegister);
                    defVal = BitHelpers.SetBit(defVal, (byte)pin.Key, interruptValue);
                    _mcpDevice.WriteRegister(RegisterAddresses.DefaultComparisonValueRegister, defVal);

                    // Set the input polarity of the pin. Basically if its normally high, we want to flip the polarity.
                    var pol = _mcpDevice.ReadRegister(RegisterAddresses.InputPolarityRegister);
                    pol = BitHelpers.SetBit(pol, (byte)pin.Key, !interruptValue);
                    _mcpDevice.WriteRegister(RegisterAddresses.InputPolarityRegister, pol);

                    // interrupt control register; whether or not the change is based 
                    // on default comparison value, or if a change from previous. We 
                    // want to raise on change, so we set it to 0, always.
                    var interruptControl = interruptMode != InterruptMode.EdgeBoth;
                    var intCon = _mcpDevice.ReadRegister(RegisterAddresses.InterruptControlRegister);
                    intCon = BitHelpers.SetBit(intCon, (byte)pin.Key, interruptControl);
                    _mcpDevice.WriteRegister(RegisterAddresses.InterruptControlRegister, intCon);
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
                // if the pin isn't configured for output, configure it
                this.SetPortDirection(pin, PortDirectionType.Output);

                // update our output latch 
                _olat = BitHelpers.SetBit(_olat, (byte)pin.Key, value);

                // write to the output latch (actually does the output setting)
                _mcpDevice.WriteRegister(RegisterAddresses.OutputLatchRegister, _olat);
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
            if (_iodir != 0)
            {
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
            if (_iodir != 1)
            {
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

        public IBiDirectionalPort CreateBiDirectionalPort(IPin pin,
            bool initialState = false,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled,
            PortDirectionType initialDirection = PortDirectionType.Input,
            double debounceDuration = 0.0,    // 0 - 1000 msec in .1 increments
            double glitchDuration = 0.0,      // 0 - 1000 msec in .1 increments
            OutputType outputType = OutputType.PushPull)
        {
            throw new NotImplementedException();
        }

        //TODO: we know adding all these sucks. when we convert to .NET Core
        // we'll be able to add these to the IIODevice interface implementation
        // and they won't be necessary to put in like this.

        public IAnalogInputPort CreateAnalogInputPort(IPin pin, float voltageReference = 3.3F)
        {
            throw new NotImplementedException();
        }

        public ISerialPort CreateSerialPort(SerialPortName portName, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int readBufferSize = 1024)
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

        public ISpiBus CreateSpiBus(IPin clock, IPin mosi, IPin miso, SpiClockConfiguration config)
        {
            throw new NotImplementedException();
        }

        public II2cBus CreateI2cBus(IPin[] pins, int frequencyHz = 100000)
        {
            throw new NotImplementedException();
        }

        public II2cBus CreateI2cBus(IPin clock, IPin data, int frequencyHz = 100000)
        {
            throw new NotImplementedException();
        }

        public void SetClock(DateTime dateTime)
        {
            throw new NotImplementedException();
        }

        public ISerialMessagePort CreateSerialMessagePort(SerialPortName portName, byte[] suffixDelimiter, bool preserveDelimiter, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int readBufferSize = 512)
        {
            throw new NotImplementedException();
        }

        public ISerialMessagePort CreateSerialMessagePort(SerialPortName portName, byte[] prefixDelimiter, bool preserveDelimiter, int messageLength, int baudRate = 9600, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One, int readBufferSize = 512)
        {
            throw new NotImplementedException();
        }

        public void SetSynchronizationContext(SynchronizationContext context)
        {
            throw new NotImplementedException();
        }

        public IPin GetPin(string pinName)
        {
            return Pins.AllPins.FirstOrDefault(p => p.Name == pinName || p.Key.ToString() == p.Name);
        }
    }
}