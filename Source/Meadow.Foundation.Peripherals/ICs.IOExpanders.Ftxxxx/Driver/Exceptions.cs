using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an exception thrown when no FT232 device is found during a connection check.
/// </summary>
public class DeviceNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceNotFoundException"/> class
    /// with a default error message instructing to check the connection.
    /// </summary>
    internal DeviceNotFoundException()
        : base("No FT232 device found.  Check your connection")
    {
    }
}

/// <summary>
/// Represents an exception thrown when the Ftd2xx driver is not installed for device operation in Ftd2xx mode.
/// </summary>
public class DriverNotInstalledException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DriverNotInstalledException"/> class
    /// with a default error message indicating that the Ftd2xx driver must be installed.
    /// </summary>
    internal DriverNotInstalledException()
        : base("The Ftd2xx driver must be installed to use the device in Ftd2xx mode.")
    {
    }
}
