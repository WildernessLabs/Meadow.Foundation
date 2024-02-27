using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        /// <summary>
        /// Represents an Mcp23xxx digital interrupt port
        /// </summary>
        public class DigitalInterruptPort : DigitalInterruptPortBase
        {
            /// <inheritdoc/>
            public override bool State => state;
            private bool state = false;

            private int lastUpdate;

            /// <inheritdoc/>
            public override ResistorMode Resistor
            {
                get => portResistorMode;
                set => throw new NotSupportedException("Cannot change port resistor mode after the port is created");
            }
            private readonly ResistorMode portResistorMode;

            /// <inheritdoc/>
            public override TimeSpan DebounceDuration { get; set; } = TimeSpan.Zero;

            /// <inheritdoc/>
            public override TimeSpan GlitchDuration
            {
                get => TimeSpan.FromMilliseconds(0.00015);
                set => _ = value; //fail silently
            }

            /// <summary>
            /// Create a new DigitalInterruptPort object
            /// </summary>
            /// <param name="pin">The interrupt pin</param>
            /// <param name="interruptMode">The interrupt mode used for the interrupt pin</param>
            /// <param name="resistorMode">The resistor mode used by the interrupt pin</param>
            public DigitalInterruptPort(IPin pin, InterruptMode interruptMode = InterruptMode.None, ResistorMode resistorMode = ResistorMode.Disabled)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels![0], interruptMode)
            {
                portResistorMode = resistorMode;
            }

            /// <summary>
            /// Update the port value 
            /// </summary>
            /// <param name="newState">The new port state</param>
            internal void Update(bool newState)
            {
                var now = Environment.TickCount;

                if (now - lastUpdate < DebounceDuration.TotalMilliseconds)
                {
                    return;
                }

                if (newState != state)
                {
                    switch (InterruptMode)
                    {
                        case InterruptMode.EdgeFalling:
                            if (newState)
                            {
                                RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(false, now), new DigitalState(true, lastUpdate)));
                            }
                            break;
                        case InterruptMode.EdgeRising:
                            if (newState)
                            {
                                RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(true, now), new DigitalState(false, lastUpdate)));
                            }
                            break;
                        case InterruptMode.EdgeBoth:
                            RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(newState, now), new DigitalState(!newState, lastUpdate)));
                            break;
                        case InterruptMode.None:
                        default:
                            break;
                    }
                }

                state = newState;
                lastUpdate = now;
            }
        }
    }
}