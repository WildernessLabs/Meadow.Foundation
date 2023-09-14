using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public class DeviceNotFoundException : Exception
{
    internal DeviceNotFoundException()
        : base("No FT232 device found.  Check your connection")
    {
    }
}

public class DriverNotInstalledException : Exception
{
    internal DriverNotInstalledException()
        : base("The Ftd2xx driver must be installed to use the device in Ftd2xx mode.")
    {
    }
}
