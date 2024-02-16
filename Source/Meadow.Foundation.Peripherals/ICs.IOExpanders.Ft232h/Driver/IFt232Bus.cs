using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an interface for interacting with an FT232 bus.
/// </summary>
internal interface IFt232Bus
{
    /// <summary>
    /// Gets the handle to the FT232 device.
    /// </summary>
    IntPtr Handle { get; }

    /// <summary>
    /// Gets or sets the GPIO direction mask for the FT232 device.
    /// </summary>
    byte GpioDirectionMask { get; set; }

    /// <summary>
    /// Gets or sets the state of the GPIO pins on the FT232 device.
    /// </summary>
    byte GpioState { get; set; }
}
