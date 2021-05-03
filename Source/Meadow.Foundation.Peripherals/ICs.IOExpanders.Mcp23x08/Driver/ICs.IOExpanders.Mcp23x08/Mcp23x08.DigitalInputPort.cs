using System;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Mcp23x08
    {
        public class DigitalInputPort : DigitalInputPortBase
        {
            Mcp23x08 _mcp;

            private readonly IPin _pin;
            private DateTime _lastChangeTime;
            private bool _lastState;

            public override bool State
            {
                get
                {
                    return _mcp.ReadPort(this.Pin);
                }
            }

            public override ResistorMode Resistor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override double DebounceDuration { get; set; } //Todo not currently used

            public override double GlitchDuration { get; set; } //Todo not currently used

            public DigitalInputPort(
                Mcp23x08 mcpController,
                IPin pin,
                InterruptMode interruptMode = InterruptMode.None)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], interruptMode)
            {
                _mcp = mcpController;
                _pin = pin;
                if (interruptMode != InterruptMode.None)
                {
                    _mcp.InputChanged += PinChanged;
                }
            }

            internal void PinChanged(object sender, IOExpanderInputChangedEventArgs e)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var isInterrupt = BitHelpers.GetBitValue(e.InterruptPins, (byte)_pin.Key);
                    if (!isInterrupt)
                    {
                        return;
                    }

                    var currentState = BitHelpers.GetBitValue(e.InputState, (byte)_pin.Key);
                    if (currentState != _lastState)
                    {
                        switch (InterruptMode)
                        {
                            case InterruptMode.EdgeFalling:
                                if (currentState)
                                    RaiseChangedAndNotify(
                                        new DigitalInputPortEventArgs(false, now, _lastChangeTime));
                                break;
                            case InterruptMode.EdgeRising:
                                if (currentState)
                                    RaiseChangedAndNotify(
                                        new DigitalInputPortEventArgs(true, now, _lastChangeTime));
                                break;
                            case InterruptMode.EdgeBoth:
                                RaiseChangedAndNotify(
                                    new DigitalInputPortEventArgs(currentState, now,
                                        _lastChangeTime));
                                break;
                            case InterruptMode.None:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    _lastState = currentState;
                    _lastChangeTime = now;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            public override void Dispose()
            {

            }
        }
    }
}
