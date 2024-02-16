using Meadow.Hardware;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represent an Pcx8575 8 bit I2C port expander
    /// </summary>
    public abstract partial class Pcx8575 : Pcx857x
    {
        /// <summary>
        /// MCP23x17 pin definitions
        /// </summary>
        public PinDefinitions Pins { get; } = default!;

        /// <summary>
        /// The number of IO pins available on the device
        /// </summary>
        public override int NumberOfPins => 16;

        /// <summary>
        /// Is the pin valid for this device instance
        /// </summary>
        /// <param name="pin">The IPin to validate</param>
        /// <returns>True if pin is valid</returns>
        protected override bool IsValidPin(IPin pin) => Pins.AllPins.Contains(pin);

        private ushort outputs;
        private ushort directionMask;

        /// <summary>
        /// Creates an Pcx8575 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="interruptPin">The interrupt pin</param>
        protected Pcx8575(II2cBus i2cBus, byte address, IPin? interruptPin) :
            base(i2cBus, address, interruptPin)
        {
            Pins = new PinDefinitions(this)
            {
                Controller = this
            };
        }

        /// <summary>
        /// Creates an Pcx8575 object
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">The I2C address</param>
        /// <param name="interruptPort">The interrupt port</param>
        protected Pcx8575(II2cBus i2cBus, byte address, IDigitalInterruptPort? interruptPort = null)
            : base(i2cBus, address, interruptPort)
        {
        }

        /// <summary>
        /// Get pin reference by name
        /// </summary>
        /// <param name="pinName">The pin name as a string</param>
        /// <returns>IPin reference if found</returns>
        public override IPin GetPin(string pinName)
        {
            return Pins.AllPins.FirstOrDefault(p => p.Name == pinName || p.Key.ToString() == p.Name);
        }

        /// <summary>
        /// Set the pin direction
        /// </summary>
        /// <param name="input">true for input, false for output</param>
        /// <param name="pinKey">The pin key value</param>
        protected override void SetPinDirection(bool input, byte pinKey)
        {
            if (input)
            {
                directionMask |= (ushort)(1 << pinKey);
            }
            else
            {
                directionMask &= (ushort)(1 << pinKey);
            }
        }

        /// <summary>
        /// Retrieves the state of a pin
        /// </summary>
        /// <param name="pin">The pin to query</param>
        protected override bool GetPinState(IPin pin)
        {
            // if it's an input, read it, otherwise reflect what we wrote

            var pinMask = 1 << ((byte)pin.Key);
            if ((pinMask & directionMask) != 0)
            {
                // this is an actual input, so read
                var state = ReadState();
                return (state & pinMask) != 0;
            }

            // this is an output, just reflect what we've been told to write
            return (outputs & pinMask) != 0;
        }

        /// <summary>
        /// Sets the state of a pin
        /// </summary>
        /// <param name="pin">The pin to affect</param>
        /// <param name="state"><b>True</b> to set the pin state high, <b>False</b> to set it low</param>
        protected override void SetPinState(IPin pin, bool state)
        {
            var offset = (byte)pin.Key;
            if (state)
            {
                outputs |= (ushort)(1 << offset);
            }
            else
            {
                outputs &= (ushort)~(1 << offset);
            }

            WriteState(outputs);
        }

        /// <summary>
        /// Reads the peripheral state register for 16 pin devices
        /// </summary>
        protected override ushort ReadState()
        {
            Span<byte> buffer = stackalloc byte[2];
            i2cComms.Read(buffer);
            return (ushort)((buffer[0] << 8) | buffer[1]);
        }

        /// <summary>
        /// Writes the peripheral state register for 16 pin devices
        /// </summary>
        protected override void WriteState(ushort state)
        {
            state |= directionMask;
            Span<byte> buffer = stackalloc byte[] { (byte)(state & 0xff), (byte)(state >> 8) };
            i2cComms.Write(buffer);
        }

        /// <summary>
        /// Writes the peripheral state register and updates driver internal state
        /// </summary>
        protected override void SetState(ushort state)
        {
            outputs = state;
            WriteState(outputs);
        }
    }
}