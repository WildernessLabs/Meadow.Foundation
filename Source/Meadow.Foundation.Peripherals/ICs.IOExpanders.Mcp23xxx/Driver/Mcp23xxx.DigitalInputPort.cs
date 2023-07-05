using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        /// <summary>
        /// Represents an Mcp23xxx digital input port
        /// </summary>
        public class DigitalInputPort : DigitalInputPortBase
        {
            /// <summary>
            /// Update state function 
            /// Assign this when the Update method isn't reliable 
            /// e.g. when not using interrupts/events
            /// </summary>
            public Func<IPin, bool> UpdateState;

            /// <summary>
            /// The port state
            /// True is high, false is low
            /// </summary>
            public override bool State
            {
                get
                {
                    if (UpdateState != null) { Update(UpdateState.Invoke(Pin)); }
                    return state;
                }
            }
            private bool state = false;

            /// <summary>
            /// The resistor mode of the port
            /// </summary>
            public override ResistorMode Resistor
            {
                get => portResistorMode;
                set => throw new NotSupportedException("Cannot change port resistor mode after the port is created");
            }
            private readonly ResistorMode portResistorMode;

            /// <summary>
            /// Create a new DigitalInputPort object
            /// </summary>
            /// <param name="pin">The interrupt pin</param>
            /// <param name="interruptMode">The interrupt mode used for the interrupt pin</param>
            /// <param name="resistorMode">The resistor mode used by the interrupt pin</param>
            public DigitalInputPort(IPin pin, ResistorMode resistorMode = ResistorMode.Disabled)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0])
            {
                portResistorMode = resistorMode;
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