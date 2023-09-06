using Meadow.Hardware;
using Meadow.Units;
using Meadow.Utilities;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a 74595 shift register
    /// </summary>
    /// <remarks>
    /// Control the outputs from a 74595 shift register (or a chain of shift registers)
    /// using a SPI interface
    /// </remarks>
    public partial class x74595 : IDigitalOutputController, ISpiPeripheral
    {
        /// <summary>
        /// The pin definitions
        /// </summary>
        public PinDefinitions Pins { get; } = default!;

        /// <summary>
        /// The default SPI bus speed for the device
        /// </summary>
        public Frequency DefaultSpiBusSpeed => new Frequency(10000, Frequency.UnitType.Kilohertz);

        /// <summary>
        /// The SPI bus speed for the device
        /// </summary>
        public Frequency SpiBusSpeed
        {
            get => spiComms.BusSpeed;
            set => spiComms.BusSpeed = value;
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
            get => spiComms.BusMode;
            set => spiComms.BusMode = value;
        }

        /// <summary>
        /// Number of chips required to implement this ShiftRegister
        /// </summary>
        private readonly int numberOfChips;

        private byte[] latchData = default!;

        /// <summary>
        /// SPI Communication bus used to communicate with the peripheral
        /// </summary>
        protected ISpiCommunications spiComms = default!;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// This is private to prevent the programmer from calling it explicitly
        /// </remarks>
        private x74595()
        {
        }

        /// <summary>
        /// Creates a new ShiftRegister 74595 object
        /// </summary>
        /// <param name="pins">Number of pins in the shift register (should be a multiple of 8 pins).</param>
        /// <param name="spiBus">SpiBus object</param>
        /// <param name="pinChipSelect">The chip select pin</param>
        /// <param name="initialStates">An optional list of initial states for the pins.  Omitting this will initialize all pins to low.</param>
        public x74595(ISpiBus spiBus, IPin pinChipSelect, int pins = 8, bool[]? initialStates = null)
        {
            if (initialStates != null && initialStates.Length != pins)
            {
                throw new ArgumentException("If not null, initialStates length must match the pin count");
            }

            Pins = new PinDefinitions(this);

            if (pins == 8)
            {
                numberOfChips = pins / 8;

                latchData = new byte[numberOfChips];

                // start with the CS/OE/lath *high* which put the outputs in high-Z while we clear things to a known state
                spiComms = new SpiCommunications(spiBus, pinChipSelect?.CreateDigitalOutputPort(true), DefaultSpiBusSpeed, DefaultSpiBusMode);

                // the chip register is in an unknown state right now - we need to set the data before the initial latch
                if (initialStates != null)
                {
                    var pin = 0;

                    for (var chip = 0; chip < numberOfChips; chip++)
                    {
                        byte b = 0;

                        for (var bit = 0; bit < 8; bit++)
                        {
                            if (initialStates[pin])
                            {
                                b |= (byte)(1 << pin);
                                if (++pin >= pins) break;
                            }
                        }
                        latchData[chip] = b;
                    }

                    ParalellWrite(latchData);
                }
                else
                {
                    Clear();
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    "x74595: Size must be greater than zero and a multiple of 8 pins, driver is currently limited to one chip (8 pins)");
            }
        }

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state
        /// </summary>
        /// <param name="pin">The pin number to create the port on</param>
        /// <param name="initialState">Whether the pin is initially high or low</param>
        /// <param name="outputType">The port output type</param>
        /// <returns></returns>
        public IDigitalOutputPort CreateDigitalOutputPort(IPin pin, bool initialState, OutputType outputType)
        {
            if (IsValidPin(pin))
            {
                // create the convenience class
                return new DigitalOutputPort(this, pin, initialState, outputType);
            }

            throw new Exception("Pin is out of range");
        }

        private void ParalellWrite(byte[] data)
        {
            spiComms.Write(data);
        }

        /// <summary>
        /// Clear the shift register buffer
        /// </summary>
        /// <param name="update">If true, send changes to the shift register</param>
        public void Clear(bool update = true)
        {
            latchData = new byte[numberOfChips];

            if (update)
            {
                ParalellWrite(latchData);
            }
        }

        /// <summary>
        /// Sets a particular pin's value. 
        /// </summary>
        /// <param name="pin">The pin to write to.</param>
        /// <param name="value">The value to write. True for high, false for low.</param>
        public void WriteToPin(IPin pin, bool value)
        {
            if (IsValidPin(pin))
            {
                //ToDo multichip logic
                latchData[0] = BitHelpers.SetBit(latchData[0], (byte)pin.Key, value);
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
            spiComms.Write(latchData);
        }

        /// <summary>
        /// Checks whether or not the pin passed in exists on the chip.
        /// </summary>
        protected bool IsValidPin(IPin pin)
        {
            return Pins.AllPins.Contains(pin);
        }

        /// <summary>
        /// Get pin from name
        /// </summary>
        /// <param name="pinName">The pin name</param>
        /// <returns>An IPin object</returns>
        public IPin GetPin(string pinName)
        {
            return Pins.AllPins.FirstOrDefault(p => p.Name == pinName || p.Key.ToString() == p.Name);
        }
    }
}