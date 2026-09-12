using System;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// A request/response address pair for one UDS module, on either 11-bit or 29-bit CAN identifiers.
/// </summary>
/// <remarks>
/// The legislated OBD-II window (<c>0x7E0</c>–<c>0x7E7</c> request, response +8) reaches the
/// powertrain and very little else. Body and chassis controllers on most vehicles built this
/// century answer only on manufacturer 11-bit addresses or on 29-bit normal-fixed addressing, so an
/// address is not a <see cref="ushort"/> — it needs to say how wide it is as well as what it is.
/// </remarks>
public readonly record struct UdsAddress(uint TxId, uint RxId, bool IsExtended)
{
    /// <summary>ISO 15765-2 normal-fixed request: <c>0x18DA</c> + target + source.</summary>
    public const uint NormalFixedRequestBase = 0x18DA0000;

    /// <summary>Normal-fixed response: <c>0x18DA</c> + tester + target.</summary>
    public const uint NormalFixedResponseBase = 0x18DA0000;

    /// <summary>The conventional external test equipment address.</summary>
    public const byte TesterAddress = 0xF1;

    /// <summary>An 11-bit pair where the response is the request plus eight, as ISO 15765-4 defines.</summary>
    public static UdsAddress Standard(uint txId) => new(txId, txId + 8, false);

    /// <summary>An 11-bit pair with an explicitly given response identifier.</summary>
    public static UdsAddress Standard(uint txId, uint rxId) => new(txId, rxId, false);

    /// <summary>
    /// A 29-bit normal-fixed pair for one target ECU: request <c>0x18DA{target}F1</c>, response
    /// <c>0x18DAF1{target}</c>.
    /// </summary>
    public static UdsAddress NormalFixed(byte ecuAddress) => new(
        NormalFixedRequestBase | (uint)(ecuAddress << 8) | TesterAddress,
        NormalFixedResponseBase | ((uint)TesterAddress << 8) | ecuAddress,
        true);

    /// <summary>The ECU's own address byte, for a normal-fixed pair. Zero for 11-bit pairs.</summary>
    public byte EcuAddress => IsExtended ? (byte)((TxId >> 8) & 0xFF) : (byte)0;

    /// <summary>
    /// The address a flow-control frame goes to. For 11-bit that is the request identifier; for
    /// normal-fixed it is likewise the request identifier, since the tester is always the sender.
    /// </summary>
    public uint FlowControlId => TxId;

    public string TxIdHex => Format(TxId);

    public string RxIdHex => Format(RxId);

    public string Summary => $"TX: {TxIdHex} → RX: {RxIdHex}";

    /// <summary>
    /// Formats an identifier at its real width. A 29-bit identifier rendered <c>:X3</c> loses its
    /// top five nibbles and comes out looking like an unrelated 11-bit address, which is worse
    /// than useless — it is wrong in a way that reads as right.
    /// </summary>
    private string Format(uint id) => IsExtended ? $"0x{id:X8}" : $"0x{id:X3}";

    public override string ToString() => Summary;
}
