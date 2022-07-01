using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            private SerialWombatBase _controller;

            public DigitalOutputPort(SerialWombatBase controller, IPin pin, bool initialState, OutputType outputType)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], initialState, outputType)
            {
                _controller = controller;
                _controller.ConfigurePin((byte)pin.Key, false, ResistorMode.Disabled);
            }

            public override bool State
            {
                get => _controller.ReadPublicData((byte)Pin.Key) != 0;
                set
                {
                    _controller.ConfigurePin((byte)Pin.Key, false, ResistorMode.Disabled);
                }
            }

            public override void Dispose()
            {
            }
        }
    }
}