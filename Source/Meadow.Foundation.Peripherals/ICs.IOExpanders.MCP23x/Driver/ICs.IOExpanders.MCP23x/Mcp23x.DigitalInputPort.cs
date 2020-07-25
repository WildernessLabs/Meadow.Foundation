using System;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class McpDigitalInputPort : DigitalInputPortBase
    {
        private readonly IMcp23x _mcp;
        private DateTime _lastChangeTime;
        private bool _lastState;

        internal McpDigitalInputPort(
            IMcp23x mcpController,
            IPin pin,
            int port,
            InterruptMode interruptMode)
            : base(pin, (IDigitalChannelInfo) pin.SupportedChannels[0], interruptMode)
        {
            _mcp = mcpController;

            // verify mcp, pin and port are valid
            // allows us to use private methods on _mcp
            if (port < 0 || port >= _mcp.Ports.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            if (!_mcp.Ports[port].AllPins.Contains(Pin))
            {
                throw new ArgumentException("Pin does not belong to mcp controller.");
            }

            if (interruptMode != InterruptMode.None)
            {
                _mcp.Ports[port].InputChanged += PinChanged;
            }
        }

        public override bool State => _mcp.ReadPin(Pin);

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


        public override void Dispose()
        {
        }

        private void PinChanged(object sender, IOExpanderPortInputChangedEventArgs e)
        {
            try
            {
                var now = DateTime.UtcNow;
                var isInterrupt = BitHelpers.GetBitValue(e.InterruptPins, (byte) Pin.Key);
                if (!isInterrupt)
                {
                    return;
                }

                var currentState = BitHelpers.GetBitValue(e.InterruptValues, (byte) Pin.Key);
                if (currentState != _lastState)
                {
                    switch (InterruptMode)
                    {
                        case InterruptMode.EdgeFalling:
                            if (currentState)
                            {
                                RaiseChangedAndNotify(new DigitalInputPortEventArgs(false, now, _lastChangeTime));
                            }

                            break;
                        case InterruptMode.EdgeRising:
                            if (currentState)
                            {
                                RaiseChangedAndNotify(new DigitalInputPortEventArgs(true, now, _lastChangeTime));
                            }

                            break;
                        case InterruptMode.EdgeBoth:
                            RaiseChangedAndNotify(
                                new DigitalInputPortEventArgs(
                                    currentState,
                                    now,
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
    }
}
