using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class SerialWombatBase
    {
        public class DigitalInputPort : DigitalInputPortBase
        {
            private SerialWombatBase _controller;
            private ResistorMode _resistor;

            public override double DebounceDuration { get; set; }
            public override double GlitchDuration { get; set; }

            public DigitalInputPort(SerialWombatBase controller, IPin pin, InterruptMode interruptMode, ResistorMode resistorMode)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], interruptMode)
            {
                if (interruptMode != InterruptMode.None) throw new NotSupportedException("Interrupts not supported");

                Resistor = resistorMode;
                _controller = controller;
                _controller.ConfigureInputPin((byte)pin.Key, Resistor);
            }

            public override bool State
            {
                get => _controller.ReadPublicData((byte)Pin.Key) != 0;
            }

            public override ResistorMode Resistor
            {
                get => _resistor;
                set
                {
                    if (value == Resistor) return;
                    _resistor = value;
                    _controller.ConfigureInputPin((byte)Pin.Key, Resistor);
                }
            }

            public override void Dispose()
            {
            }
        }
    }
}