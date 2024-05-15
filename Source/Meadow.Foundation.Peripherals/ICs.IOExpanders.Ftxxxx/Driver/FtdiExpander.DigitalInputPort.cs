using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    public sealed class DigitalInputPort : DigitalInputPortBase
    {
        private FtdiExpander _expander;
        private FtdiPin _pin;

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