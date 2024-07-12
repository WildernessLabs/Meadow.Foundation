using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an FTDI expander.
/// </summary>
public abstract partial class FtdiExpander
{
    /// <summary>
    /// Represents a digital input port for the FTDI expander.
    /// </summary>
    public sealed class DigitalInputPort : DigitalInputPortBase
    {
        private readonly FtdiExpander _expander;
        private readonly FtdiPin _pin;

        /// <inheritdoc/>
        public override ResistorMode Resistor { get; set; }

        internal DigitalInputPort(FtdiExpander expander, IPin pin, IDigitalChannelInfo channel, ResistorMode resistorMode)
            : base(pin, channel)
        {
            Resistor = resistorMode;

            _expander = expander;
            _pin = pin as FtdiPin ?? throw new ArgumentException("Invalid pin");
        }

        /// <inheritdoc/>
        public override bool State
        {
            get
            {
                var states = _expander.GetGpioStates(_pin.IsLowByte);
                return (states & (byte)_pin.Key) != 0;
            }
        }
    }
}