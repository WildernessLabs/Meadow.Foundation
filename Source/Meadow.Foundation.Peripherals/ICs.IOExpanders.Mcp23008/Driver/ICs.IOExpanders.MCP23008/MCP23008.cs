using System;
using Meadow.Hardware;
using Meadow.Utilities;


namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a MCP23008 port expander.
    /// 
    /// Note: this class is not yet implemented.
    /// </summary>
    public class MCP23008
    {
        /// <summary>
        ///     Raised on Interrupt
        /// </summary>
        public event EventHandler InterruptRaised = delegate { }; //ToDo - is this being used??

        private readonly II2cPeripheral _i2cPeripheral;

        // state
        byte _iodir;
        byte _gpio;
        byte _olat;
        byte _gppu;

        /// <summary>
        ///     object for using lock() to do thread synch
        /// </summary>
        protected object _lock = new object();

        // register addresses
        // IO Direction - controls the direction of the GPIO
        private const byte _IODirectionRegister = 0x00; //IODIR
        private const byte _InputPolarityRegister = 0x01; //IPOL
        private const byte _InterruptOnChangeRegister = 0x02; //GPINTEN
        private const byte _DefaultComparisonValueRegister = 0x03; //DEFVAL
        private const byte _InterruptControlRegister = 0x04; //INTCON
        private const byte _IOConfigurationRegister = 0x05; //IOCON
        private const byte _PullupResistorConfigurationRegister = 0x06; //GPPU
        private const byte _InterruptFlagRegister = 0x07; //INTF
        private const byte _InterruptCaptureRegister = 0x08; //INTCAP
        private const byte _GPIORegister = 0x09; //GPIO
        private const byte _OutputLatchRegister = 0x0A; //OLAT

        //// protected properties
        // don't think there's a lot of value in this.  it's enabled by default, and is good.
        //protected bool SequentialAddressOperationEnabled
        //{
        //    get {
        //        return _sequentialAddressOperationEnabled;
        //    }
        //    set {
        //        _i2cBus.WriteRegister(_IOConfigurationRegister, (byte)(value ? 1 : 0));
        //    }
        //} private bool _sequentialAddressOperationEnabled = false;


        protected MCP23008()
        { }


      /*  public MCP23008(bool pinA0, bool pinA1, bool pinA2, ushort speed = 100)
            : this(MCPAddressTable.GetAddressFromPins(pinA0, pinA1, pinA2), speed)
        {
            // nothing goes here
        } */

        public MCP23008(II2cBus i2cBus, byte address = 0x20)
        {
            // tried this, based on a forum post, but seems to have no effect.
            //H.OutputPort SDA = new H.OutputPort(N.Pins.GPIO_PIN_A4, false);
            //H.OutputPort SCK = new H.OutputPort(N.Pins.GPIO_PIN_A5, false);
            //SDA.Dispose();
            //SCK.Dispose();

            // configure our i2c bus so we can talk to the chip
            _i2cPeripheral = new I2cPeripheral(i2cBus, address);

            Console.WriteLine("initialized.");

            Initialize();

            Console.WriteLine("Chip Reset.");

            // read in the initial state of the chip
            _iodir = _i2cPeripheral.ReadRegister(_IODirectionRegister);
            // tried some sleeping, but also has no effect on its reliability
            //Thread.Sleep(100);
            //Console.WriteLine("IODIR: " + _iodir.ToString("X"));
            _gpio = _i2cPeripheral.ReadRegister(_GPIORegister);
            //Thread.Sleep(100);
            //Console.WriteLine("GPIO: " + _gpio.ToString("X"));
            _olat = _i2cPeripheral.ReadRegister(_OutputLatchRegister);
            //Thread.Sleep(100);
            //Console.WriteLine("OLAT: " + _olat.ToString("X")); 
        } 

        protected void Initialize()
        {
            byte[] buffers = new byte[10];

            // IO Direction
            buffers[0] = 0xFF; //all input `11111111`

            // set all the other registers to zeros (we skip the last one, output latch)
            for (int i = 1; i < 10; i++ ) {
                buffers[i] = 0x00; //all zero'd out `00000000`
            }

            // the chip will automatically write all registers sequentially.
            _i2cPeripheral.WriteRegisters(_IODirectionRegister, buffers);

            // save our state
            _iodir = buffers[0];
            _gpio = 0x00;
            _olat = 0x00;
            _gppu = 0x00;
        }

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state.
        /// </summary>
        /// <param name="pin">The pin number to create the port on.</param>
        /// <param name="initialState">Whether the pin is initially high or low.</param>
        /// <returns></returns>
        public DigitalOutputPort CreateOutputPort(byte pin, bool initialState)
        {
            if (IsValidPin(pin))
            {
                // setup the port internally for output
                this.SetPortDirection(pin, PortDirectionType.Output);

                // create the convenience class
                DigitalOutputPort port = null; //ToDo = new DigitalOutputPort(pin, initialState);

                // return the port
                return port;
            }

            throw new Exception("Pin is out of range");
        }

        public IDigitalInputPort CreateInputPort(IIODevice device, IPin pin, bool enablePullUp = false)
        {
            if (pin != null)
            {
                return device.CreateDigitalInputPort(pin, InterruptMode.None, ResistorMode.PullUp);
            }

            throw new Exception("Pin is out of range");
        }

        /// <summary>
        /// Sets the direction of a particulare port.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="direction"></param>
        public void SetPortDirection(byte pin, PortDirectionType direction)
        {
            if (IsValidPin(pin))
            {
                // if it's already configured, get out. (1 = input, 0 = output)
                if (direction == PortDirectionType.Input)
                {
                    if (BitHelpers.GetBitValue(_iodir, pin)) return;
                    //if ((_iodir & (byte)(1 << pin)) != 0) return;
                }
                else
                {
                    if (!BitHelpers.GetBitValue(_iodir, pin)) return;
                    //if ((_iodir & (byte)(1 << pin)) == 0) return;
                }

                // set the IODIR bit and write the setting
                _iodir = BitHelpers.SetBit(_iodir, (byte)pin, (byte)direction);
                _i2cPeripheral.WriteRegister(_IODirectionRegister, _iodir);
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
        }

        public void ConfigureInputPort(byte pin, bool enablePullUp = false, bool enableInterrupt = true)
        {
            if (IsValidPin(pin))
            {
                // set the port direction
                this.SetPortDirection(pin, PortDirectionType.Input);

                // refresh out pull up state
                // TODO: do away with this and trust internal state?
                _gppu = _i2cPeripheral.ReadRegister(_PullupResistorConfigurationRegister);

                _gppu = BitHelpers.SetBit(_gppu, pin, enablePullUp);

                _i2cPeripheral.WriteRegister(_PullupResistorConfigurationRegister, _gppu);

                if (enableInterrupt)
                {
                    // interrupt on change (whether or not we want to raise an interrupt on the interrupt pin on change)
                    byte gpinten = _i2cPeripheral.ReadRegister(_InterruptOnChangeRegister);
                    gpinten = BitHelpers.SetBit(gpinten, pin, true);

                    // interrupt control register; whether or not the change is based 
                    // on default comparison value, or if a change from previous. We 
                    // want to raise on change, so we set it to 0, always.
                    byte interruptControl = _i2cPeripheral.ReadRegister(_InterruptControlRegister);
                    interruptControl = BitHelpers.SetBit(interruptControl, pin, false);
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
        public void WriteToPort(byte pin, bool value)
        {
            if (IsValidPin(pin))
            {
                // if the pin isn't configured for output, configure it
                this.SetPortDirection((byte)pin, PortDirectionType.Output);

                // update our output latch 
                _olat = BitHelpers.SetBit(_olat, (byte)pin, value);

                // write to the output latch (actually does the output setting)
                _i2cPeripheral.WriteRegister(_OutputLatchRegister, _olat);
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
        }

        public bool ReadPort(byte pin)
        {
            if (IsValidPin(pin))
            {
                // if the pin isn't set for input, configure it
                this.SetPortDirection((byte)pin, PortDirectionType.Input);

                // update our GPIO values
                _gpio = _i2cPeripheral.ReadRegister(_GPIORegister);

                // return the value on that port
                return BitHelpers.GetBitValue(_gpio, (byte)pin);
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
                _i2cPeripheral.WriteRegister(_IODirectionRegister, _iodir);
            }
            // write the output
            _olat = mask;
            _i2cPeripheral.WriteRegister(_OutputLatchRegister, _olat);
        }

        protected bool IsValidPin(byte pin)
        {
            return (pin >= 0 && pin <= 7);
        }

        // what's a good way to do this? maybe constants? how to name?
        public enum ValidSpeeds : ushort
        {
            hundredk = 100,
            fourhundredk = 400,
            onepointsevenmegs = 17000,
        }
    }
}