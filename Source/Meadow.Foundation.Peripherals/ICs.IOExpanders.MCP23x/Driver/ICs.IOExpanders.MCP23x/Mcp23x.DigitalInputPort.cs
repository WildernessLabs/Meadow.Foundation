using System;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public abstract partial class Mcp23x
    {
        public class DigitalInputPort : DigitalInputPortBase
        {
            private readonly Mcp23x _mcp;
            private readonly int _port;
            private readonly byte _pinKey;
            private DateTime _lastChangeTime;
            private bool _lastState;

            public override bool State => _mcp.ReadPin(_port, _pinKey);

            public override ResistorMode Resistor
            {
                get => ResistorMode.Disabled;
                set
                {
                    if (value != ResistorMode.Disabled)
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            public override double DebounceDuration { get; set; } //Todo not currently used

            public override double GlitchDuration { get; set; } //Todo not currently used

            internal DigitalInputPort(
                Mcp23x mcpController,
                IPin pin,
                int port,
                InterruptMode interruptMode)
                : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], interruptMode)
            {
                _mcp = mcpController;
                _port = port;
                _pinKey = (byte) pin.Key;

                // verify mcp, pin and port are valid
                // allows us to use private methods on _mcp
                if (_port < 0 || _port >= _mcp.Ports.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(port));
                }

                if (!_mcp.Ports[_port].AllPins.Contains(Pin))
                {
                    throw new ArgumentException("Pin does not belong to mcp controller.");
                }

                if (interruptMode != InterruptMode.None)
                {
                    _mcp.Ports[_port].InputChanged += PinChanged;
                }
            }

            private void PinChanged(object sender, IOExpanderPortInputChangedEventArgs e)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var isInterrupt = BitHelpers.GetBitValue(e.InterruptPins, (byte)Pin.Key);
                    if (!isInterrupt)
                    {
                        return;
                    }

                    var currentState = BitHelpers.GetBitValue(e.InterruptValues, (byte)Pin.Key);
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
