namespace Meadow.Foundation.Sensors.Radio.Rfid
{
    public enum RfidValidationStatus
    {
        /// <summary>
        /// Rfid read was successful
        /// </summary>
        Ok = 0,

        /// <summary>
        /// Rfid was read but failed checksum validation
        /// </summary>
        ChecksumFailed,

        /// <summary>
        /// Attempt to read the Rfid failed due to the data read from the serial port not being in an expected format.
        /// </summary>
        InvalidDataFormat
    }
}
