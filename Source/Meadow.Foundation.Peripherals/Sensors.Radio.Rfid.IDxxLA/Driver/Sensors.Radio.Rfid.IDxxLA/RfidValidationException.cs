using System;

namespace Meadow.Foundation.Sensors.Radio.Rfid
{
    public class RfidValidationException : Exception
    {
        public RfidValidationException(RfidValidationStatus status) : base(
            $"Failed to read RFID from serial data with error {status}")
        {
            Status = status;
        }

        public RfidValidationStatus Status { get; }
    }
}