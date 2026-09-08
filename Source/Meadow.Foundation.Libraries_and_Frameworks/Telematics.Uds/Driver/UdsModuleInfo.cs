using System.Collections.Generic;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Discovered or configured UDS module / ECU info.
/// </summary>
public record UdsModuleInfo(
    ushort TxId,
    ushort RxId,
    string Name,
    string? EcuName,
    string? PartNumber,
    string? SoftwareVersion,
    string? HardwareNumber,
    string? Vin,
    IReadOnlyList<UdsDtc> Dtcs)
{
    public int DtcCount => Dtcs.Count;
    public bool HasFaults => DtcCount > 0;
    public string TxIdHex => $"0x{TxId:X3}";
    public string RxIdHex => $"0x{RxId:X3}";
    public string AddressSummary => $"TX: 0x{TxId:X3} → RX: 0x{RxId:X3}";
}
