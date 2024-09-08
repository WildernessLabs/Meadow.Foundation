using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Ads1263
    {
        /// <summary>
        /// Represents an Ads1263 digital input port
        /// </summary>
        public class DigitalInputPort : DigitalInputPortBase
        {
            /// <inheritdoc/>
            public override bool State
            {
                get
                {
                    if (Pin.Controller is Ads1263 controller)
                    {
                        Update(controller.ReadPort(Pin));
                    }
                    return state;
                }
            }
            private bool state = false;

            /// <inheritdoc/>
            public override ResistorMode Resistor
            {
                get => portResistorMode;
                set => throw new NotSupportedException("Cannot change port resistor mode");
            }
            private readonly ResistorMode portResistorMode;

            /// <summary>
            /// Create a new DigitalInputPort object
            /// </summary>
            /// <param name="pin">The input pin</param>
            public DigitalInputPort(IPin pin)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels![0])
            {
                portResistorMode = ResistorMode.Disabled;
            }

            /// <summary>
            /// Update the port value 
            /// </summary>
            /// <param name="newState">The new port state</param>
            internal void Update(bool newState)
            {
                state = newState;
            }
        }
    }
}