using System;

namespace Meadow.Foundation.Sensors.Radio.Rfid
{
    /// <summary>
    /// RFID Read Result event args class
    /// </summary>
    public class RfidReadResult : EventArgs
    {
        /// <summary>
        /// RFID validation status
        /// </summary>
        public RfidValidationStatus Status { get; set; }

        /// <summary>
        /// RFIS tag
        /// </summary>
        public byte[]? RfidTag { get; set; }
    }
}