using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents an Sc16is7x2 SPI/I2C dual UART (with 8 GPIO's)
    /// </summary>
    public partial class Sc16is7x2
    {
        /// <summary>
        /// Represents an SC16IS7x2 digital input port
        /// Mostly copied from the MCP23xxx implementation
        /// </summary>
        public class DigitalInputPort : DigitalInputPortBase
        {
            /// <inheritdoc/>
            public override bool State
            {
                get
                {
                    if (Pin.Controller is Sc16is7x2 controller)
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
                set => throw new NotSupportedException("Cannot change port resistor mode after the port is created");
            }
            private readonly ResistorMode portResistorMode;

            /// <summary>
            /// Create a new DigitalInputPort object
            /// </summary>
            /// <param name="pin">The GPIO pin to configure as an input pin</param>
            /// <param name="initState">The initial state of the pin</param>
            /// <param name="resistorMode">The resistor mode used by the interrupt pin</param>
            public DigitalInputPort(IPin pin, bool initState, ResistorMode resistorMode = ResistorMode.Disabled)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels![0])
            {
                portResistorMode = resistorMode;
                state = initState;
            }

            /// <summary>
            /// Update the port value 
            /// </summary>
            /// <param name="newState">The new port state</param>
            internal void Update(bool newState)
            {
                if (newState == state) return;

                bool oldState = state;
                state = newState;
                if (StateChanged != null)
                    StateChanged(this, new StateChangedEventArgs() { OldState = oldState, NewState = newState });
            }

            /// <summary>
            /// Event to be raised whenever the port state changes.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            public delegate void StateChangedHandler(object sender, StateChangedEventArgs e);
            /// <summary>
            /// Event to be raised whenever the port state changes.
            /// </summary>
            public event StateChangedHandler? StateChanged;

            /// <summary>
            /// Event arguments for the StateChanged event
            /// </summary>
            public class StateChangedEventArgs
            {
                /// <summary>
                /// The new pin state
                /// </summary>
                public bool NewState { get; set; }
                /// <summary>
                /// The old pin state
                /// </summary>
                public bool OldState { get; set; }
            }
        }
    }
}