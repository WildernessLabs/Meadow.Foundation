using System;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        public class DigitalInputPort : DigitalInputPortBase
        {
            Mcp23x08 _mcp;

            public override bool State {
                get {
                    return _mcp.ReadPort(this.Pin);
                }
            }

            public override ResistorMode Resistor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override int DebounceDuration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override int GlitchFilterCycleCount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public DigitalInputPort(
                Mcp23x08 mcpController,
                IPin pin,
                InterruptMode interruptMode = InterruptMode.None)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], interruptMode)
            {
                _mcp = mcpController;

                // TODO:
                if (interruptMode != InterruptMode.None) {
                    //_mcp.InputChanged += (s, e) => { };
                }
            }



            public override void Dispose()
            {

            }
        }
    }
}
