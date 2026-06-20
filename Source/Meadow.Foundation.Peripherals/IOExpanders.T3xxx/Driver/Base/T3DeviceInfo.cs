using System.Net;
using System.Text;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// Holds identification and network information for a T3 module discovered via UDP broadcast.
/// </summary>
public class T3DeviceInfo
{
    /// <summary>Gets the IP address of the discovered module.</summary>
    public IPAddress IpAddress { get; }

    /// <summary>Gets the unique serial number of the module.</summary>
    public int SerialNumber { get; }

    /// <summary>Gets the product/model identifier of the module.</summary>
    public T3ModuleModel Model { get; }

    /// <summary>Gets the Modbus node address of the module.</summary>
    public byte ModbusAddress { get; }

    /// <summary>Gets the Modbus TCP port the module listens on.</summary>
    public ushort ModbusPort { get; }

    /// <summary>Gets the firmware (software) version of the module.</summary>
    public ushort FirmwareVersion { get; }

    /// <summary>Gets the hardware revision of the module.</summary>
    public ushort HardwareVersion { get; }

    /// <summary>Gets the panel name configured on the module.</summary>
    public string PanelName { get; }

    /// <summary>Gets the serial number of the parent device, or zero if none.</summary>
    public uint ParentSerialNumber { get; }

    internal T3DeviceInfo(
        IPAddress ipAddress,
        int serialNumber,
        T3ModuleModel model,
        byte modbusAddress,
        ushort modbusPort,
        ushort firmwareVersion,
        ushort hardwareVersion,
        string panelName,
        uint parentSerialNumber)
    {
        IpAddress = ipAddress;
        SerialNumber = serialNumber;
        Model = model;
        ModbusAddress = modbusAddress;
        ModbusPort = modbusPort;
        FirmwareVersion = firmwareVersion;
        HardwareVersion = hardwareVersion;
        PanelName = panelName;
        ParentSerialNumber = parentSerialNumber;
    }

    /// <summary>
    /// Parses a discovery response packet into a <see cref="T3DeviceInfo"/> instance.
    /// Returns <c>null</c> if the packet is too short, has a wrong command byte, or the
    /// device is in bootloader mode.
    /// </summary>
    internal static T3DeviceInfo? FromResponsePacket(byte[] buffer, int length)
    {
        // Minimum packet size to reach hw_version (byte 29)
        if (length < 30) return null;
        if (buffer[0] != T3xxx.DiscoveryResponseCommand) return null;

        // Fields are interleaved: each logical byte sits at an even offset,
        // followed by a reserved/padding byte (the device sends 16-bit words, low byte first).
        int serialNumber =
            buffer[4] |
            (buffer[6] << 8) |
            (buffer[8] << 16) |
            (buffer[10] << 24);

        var model = (T3ModuleModel)buffer[12];
        byte modbusAddress = buffer[14];

        var ip = new IPAddress(new byte[]
        {
            buffer[16], buffer[18], buffer[20], buffer[22]
        });

        ushort modbusPort = (ushort)(buffer[24] | (buffer[25] << 8));
        ushort fwVersion  = (ushort)(buffer[26] | (buffer[27] << 8));
        ushort hwVersion  = (ushort)(buffer[28] | (buffer[29] << 8));

        uint parentSerial = 0;
        string panelName = string.Empty;
        byte ispMode = 0;

        if (length >= 34)
        {
            parentSerial = (uint)(
                buffer[30] |
                (buffer[31] << 8) |
                (buffer[32] << 16) |
                (buffer[33] << 24));
        }

        // panel_name starts at byte 37, 20 bytes
        if (length >= 57)
        {
            panelName = Encoding.ASCII.GetString(buffer, 37, 20).TrimEnd('\0');
        }

        // isp_mode at byte 59
        if (length >= 60)
        {
            ispMode = buffer[59];
        }

        // Ignore devices in bootloader mode
        if (ispMode != 0) return null;

        return new T3DeviceInfo(ip, serialNumber, model, modbusAddress, modbusPort, fwVersion, hwVersion, panelName, parentSerial);
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Model} SN={SerialNumber} IP={IpAddress} Modbus={ModbusAddress} FW={FirmwareVersion}";
}
