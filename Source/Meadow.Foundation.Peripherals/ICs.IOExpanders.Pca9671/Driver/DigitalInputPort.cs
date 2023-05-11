using Meadow.Hardware;
using System;
using System.Linq;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Pca9671
    {
        public class DigitalInputPort : DigitalInputPortBase
        {
            public Pca9671 Peripheral { get; }

            public DigitalInputPort(Pca9671 peripheral, IPin pin)
                : base(pin, pin.SupportedChannels.OfType<IDigitalChannelInfo>().First())

            {
                Peripheral = peripheral;
            }

            // TODO: these need to go away.  Gotta fix the base class
            public override ResistorMode Resistor { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override TimeSpan DebounceDuration { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override TimeSpan GlitchDuration { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public override bool State => Peripheral.GetState(Pin);
        }
    }
}