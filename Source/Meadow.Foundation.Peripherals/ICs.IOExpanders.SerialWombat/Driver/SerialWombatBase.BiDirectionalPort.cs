using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        /// <summary>
        /// Serial Wombat bi-directional port class
        /// </summary>
        public class BiDirectionalPort : BiDirectionalPortBase
        {
            private SerialWombatBase _controller;

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
                SerialWombatBase controller,
                bool initialState,
                InterruptMode interruptMode = InterruptMode.None,
                ResistorMode resistorMode = ResistorMode.Disabled,
                PortDirectionType initialDirection = PortDirectionType.Input)
                : this(pin, controller, channel, initialState, interruptMode, resistorMode, initialDirection, debounceDuration: TimeSpan.Zero, glitchDuration: TimeSpan.Zero, outputType: OutputType.PushPull)
            { }

            /// <summary>
            /// Create a new BiDirectionalPort object
            /// </summary>
            protected BiDirectionalPort(
                IPin pin,
                SerialWombatBase controller,
                IDigitalChannelInfo channel,
                bool initialState,
                InterruptMode interruptMode,
                ResistorMode resistorMode,
                PortDirectionType initialDirection,
                TimeSpan debounceDuration,
                TimeSpan glitchDuration,
                OutputType outputType
                )
                : base(pin, channel, initialState, interruptMode, resistorMode, initialDirection, debounceDuration, glitchDuration, outputType)
            {
                _controller = controller;
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