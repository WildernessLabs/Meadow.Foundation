using System;

namespace Meadow.Foundation.Sensors.Radio.Rfid
{
    public class RfidReadResult : EventArgs
    {
        public RfidValidationStatus Status { get; set; }
        public byte[] RfidTag { get; set; }
    }
}