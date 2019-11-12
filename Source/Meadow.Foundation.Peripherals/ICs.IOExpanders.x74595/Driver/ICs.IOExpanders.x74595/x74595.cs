using System;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Provide an interface to connect to a 74595 shift register.
    /// 
    /// Note: this class is not yet implemented.
    /// </summary>
    /// <remarks>
    /// Control the outputs from a 74595 shift register (or a chain of shift registers)
    /// using a SPI interface.
    /// </remarks>
    public class x74595
    {
        #region Constants

        /// <summary>
        ///     Error message for a pin number out of range exception.
        /// </summary>
        private const string EM_PIN_RANGE_MESSAGE = "x74595: Pin number is out of range.";

        #endregion

        #region Member variables / fields

        /// <summary>
        ///     Array containing the pins to be output to the shift register.
        /// </summary>
        private readonly bool[] _pins;

        /// <summary>
        ///     Number of chips required to implement this ShiftRegister.
        /// </summary>
        private readonly int _numberOfChips;

        /// <summary>
        ///     SPI interface used to communicate with the shift registers.
        /// </summary>
        private readonly ISpiPeripheral _spi;

        #endregion Member variables / fields

        #region Constructor(s)

        /// <summary>
        ///     Default constructor.
        /// </summary>
        /// <remarks>
        ///     This is private to prevent the programmer from calling it explicitly.
        /// </remarks>
        private x74595()
        {
        }

        /// <summary>
        ///     Constructor a ShiftRegister74595 object.
        /// </summary>
        /// <param name="pins">Number of pins in the shift register (should be a multiple of 8 pins).</param>
        /// <param name="spiBus">SpiBus object</param>
        public x74595(ISpiBus spiBus, int pins)
        {
            if ((pins > 0) && ((pins % 8) == 0))
            {
                _pins = new bool[pins];
                _numberOfChips = pins / 8;
                Clear();

                _spi = new SpiPeripheral(spiBus, null);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    "x74595: Size must be greater than zero and a multiple of 8 pins");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Overload the index operator to allow the user to get/set a particular
        ///     pin in the shift register.
        /// </summary>
        /// <param name="pin">Bit number to get/set.</param>
        /// <returns>Value in the specified pin.</returns>
        public bool this[int pin]
        {
            get
            {
                if ((pin >= 0) && (pin < _pins.Length))
                {
                    return _pins[pin];
                }
                throw new IndexOutOfRangeException(EM_PIN_RANGE_MESSAGE);
            }
            set
            {
                if ((pin >= 0) && (pin < _pins.Length))
                {
                    _pins[pin] = value;
                    LatchData();
                }
                else
                {
                    throw new IndexOutOfRangeException(EM_PIN_RANGE_MESSAGE);
                }
            }
        }

        /// <summary>
        /// Creates a new DigitalOutputPort using the specified pin and initial state.
        /// </summary>
        /// <param name="pin">The pin number to create the port on.</param>
        /// <param name="initialState">Whether the pin is initially high or low.</param>
        /// <returns></returns>
        public IDigitalOutputPort CreateOutputPort(IIODevice device, IPin pin, bool initialState)
        {
            return device.CreateDigitalOutputPort(pin, initialState);

           // throw new IndexOutOfRangeException(EM_PIN_RANGE_MESSAGE);
        }

        /// <summary>
        /// Sets a particular pin's value. 
        /// </summary>
        /// <param name="pin">The pin to write to.</param>
        /// <param name="value">The value to write. True for high, false for low.</param>
        public void WriteToPort(byte pin, bool value)
        {
            if (IsValidPin(pin))
            {
                // write new value on the specific pin
                _pins[pin] = value;

                // send the data to the SPI interface.
                LatchData();
            }
            else
            {
                throw new IndexOutOfRangeException(EM_PIN_RANGE_MESSAGE);
            }
        }

        /// <summary>
        /// Outputs a byte value across all of the pins by writing directly 
        /// to the shift register.
        /// </summary>
        /// <param name="mask"></param>
        public void WriteToPorts(byte mask)
        {
            for (byte i = 0; i < 8; i++)
            {
                _pins[i] = BitHelpers.GetBitValue(mask, i);
            }

            // send the data to the SPI interface.
            LatchData();
        }

        /// <summary>
        ///     Clear all of the pins in the shift register.
        /// </summary>
        /// <param name="latch">If true, latch the data after the shift register is cleared (default is false)?</param>
        public void Clear(bool latch = false)
        {
            for (var index = 0; index < _pins.Length; index++)
            {
                _pins[index] = false;
            }

            if (latch)
            {
                LatchData();
            }
        }

        /// <summary>
        ///     Send the data to the SPI interface.
        /// </summary>
        protected void LatchData()
        {
            var data = new byte[_numberOfChips];

            for (var chip = 0; chip < _numberOfChips; chip++)
            {
                var dataByte = _numberOfChips - chip - 1;
                data[dataByte] = 0;
                byte bitValue = 1;
                var offset = chip * 8;
                for (var bit = 0; bit < 8; bit++)
                {
                    if (_pins[offset + bit])
                    {
                        data[dataByte] |= bitValue;
                    }
                    bitValue <<= 1;
                }
            }
            _spi.WriteBytes(data);
        }

        /// <summary>
        ///     Check if the specified pin is valid.
        /// </summary>
        /// <param name="pin">Pin number</param>
        /// <returns>True if the pin number is valid, false if it not.</returns>
        protected bool IsValidPin(byte pin)
        {
            return (pin <= _pins.Length);
        }

        #endregion
    }
}