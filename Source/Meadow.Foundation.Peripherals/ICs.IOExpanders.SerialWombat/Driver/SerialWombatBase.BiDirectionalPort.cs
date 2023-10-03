using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        /// <summary>
        /// Serial Wombat bi-directional port class
        /// </summary>
        public class BiDirectionalPort : BiDirectionalPortBase
        {
            private readonly SerialWombatBase _controller;

            /// <summary>
            /// The port direction
            /// </summary>
            public override PortDirectionType Direction { get; set; }

            /// <summary>
            /// Create a new BiDirectionalPort object
            /// </summary>
            protected BiDirectionalPort(
                IPin pin,
                IDigitalChannelInfo channel,
                bool initialState,
                ResistorMode resistorMode = ResistorMode.Disabled,
                PortDirectionType initialDirection = PortDirectionType.Input)
                : base(pin, channel, initialState, resistorMode, initialDirection, OutputType.PushPull)
            { }

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