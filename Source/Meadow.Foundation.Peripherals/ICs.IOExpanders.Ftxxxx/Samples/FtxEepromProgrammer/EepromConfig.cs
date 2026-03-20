namespace FtxEepromProgrammer;

/// <summary>
/// EEPROM configuration model
/// </summary>
internal class EepromConfig
{
    public string? DeviceType { get; set; }
    public string? VendorId { get; set; }
    public string? ProductId { get; set; }
    public string? Manufacturer { get; set; }
    public string? ManufacturerId { get; set; }
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public ushort MaxPower { get; set; }
    public bool PnP { get; set; }
    public bool SelfPowered { get; set; }
    public bool RemoteWakeup { get; set; }
    public bool PullDownEnable { get; set; }
    public bool SerNumEnable { get; set; }
    public bool UsbVersionEnable { get; set; }
    public string? UsbVersion { get; set; }
    public ChannelConfig? ChannelA { get; set; }
    public ChannelConfig? ChannelB { get; set; }
}

/// <summary>
/// Channel-specific configuration
/// </summary>
internal class ChannelConfig
{
    public bool IsHighCurrent { get; set; }
    public bool IsVCP { get; set; }
    public bool IsFifo { get; set; }
    public bool IsFifoTarget { get; set; }
    public bool IsFastSer { get; set; }
    public string? DriverType { get; set; }
}
