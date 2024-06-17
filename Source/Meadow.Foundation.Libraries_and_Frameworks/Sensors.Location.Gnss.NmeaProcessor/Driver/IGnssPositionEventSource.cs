using Meadow.Peripherals.Sensors.Location.Gnss;
using System;

namespace Meadow.Foundation.Sensors.Location.Gnss;

/// <summary>
/// An interface for classes that generate GNSS positional data events
/// </summary>
public interface IGnssPositionEventSource
{
    /// <summary>
    /// Raised when position data is received
    /// </summary>
    event EventHandler<GnssPositionInfo>? PositionReceived;
}
