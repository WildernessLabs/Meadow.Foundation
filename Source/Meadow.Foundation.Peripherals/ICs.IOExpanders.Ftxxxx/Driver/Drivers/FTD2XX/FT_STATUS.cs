namespace FTD2XX;

/// <summary>
/// Status values for FTDI devices.
/// </summary>
public enum FT_STATUS
{
    /// <summary>
    /// Status OK
    /// </summary>
    FT_OK = 0,

    /// <summary>
    /// The device handle is invalid
    /// </summary>
    FT_INVALID_HANDLE,

    /// <summary>
    /// Device not found
    /// </summary>
    FT_DEVICE_NOT_FOUND,

    /// <summary>
    /// Device is not open
    /// </summary>
    FT_DEVICE_NOT_OPENED,

    /// <summary>
    /// IO error
    /// </summary>
    FT_IO_ERROR,

    /// <summary>
    /// Insufficient resources
    /// </summary>
    FT_INSUFFICIENT_RESOURCES,

    /// <summary>
    /// A parameter was invalid
    /// </summary>
    FT_INVALID_PARAMETER,

    /// <summary>
    /// The requested baud rate is invalid
    /// </summary>
    FT_INVALID_BAUD_RATE,

    /// <summary>
    /// Device not opened for erase
    /// </summary>
    FT_DEVICE_NOT_OPENED_FOR_ERASE,

    /// <summary>
    /// Device not poened for write
    /// </summary>
    FT_DEVICE_NOT_OPENED_FOR_WRITE,

    /// <summary>
    /// Failed to write to device
    /// </summary>
    FT_FAILED_TO_WRITE_DEVICE,

    /// <summary>
    /// Failed to read the device EEPROM
    /// </summary>
    FT_EEPROM_READ_FAILED,

    /// <summary>
    /// Failed to write the device EEPROM
    /// </summary>
    FT_EEPROM_WRITE_FAILED,

    /// <summary>
    /// Failed to erase the device EEPROM
    /// </summary>
    FT_EEPROM_ERASE_FAILED,

    /// <summary>
    /// An EEPROM is not fitted to the device
    /// </summary>
    FT_EEPROM_NOT_PRESENT,

    /// <summary>
    /// Device EEPROM is blank
    /// </summary>
    FT_EEPROM_NOT_PROGRAMMED,

    /// <summary>
    /// Invalid arguments
    /// </summary>
    FT_INVALID_ARGS,

    /// <summary>
    /// An other error has occurred
    /// </summary>
    FT_OTHER_ERROR
};

public static class FT_STATUS_EXTENSIONS
{
    public static bool IsOK(this FT_STATUS status)
    {
        return status == FT_STATUS.FT_OK;
    }

    public static void ThrowIfNotOK(this FT_STATUS status)
    {
        switch (status)
        {
            case FT_STATUS.FT_OK:
                return;
            case FT_STATUS.FT_DEVICE_NOT_FOUND:
                throw new FT_EXCEPTION("FTDI device not found.");
            case FT_STATUS.FT_DEVICE_NOT_OPENED:
                throw new FT_EXCEPTION("FTDI device not opened.");
            case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_ERASE:
                throw new FT_EXCEPTION("FTDI device not opened for erase.");
            case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_WRITE:
                throw new FT_EXCEPTION("FTDI device not opened for write.");
            case FT_STATUS.FT_EEPROM_ERASE_FAILED:
                throw new FT_EXCEPTION("Failed to erase FTDI device EEPROM.");
            case FT_STATUS.FT_EEPROM_NOT_PRESENT:
                throw new FT_EXCEPTION("No EEPROM fitted to FTDI device.");
            case FT_STATUS.FT_EEPROM_NOT_PROGRAMMED:
                throw new FT_EXCEPTION("FTDI device EEPROM not programmed.");
            case FT_STATUS.FT_EEPROM_READ_FAILED:
                throw new FT_EXCEPTION("Failed to read FTDI device EEPROM.");
            case FT_STATUS.FT_EEPROM_WRITE_FAILED:
                throw new FT_EXCEPTION("Failed to write FTDI device EEPROM.");
            case FT_STATUS.FT_FAILED_TO_WRITE_DEVICE:
                throw new FT_EXCEPTION("Failed to write to FTDI device.");
            case FT_STATUS.FT_INSUFFICIENT_RESOURCES:
                throw new FT_EXCEPTION("Insufficient resources.");
            case FT_STATUS.FT_INVALID_ARGS:
                throw new FT_EXCEPTION("Invalid arguments for FTD2XX function call.");
            case FT_STATUS.FT_INVALID_BAUD_RATE:
                throw new FT_EXCEPTION("Invalid Baud rate for FTDI device.");
            case FT_STATUS.FT_INVALID_HANDLE:
                throw new FT_EXCEPTION("Invalid handle for FTDI device.");
            case FT_STATUS.FT_INVALID_PARAMETER:
                throw new FT_EXCEPTION("Invalid parameter for FTD2XX function call.");
            case FT_STATUS.FT_IO_ERROR:
                throw new FT_EXCEPTION("FTDI device IO error.");
            case FT_STATUS.FT_OTHER_ERROR:
                throw new FT_EXCEPTION("An unexpected error has occurred when trying to communicate with the FTDI device.");
            default:
                throw new FT_EXCEPTION($"Unknown Error: {status}");
        }
    }
}