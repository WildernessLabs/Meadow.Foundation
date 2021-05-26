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
                                if (currentState) {
                                    RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(false, now), new DigitalState(true, _lastChangeTime)));
                                    // BC: 2021.05.21 updating to the new b5.0 result type.
                                    // old code below. TODO: passing an assumption for the old result, but
                                    // if it's the first time through, the old result should be `null`
                                    /*new DigitalPortResult(false, now, _lastChangeTime));*/
                                }
                                break;
                            case InterruptMode.EdgeRising:
                                if (currentState) {
                                    RaiseChangedAndNotify(new DigitalPortResult(new DigitalState(true, now), new DigitalState(false, _lastChangeTime)));
                                    /*new DigitalPortResult(true, now, _lastChangeTime));*/
                                }
                                break;
                            case InterruptMode.EdgeBoth:
                                RaiseChangedAndNotify(
                                    new DigitalPortResult(new DigitalState(currentState, now), new DigitalState(!currentState, _lastChangeTime)));
                                    /*new DigitalPortResult(currentState, now, _lastChangeTime));*/
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
