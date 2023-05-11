using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pca9671
    {
        public class DigitalOutputPort : DigitalOutputPortBase
        {
            public Pca9671 Peripheral { get; }

            public DigitalOutputPort(Pca9671 peripheral, IPin pin, bool initialState, OutputType initialOutputType = OutputType.PushPull)
                : base(pin, (IDigitalChannelInfo)pin!.SupportedChannels![0], initialState, initialOutputType)
            {
                Peripheral = peripheral;

                // TODO: assert initial state
            }

            public override bool State
            {

                get => Peripheral.GetState(Pin);
                set => Peripheral.SetState(Pin, value);
            }
        }
    }
}