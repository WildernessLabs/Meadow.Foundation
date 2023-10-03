using System;

namespace Meadow.Foundation.Sensors.Radio.Rfid
{
    /// <summary>
    /// RfidValidationException class
    /// </summary>
    public class RfidValidationException : Exception
    {
        /// <summary>
        /// Create a new RfidValidationException objec
        /// </summary>
        /// <param name="status">The exception status message</param>
        public RfidValidationException(RfidValidationStatus status)
            : base($"Failed to read RFID from serial data with error {status}")
        {
            Status = status;
        }

        /// <summary>
        /// The RFID validation status
        /// </summary>
        public RfidValidationStatus Status { get; }
    }
}