using System.Collections.Generic;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Discovered or configured UDS module / ECU info.
/// </summary>
public record UdsModuleInfo(
    UdsAddress Address,
    string Name,
    string? EcuName,
    string? PartNumber,
    string? SoftwareVersion,
    string? HardwareNumber,
    string? Vin,
    IReadOnlyList<UdsDtc> Dtcs)
{
    /// <summary>Convenience constructor for the 11-bit case, which is most of them.</summary>
    public UdsModuleInfo(
        ushort txId,
        ushort rxId,
        string name,
        string? ecuName,
        string? partNumber,
        string? softwareVersion,
        string? hardwareNumber,
        string? vin,
        IReadOnlyList<UdsDtc> dtcs)
        : this(UdsAddress.Standard(txId, rxId), name, ecuName, partNumber, softwareVersion,
               hardwareNumber, vin, dtcs)
    {
    }

    public uint TxId => Address.TxId;

    public uint RxId => Address.RxId;

    public bool IsExtended => Address.IsExtended;

    public int DtcCount => Dtcs.Count;

    public bool HasFaults => DtcCount > 0;

    /// <summary>
    /// Widths follow the addressing mode. These used to be <c>:X3</c> unconditionally, which turns
    /// a 29-bit identifier into three nibbles of a different address entirely — a display that is
    /// not merely unhelpful but wrong in a way that reads as correct.
    /// </summary>
    public string TxIdHex => Address.TxIdHex;

    public string RxIdHex => Address.RxIdHex;

    public string AddressSummary => Address.Summary;

    /// <summary>Spelled out for the module list, where the addressing mode is worth seeing.</summary>
    public string AddressingText => Address.IsExtended ? "29-bit" : "11-bit";
}
