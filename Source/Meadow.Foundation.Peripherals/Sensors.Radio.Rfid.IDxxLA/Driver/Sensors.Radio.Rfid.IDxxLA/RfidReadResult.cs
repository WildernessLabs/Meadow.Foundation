using System;

namespace Meadow.Foundation.Communications.Rfid
{
    public class RfidReadResult : EventArgs
    {
        public RfidValidationStatus Status { get; set; }
        public byte[] RfidTag { get; set; }
    }
}