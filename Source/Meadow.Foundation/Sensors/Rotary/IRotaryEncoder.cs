using Meadow.Hardware;

namespace Meadow.Foundation.Sensors.Rotary
{
    public interface IRotaryEncoder
    {
        event RotaryTurnedEventHandler Rotated;

        DigitalInputPort APhasePin { get; }
        DigitalInputPort BPhasePin { get; }
    }
}