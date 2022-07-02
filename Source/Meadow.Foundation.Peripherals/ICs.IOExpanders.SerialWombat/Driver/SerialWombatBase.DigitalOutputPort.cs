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
                _controller.ConfigureOutputPin((byte)pin.Key, false, OutputType.PushPull);
            }

            public override bool State
            {
                get => _controller.ReadPublicData((byte)Pin.Key) != 0;
                set => _controller.ConfigureOutputPin((byte)Pin.Key, value, OutputType.PushPull);
            }

            public override void Dispose()
            {
            }
        }
    }
}