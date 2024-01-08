using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Digital output port for CP2112 devices.
    /// </summary>
    public sealed class Cp2112DigitalOutputPort : DigitalOutputPortBase
    {
        private readonly Cp2112 _device;
        private readonly bool _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cp2112DigitalOutputPort"/> class.
        /// </summary>
        /// <param name="pin">The pin to use.</param>
        /// <param name="info">The digital channel info.</param>
        /// <param name="initialState">The initial state of the output port.</param>
        /// <param name="initialOutputType">The initial output type.</param>
        /// <param name="device">The CP2112 device.</param>
        internal Cp2112DigitalOutputPort(IPin pin, IDigitalChannelInfo info, bool initialState, OutputType initialOutputType, Cp2112 device)
            : base(pin, info, initialState, initialOutputType)
        {
            _device = device;
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
                if (value)
                {
                    Console.WriteLine("ON");
                    _device.SetState((byte)this.Pin.Key);

                }
                else
                {
                    Console.WriteLine("OFF");
                    _device.ClearState((byte)this.Pin.Key);
                }
            }
        }
    }
}