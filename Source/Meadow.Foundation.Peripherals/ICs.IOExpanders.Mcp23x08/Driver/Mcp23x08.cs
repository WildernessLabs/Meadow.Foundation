﻿using Meadow.Hardware;
using Meadow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a MCP23008 port expander.
    /// </summary>
    public partial class Mcp23x08 : IDigitalInputController, IDigitalOutputController
    {
        /// <summary> 
        /// Raised when the value of any pin configured for input interrupts changes.
        /// This provides raw port state data from the MCP23xxx.
        /// It's highly recommended to prefer using the events exposed on the digital input ports instead.
        /// </summary>
        public event EventHandler<IOExpanderInputChangedEventArgs> InputChanged = delegate { };

        private readonly IMcpDeviceComms mcpDevice;
        private readonly IDigitalInputPort interruptPort;
        private readonly IDictionary<IPin, DigitalInputPort> inputPorts;

        /// <summary>
        /// MCP230x8 pin definitions
        /// </summary>
        public PinDefinitions Pins { get; } = new PinDefinitions();

        // state
        byte ioDir;
        byte gpio;
        byte olat;
        byte gppu;
        byte iocon;

        /// <summary>
        /// object for using lock() to do thread sync
        /// </summary>
        protected object _lock = new object();

        /// <summary>
        /// Instantiates an Mcp23008 on the specified I2C bus, with the specified
        /// peripheral address.
        /// </summary>
        /// <param name="i2cBus"></param>
        /// <param name="address"></param>
        /// <param name="interruptPort">optional interupt port</param>
        public Mcp23x08(II2cBus i2cBus, byte address = (byte)Addresses.Default, IDigitalInputPort interruptPort = null) :
            this(new I2cMcpDeviceComms(i2cBus, address), interruptPort) // use the internal constructor that takes an IMcpDeviceComms
        {
        }

        /// <summary>
        /// Instantiates an Mcp23008 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="interruptPort"></param>
        internal Mcp23x08(IMcpDeviceComms device, IDigitalInputPort interruptPort = null)
        {   // TODO: more interrupt stuff to solve
            // at a minimum, we need to check the interrupt mode and make sure
            // it's correct, raise an exception if not. also, doc in constructor
            // what we expect from an interrupt port.
            //interruptPort.InterruptMode = InterruptMode.EdgeRising;
            if (interruptPort != null)
            {
                this.interruptPort = interruptPort;
                this.interruptPort.Changed += HandleChangedInterrupt;
            }

            inputPorts = new Dictionary<IPin, DigitalInputPort>();
            mcpDevice = device;

            Initialize();
        }

        void HandleChangedInterrupt(object sender, DigitalPortResult e)
        {
            {   // sus out which pin fired
                byte interruptFlag = mcpDevice.ReadRegister(Registers.InterruptFlagRegister);
                byte currentStates = mcpDevice.ReadRegister(Registers.GPIORegister);

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
        protected void Initialize()
        {
            mcpDevice.WriteRegister(Registers.IODirectionRegister, 0xFF); // set all the other registers to zeros (we skip the last one, output latch)
            mcpDevice.WriteRegister(Registers.InputPolarityRegister, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptOnChangeRegister, 0x00);
            mcpDevice.WriteRegister(Registers.DefaultComparisonValueRegister, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptControlRegister, 0x00);
            mcpDevice.WriteRegister(Registers.IOConfigurationRegister, 0x00);
            mcpDevice.WriteRegister(Registers.PullupResistorConfigurationRegister, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptFlagRegister, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptCaptureRegister, 0x00);
            mcpDevice.WriteRegister(Registers.GPIORegister, 0x00);

            // save our state
            ioDir = 0xFF;
            gpio = 0x00;
            olat = 0x00;
            gppu = 0x00;
            iocon = 0x00;

            ioDir = mcpDevice.ReadRegister(Registers.IODirectionRegister);
            gpio = mcpDevice.ReadRegister(Registers.GPIORegister);
            olat = mcpDevice.ReadRegister(Registers.OutputLatchRegister);

            bool intHigh = true;
            if(interruptPort.Resistor == ResistorMode.InternalPullUp || 
               interruptPort.Resistor == ResistorMode.ExternalPullUp)
            {
                intHigh = false;
            }

            iocon = BitHelpers.SetBit(iocon, 0x01, intHigh); //set interrupt pin to active high (true), low (false)
            iocon = BitHelpers.SetBit(iocon, 0x02, false); //don't set interrupt to open drain (should be the default)

            mcpDevice.WriteRegister(Registers.IOConfigurationRegister, iocon);

            // Clear out I/O Settings
            mcpDevice.WriteRegister(Registers.DefaultComparisonValueRegister, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptOnChangeRegister, 0x00);
            mcpDevice.WriteRegister(Registers.InterruptControlRegister, 0x00);
            mcpDevice.WriteRegister(Registers.InputPolarityRegister, 0x00);
        }

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
        /// Sets the direction of a particular port.
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="direction"></param>
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
                mcpDevice.WriteRegister(Registers.IODirectionRegister, ioDir);
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

                gppu = mcpDevice.ReadRegister(Registers.PullupResistorConfigurationRegister);
                gppu = BitHelpers.SetBit(gppu, (byte)pin.Key, enablePullUp);
                mcpDevice.WriteRegister(Registers.PullupResistorConfigurationRegister, gppu);

                if (interruptMode != InterruptMode.None)
                {
                    // interrupt on change (whether or not we want to raise an interrupt on the interrupt pin on change)
                    byte gpinten = mcpDevice.ReadRegister(Registers.InterruptOnChangeRegister);
                    gpinten = BitHelpers.SetBit(gpinten, (byte)pin.Key, true);
                    mcpDevice.WriteRegister(Registers.InterruptOnChangeRegister, gpinten);

                    // Set the default value for the pin for interrupts.
                    /* Adrian - this isn't needed because we're not comparing the default value
                     * Code here as a reference if we want to enable in the future */
                    
                   /* var interruptValue = interruptMode == InterruptMode.EdgeFalling;
                    byte defVal = mcpDevice.ReadRegister(Registers.DefaultComparisonValueRegister);
                    defVal = BitHelpers.SetBit(defVal, (byte)pin.Key, enablePullUp);
                    mcpDevice.WriteRegister(Registers.DefaultComparisonValueRegister, defVal);
                   */

                    // interrupt control register; whether or not the change is based 
                    // on default comparison value, or if a change from previous. We 
                    // want to raise on change, so we set it to 0, always.
                    //     var intCon = mcpDevice.ReadRegister(Registers.InterruptControlRegister);
                    //     intCon = BitHelpers.SetBit(intCon, (byte)pin.Key, 0);
                    //     mcpDevice.WriteRegister(Registers.InterruptControlRegister, intCon);
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
                mcpDevice.WriteRegister(Registers.OutputLatchRegister, olat);
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
                gpio = mcpDevice.ReadRegister(Registers.GPIORegister);

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
                mcpDevice.WriteRegister(Registers.IODirectionRegister, ioDir);
            }
            // write the output
            olat = mask;
            mcpDevice.WriteRegister(Registers.OutputLatchRegister, olat);
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
                mcpDevice.WriteRegister(Registers.IODirectionRegister, ioDir);
            }
            // read the input
            gpio = mcpDevice.ReadRegister(Registers.GPIORegister);
            return gpio;
        }

        /// <summary>
        /// Checks whether or not the pin passed in exists on the chip.
        /// </summary>
        protected bool IsValidPin(IPin pin) => Pins.AllPins.Contains(pin);

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
        public IPin GetPin(string pinName)
        {
            return Pins.AllPins.FirstOrDefault(p => p.Name == pinName || p.Key.ToString() == p.Name);
        }
    }
}