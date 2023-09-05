using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Digital output port for FT232 devices.
    /// </summary>
    public sealed class Ft232DigitalOutputPort : DigitalOutputPortBase
    {
        private IFt232Bus _bus;
        private bool _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ft232DigitalOutputPort"/> class.
        /// </summary>
        /// <param name="pin">The pin to use.</param>
        /// <param name="info">The digital channel info.</param>
        /// <param name="initialState">The initial state of the output port.</param>
        /// <param name="initialOutputType">The initial output type.</param>
        /// <param name="bus">The FT232 bus.</param>
        internal Ft232DigitalOutputPort(IPin pin, IDigitalChannelInfo info, bool initialState, OutputType initialOutputType, IFt232Bus bus)
            : base(pin, info, initialState, initialOutputType)
        {
            if (initialOutputType != OutputType.PushPull)
            {
                throw new NotSupportedException("The FT232 only supports push-pull outputs");
            }

            _bus = bus;
            State = initialState;
        }

        /// <summary>
        /// Gets or sets the state of the digital output port.
        /// </summary>
        /// <value>
        /// The state of the digital output port.
        /// </value>
        public override bool State
        {
            get => _state;
            set
            {
                byte s = _bus.GpioState;

                if (value)
                {
                    s |= (byte)Pin.Key;
                }
                else
                {
                    s &= (byte)~((byte)Pin.Key);
                }

                var result = Native.Mpsse.FT_WriteGPIO(_bus.Handle, _bus.GpioDirectionMask, s);
                Native.CheckStatus(result);

                _bus.GpioState = s;
                _state = value;
            }
        }
    }
}