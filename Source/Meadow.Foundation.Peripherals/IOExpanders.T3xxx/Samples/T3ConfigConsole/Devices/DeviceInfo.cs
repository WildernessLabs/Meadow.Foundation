using System.Net;

namespace T3ConfigConsole.Devices;

public class DeviceInfo
{
    public int SerialNumber { get; set; }
    public float FirmwareVersion { get; set; }
    public byte ModbusAddress { get; set; }
    public ushort ProductModel { get; set; }
    public byte HardwareRevision { get; set; }
    public IPAddress IpAddress { get; set; } = IPAddress.None;

    public string FirmwareVersionString => FirmwareVersion.ToString("F1");
    public string Summary => $"Model: 0x{ProductModel:X4}  FW: {FirmwareVersionString}  HW: {HardwareRevision}  Addr: {ModbusAddress}";
}
