using Meadow.Hardware;
using Meadow.Utilities;
using System;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a MCP23008 port expander.
    /// </summary>
    abstract partial class Mcp23xxx : IDigitalInputOutputController
    {
        /// <summary> 
        /// Raised when the value of any pin configured for input interrupts changes.
        /// This provides raw port state data from the MCP23xxx.
        /// It's highly recommended to prefer using the events exposed on the digital input ports instead.
        /// </summary>
        public event EventHandler<IOExpanderInputChangedEventArgs> InputChanged = delegate { };

        /// <summary>
        /// The number of IO pins avaliable on the device
        /// </summary>
        public abstract int NumberOfPins { get; }

        private readonly IMcpDeviceComms mcpDevice;
        private readonly IDigitalInputPort interruptPortA;
        private readonly IDigitalInputPort interruptPortB;
        private readonly IDictionary<IPin, DigitalInputPort> inputPorts;

        // state
        byte ioDir;
        byte gpio;
        byte olat;
        byte gppu;
        byte iocon;

        PortBankType portBankType = PortBankType.Segregated; //default

        /// <summary>
        /// object for using lock() to do thread sync
        /// </summary>
        protected object _lock = new object();

        /// <summary>
        /// Instantiates an Mcp23008 on the specified I2C bus, with the specified
        /// peripheral address.
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="interruptPort">optional interupt port, needed for input interrupts</param>
        public Mcp23xxx(II2cBus i2cBus, byte address, IDigitalInputPort interruptPort = null) :
            this(new I2cMcpDeviceComms(i2cBus, address), interruptPort) // use the internal constructor that takes an IMcpDeviceComms
        {
        }

        /// <summary>
        /// Mcpxxx base class contructor
        /// peripheral address.
        /// </summary>
        /// <param name="spiBus"></param>
        /// <param name="chipSelectPort">Chip select port</param>
        /// <param name="interruptPort">optional interupt port, needed for input interrupts</param>
        public Mcp23xxx(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalInputPort interruptPort = null) :
            this(new SpiMcpDeviceComms(spiBus, chipSelectPort), interruptPort) // use the internal constructor that takes an IMcpDeviceComms
        {
        }

        /// <summary>
        /// Mcp23xxx base class
        /// </summary>
        /// <param name="device"></param>
        /// <param name="interruptPort"></param>
        internal Mcp23xxx(IMcpDeviceComms device, IDigitalInputPort interruptPort = null)
        {   // TODO: more interrupt stuff to solve
            // at a minimum, we need to check the interrupt mode and make sure
            // it's correct, raise an exception if not. also, doc in constructor
            // what we expect from an interrupt port.
            //interruptPort.InterruptMode = InterruptMode.EdgeRising;
            if (interruptPort != null)
            {
                this.interruptPortA = interruptPort;
                this.interruptPortA.Changed += HandleChangedInterrupt;
            }

            inputPorts = new Dictionary<IPin, DigitalInputPort>();
            mcpDevice = device;

            Initialize();
        }

        void HandleChangedInterrupt(object sender, DigitalPortResult e)
        {
            {   // sus out which pin fired
                byte interruptFlag = mcpDevice.ReadRegister(Registers.InterruptFlag);
                byte currentStates = mcpDevice.ReadRegister(Registers.GPIO);

                //Console.WriteLine($"Input flag:          {intflag:X2}");
                //Console.WriteLine($"Input Current Value: {currentValues:X2}");

                foreach(var port in inputPorts)
                {   //looks ugly but it's correct
                    var state = BitHelpers.GetBitValue(currentStates, (byte)port.Key.Key);
                    port.Value.Update(state);
                }
                InputChanged?.Invoke(this, new IOExpanderInputChangedEventArgs(interruptFlag, currentStates));
            }
        }

        /// <summary>
        /// Initializes the chip for use:
        /// * Puts all IOs into an input state
        /// * zeros out all setting and state registers
        /// </summary>
        protected virtual void Initialize()
        {
            if(NumberOfPins == 8)
            {
                mcpDevice.WriteRegister(Registers.IODirection, 0xFF); // set all the other registers to zeros (we skip the last one, output latch)
                mcpDevice.WriteRegister(Registers.InputPolarity, 0x00);
                mcpDevice.WriteRegister(Registers.GPIO, 0x00);
            }
            else
            {
                mcpDevice.WriteRegister(Registers.IODirection, new byte[] {0xFF, 0xFF}); // set all the other registers to zeros (we skip the last one, output latch)
                mcpDevice.WriteRegister(Registers.InputPolarity, new byte[] { 0, 0 });
                mcpDevice.WriteRegister(Registers.GPIO, new byte[] { 0, 0 });
            }
            
            mcpDevice.WriteRegister(Registers.InterruptOnChange, 0x00);
            mcpDevice.WriteRegister(Registers.DefaultComparisonValue, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptControl, 0x00);
            mcpDevice.WriteRegister(Registers.IOConfiguration, 0x00);
            mcpDevice.WriteRegister(Registers.PullupResistorConfiguration, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptFlag, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptCapture, 0x00);
            

            // save our state
            ioDir = 0xFF;
            gpio = 0x00;
            olat = 0x00;
            gppu = 0x00;
            iocon = 0x00;

            ioDir = mcpDevice.ReadRegister(Registers.IODirection);
            gpio = mcpDevice.ReadRegister(Registers.GPIO);
            olat = mcpDevice.ReadRegister(Registers.OutputLatch);

            bool intHigh = true;
            if(interruptPortA.Resistor == ResistorMode.InternalPullUp || 
               interruptPortA.Resistor == ResistorMode.ExternalPullUp)
            {
                intHigh = false;
            }

            iocon = BitHelpers.SetBit(iocon, 0x01, intHigh); //set interrupt pin to active high (true), low (false)
            iocon = BitHelpers.SetBit(iocon, 0x02, false); //don't set interrupt to open drain (should be the default)

            mcpDevice.WriteRegister(Registers.IOConfiguration, iocon);

            // Clear out I/O Settings
            mcpDevice.WriteRegister(Registers.DefaultComparisonValue, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptOnChange, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptControl, 0x00);
            mcpDevice.WriteRegister(Registers.InputPolarity, 0x00);
        }

        /// <summary>
        /// Checks whether or not the pin passed in exists on the chip.
        /// </summary>
        protected abstract bool IsValidPin(IPin pin);

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state.
        /// </summary>
        /// <param name="pin">The pin number to create the port on.</param>
        /// <param name="initialState">Whether the pin is initially high or low.</param>
        /// <param name="outputType"></param>
        /// <returns></returns>
        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState = false, OutputType outputType = OutputType.OpenDrain)
        {
            if (IsValidPin(pin))
            {   // setup the port internally for output
                SetPortDirection(pin, PortDirectionType.Output);

                // create the convenience class
                return new DigitalOutputPort(this, pin, initialState);
            }

            throw new Exception("Pin is out of range");
        }

        public IDigitalInputPort CreateDigitalInputPort(IPin pin)
        {
            return CreateDigitalInputPort(pin, InterruptMode.None, ResistorMode.Disabled, TimeSpan.Zero, TimeSpan.Zero);
        }

        public IDigitalInputPort CreateDigitalInputPort(
            IPin pin,
            InterruptMode interruptMode = InterruptMode.None,
            ResistorMode resistorMode = ResistorMode.Disabled)
        {
            return CreateDigitalInputPort(pin, interruptMode, resistorMode, TimeSpan.Zero, TimeSpan.Zero);
        }

        public IDigitalInputPort CreateDigitalInputPort(
            IPin pin,
            InterruptMode interruptMode,
            ResistorMode resistorMode,
            TimeSpan debounceDuration,
            TimeSpan glitchFilterCycleCount)
        {
            if (IsValidPin(pin))
            {
                if (resistorMode == ResistorMode.InternalPullDown)
                {
                    Console.WriteLine("Pull-down resistor mode is not supported.");
                    throw new Exception("Pull-down resistor mode is not supported.");
                }
                ConfigureMcpInputPort(pin, resistorMode == ResistorMode.InternalPullUp, interruptMode);

                var port = new DigitalInputPort(pin, interruptMode);

                inputPorts.Add(pin, port);
                return port;
            }
            throw new Exception("Pin is out of range");
        }

        /// <summary>
        /// Sets the direction of a particular port
        /// </summary>
        /// <param name="pin">The pin representing the port</param>
        /// <param name="direction">The direction to change to</param>
        public void SetPortDirection(IPin pin, PortDirectionType direction)
        {
            if (IsValidPin(pin))
            {   // if it's already configured, get out. (1 = input, 0 = output)
                if (direction == PortDirectionType.Input)
                {
                    if (BitHelpers.GetBitValue(ioDir, (byte)pin.Key)) { return; }
                }
                else
                {
                    if (!BitHelpers.GetBitValue(ioDir, (byte)pin.Key)) { return; }
                }

                // set the IODIR bit and write the setting
                ioDir = BitHelpers.SetBit(ioDir, (byte)pin.Key, (byte)direction);
                mcpDevice.WriteRegister(Registers.IODirection, ioDir);
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

                gppu = mcpDevice.ReadRegister(Registers.PullupResistorConfiguration);
                gppu = BitHelpers.SetBit(gppu, (byte)pin.Key, enablePullUp);
                mcpDevice.WriteRegister(Registers.PullupResistorConfiguration, gppu);

                if (interruptMode != InterruptMode.None)
                {
                    // interrupt on change (whether or not we want to raise an interrupt on the interrupt pin on change)
                    byte gpinten = mcpDevice.ReadRegister(Registers.InterruptOnChange);
                    gpinten = BitHelpers.SetBit(gpinten, (byte)pin.Key, true);
                    mcpDevice.WriteRegister(Registers.InterruptOnChange, gpinten);
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
                SetPortDirection(pin, PortDirectionType.Output);

                // update our output latch 
                olat = BitHelpers.SetBit(olat, (byte)pin.Key, value);

                // write to the output latch (actually does the output setting)
                mcpDevice.WriteRegister(Registers.OutputLatch, olat);
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

                // update our GPIO values
                gpio = mcpDevice.ReadRegister(Registers.GPIO);

                // return the value on that port
                return BitHelpers.GetBitValue(gpio, (byte)pin.Key);
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
            if (ioDir != 0)
            {
                ioDir = 0;
                mcpDevice.WriteRegister(Registers.IODirection, ioDir);
            }
            // write the output
            olat = mask;
            mcpDevice.WriteRegister(Registers.OutputLatch, olat);
        }

        /// <summary>
        /// Reads a byte value from all of the pins. little-endian; the least
        /// significant bit is the value of GP0. So a byte value of 0x60, or
        /// 0110 0000, means that pins GP5 and GP6 are high.
        /// </summary>
        /// <returns>A little-endian byte mask of the pin values.</returns>
        public byte ReadFromPorts()
        {   // set all IO to input
            if (ioDir != 1)
            {
                ioDir = 1;
                mcpDevice.WriteRegister(Registers.IODirection, ioDir);
            }
            // read the input
            gpio = mcpDevice.ReadRegister(Registers.GPIO);
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
        /// <returns>The  address of the register</returns>
        byte MapAddress(byte address, Port port = Port.PortA, PortBankType bankStyle = PortBankType.Segregated)
        {
            // There is no mapping for 8 bit expanders
            if (NumberOfPins == 8) { return address; }

            if (bankStyle == PortBankType.Segregated)
            {   // Registers for each bank are sequential
                // (IODIRA = 0x00, IODIRB = 0x01, IPOLA = 0x02, IPOLB = 0x03, ...)
                address += address;
                return port == Port.PortA ? address : ++address;
            }
            // Registers for each bank are separated
            // (IODIRA = 0x00, ... OLATA = 0x0A, IODIRB = 0x10, ... OLATB = 0x1A)
            return port == Port.PortA ? address : address += 0x10;
        }
    }
}