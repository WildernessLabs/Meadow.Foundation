using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        public class BiDirectionalPort : BiDirectionalPortBase
        {
            private SerialWombatBase _controller;

            public override PortDirectionType Direction { get; set; }
            public override TimeSpan DebounceDuration { get; set; }
            public override TimeSpan GlitchDuration { get; set; }

            protected BiDirectionalPort(
                IPin pin,
                IDigitalChannelInfo channel,
                SerialWombatBase controller,
                bool initialState,
                InterruptMode interruptMode = InterruptMode.None,
                ResistorMode resistorMode = ResistorMode.Disabled,
                PortDirectionType initialDirection = PortDirectionType.Input)
                : this(pin, controller, channel, initialState, interruptMode, resistorMode, initialDirection, debounceDuration: TimeSpan.Zero, glitchDuration: TimeSpan.Zero, outputType: OutputType.PushPull)
            {
            }

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

            public override bool State
            {
                get => _controller.ReadPublicData((byte)Pin.Key) != 0;
                set => _controller.ConfigureOutputPin((byte)Pin.Key, value, OutputType.PushPull);
            }
        }
    }
}