using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        /// <summary>
        /// Serial Wombat DigitalOutputPort class
        /// </summary>
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            private SerialWombatBase _controller;

            /// <summary>
            /// Create a new DigitalOutputPort object
            /// </summary>
            public DigitalOutputPort(SerialWombatBase controller, IPin pin, bool initialState, OutputType outputType)
                : base(pin, GetChannelInfoForPin(pin), initialState, outputType)
            {
                _controller = controller;
                _controller.ConfigureOutputPin((byte)pin.Key, false, OutputType.PushPull);
            }

            /// <summary>
            /// The port state
            /// </summary>
            public override bool State
            {
                get => _controller.ReadPublicData((byte)Pin.Key) != 0;
                set => _controller.ConfigureOutputPin((byte)Pin.Key, value, OutputType.PushPull);
            }
        }
    }
}