using System;
using System.Linq;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a 74595 shift register.
    /// </summary>
    /// <remarks>
    /// Control the outputs from a 74595 shift register (or a chain of shift registers)
    /// using a SPI interface.
    /// </remarks>
    public partial class x74595 : IDigitalOutputController
    {
        /// <summary>
        /// The pin definitions
        /// </summary>
        public PinDefinitions Pins { get; } = new PinDefinitions();

        /// <summary>
        /// Number of chips required to implement this ShiftRegister.
        /// </summary>
        private readonly int numberOfChips;

        private byte[] latchData;

        /// <summary>
        /// SPI interface used to communicate with the shift registers.
        /// </summary>
        private readonly ISpiPeripheral spiPeripheral;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// This is private to prevent the programmer from calling it explicitly.
        /// </remarks>
        private x74595()
        {
        }

        /// <summary>
        /// Creates a new ShiftRegister 74595 object
        /// </summary>
        /// <param name="device">The device conneced to the shift register</param>
        /// <param name="pins">Number of pins in the shift register (should be a multiple of 8 pins).</param>
        /// <param name="spiBus">SpiBus object</param>
        /// <param name="pinChipSelect">The chip select pin</param>
        public x74595(IMeadowDevice device, ISpiBus spiBus, IPin pinChipSelect, int pins = 8)
        {
           // if ((pins > 0) && ((pins % 8) == 0))
            if(pins == 8)
            {
                numberOfChips = pins / 8;

                latchData = new byte[numberOfChips];

                spiPeripheral = new SpiPeripheral(spiBus, device.CreateDigitalOutputPort(pinChipSelect));
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

        /// <summary>
        /// Clear the shift register buffer
        /// </summary>
        /// <param name="update">If true, send changes to the shift register</param>
        public void Clear(bool update = true)
        {
            latchData = new byte[numberOfChips];

            if(update)
            {
                spiPeripheral.Write(latchData);
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
                latchData[0] =  BitHelpers.SetBit(latchData[0], (byte)pin.Key, value);
            }
            else
            {
                throw new Exception("Pin is out of range");
            }
            spiPeripheral.Write(latchData);
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