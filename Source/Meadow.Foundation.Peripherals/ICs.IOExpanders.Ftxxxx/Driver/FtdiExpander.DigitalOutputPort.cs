using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an FTDI expander.
/// </summary>
public abstract partial class FtdiExpander
{
    /// <summary>
    /// Represents a digital output port for the FTDI expander.
    /// </summary>
    public sealed class DigitalOutputPort : DigitalOutputPortBase
    {
        private readonly FtdiExpander _expander;
        private bool _state;
        private readonly FtdiPin _pin;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalOutputPort"/> class.
        /// </summary>
        /// <param name="expander">The FTDI expander instance.</param>
        /// <param name="pin">The pin associated with this digital output port.</param>
        /// <param name="channel">The digital channel information.</param>
        /// <param name="initialState">The initial state of the port.</param>
        /// <param name="initialOutputType">The initial output type of the port.</param>
        /// <exception cref="ArgumentException">Thrown when the pin is invalid.</exception>
        internal DigitalOutputPort(FtdiExpander expander, IPin pin, IDigitalChannelInfo channel, bool initialState, OutputType initialOutputType)
            : base(pin, channel, initialState, initialOutputType)
        {
            _expander = expander;
            _state = initialState;

            if (pin is FtdiPin p)
            {
                _pin = p;
            }
            else
            {
                throw new ArgumentException("Invalid pin");
            }
        }

        /// <summary>
        /// Gets or sets the state of the digital output port.
        /// </summary>
        /// <inheritdoc/>
        public override bool State
        {
            get => _state;
            set
            {
                byte s = _pin.IsLowByte ? _expander.GpioStateLow : _expander.GpioStateHigh;

                if (value)
                {
                    s |= (byte)_pin.Key;
                }
                else
                {
                    s &= (byte)~(byte)_pin.Key;
                }

                if (_pin.IsLowByte)
                {
                    // Dxxx pins
                    _expander.SetGpioDirectionAndState(true, _expander.GpioDirectionLow, s);
                }
                else
                {
                    // Cxxx pins
                    _expander.SetGpioDirectionAndState(false, _expander.GpioDirectionHigh, s);
                }
                _state = value;
            }
        }
    }
}