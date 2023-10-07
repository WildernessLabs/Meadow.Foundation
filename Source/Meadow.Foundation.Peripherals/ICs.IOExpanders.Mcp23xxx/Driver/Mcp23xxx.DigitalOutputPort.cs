using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23xxx
    {
        /// <summary>
        /// Represents an Mcp23xxx DigitalOutputPort 
        /// </summary>
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            /// <summary>
            /// Enable the caller to receive pin state updates
            /// </summary>
            /// <param name="pin"></param>
            /// <param name="state"></param>
            public delegate void SetPinStateDelegate(IPin pin, bool state);

            /// <summary>
            /// The SetPinState delegate 
            /// </summary>
            public SetPinStateDelegate SetPinState;

            /// <summary>
            /// The port state
            /// True for high, false for low
            /// </summary>
            public override bool State
            {
                get => state;
                set => SetPinState?.Invoke(Pin, state = value);
            }
            bool state;

            /// <summary>
            /// Create a new DigitalOutputPort 
            /// </summary>
            /// <param name="pin">The pin representing the port</param>
            /// <param name="initialState">The initial port state</param>
            /// <param name="outputType">An IPin instance</param>
            public DigitalOutputPort(
                IPin pin,
                bool initialState = false,
                OutputType outputType = OutputType.OpenDrain)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], initialState, outputType)
            {
            }
        }
    }
}