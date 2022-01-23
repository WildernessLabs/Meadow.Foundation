using System;

namespace Meadow.Foundation
{
    /// <summary>
    /// Exception thrown when a device or peripheral is misconfigured for a requested action, activity or behavior.
    /// </summary>
    public class DeviceConfigurationException : Exception
    {
        /// <summary>
        /// Create a new DeviceConfigurationException object
        /// </summary>
        /// <param name="message"></param>
        public DeviceConfigurationException(string message)
            : base(message)
        {
        }
    }
}