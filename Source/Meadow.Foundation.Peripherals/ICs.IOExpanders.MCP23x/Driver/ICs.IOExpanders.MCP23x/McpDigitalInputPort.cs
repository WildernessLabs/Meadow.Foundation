using System;
using Meadow.Foundation.ICs.IOExpanders.Ports;
using Meadow.Hardware;
using Meadow.Utilities;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class McpDigitalInputPort : DigitalInputPortBase
    {
        private readonly IMcp23x _mcp;
        private readonly IMcpGpioPort _port;
        private DateTime _lastChangeTime;
        private bool _lastState;

        internal McpDigitalInputPort(
            IMcp23x mcpController,
            IPin pin,
            InterruptMode interruptMode)
            : base(pin, (IDigitalChannelInfo)pin.SupportedChannels[0], interruptMode)
        {
            _mcp = mcpController;

            if (!_mcp.Ports.AllPins.Contains(Pin))
            {
                throw new ArgumentException("Pin does not belong to mcp controller.");
            }

            _port = _mcp.Ports.GetPortOfPin(Pin);

            if (interruptMode != InterruptMode.None)
            {
                _port.InputChanged += PinChanged;
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
            if (InterruptMode != InterruptMode.None)
            {
                _port.InputChanged -= PinChanged;
            }
        }

        private void PinChanged(object sender, IOExpanderPortInputChangedEventArgs e)
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
                        if (!currentState)
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _lastState = currentState;
            _lastChangeTime = now;
        }
    }
}
