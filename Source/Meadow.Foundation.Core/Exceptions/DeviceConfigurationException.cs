using System;

namespace Meadow.Foundation
{
    /// <summary>
    /// Exception thrown when a device or peripheral is misconfigured for a requested action, activity or behavior.
    /// </summary>
    public class DeviceConfigurationException : Exception
    {
        public DeviceConfigurationException(string message)
            : base(message)
        {
        }
    }
}