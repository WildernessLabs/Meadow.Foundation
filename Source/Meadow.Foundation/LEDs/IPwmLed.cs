using System;
namespace Meadow.Foundation.LEDs
{
    public interface IPwmLed : ILed
    {
        new IPwmPort Port { get; }
    }
}
