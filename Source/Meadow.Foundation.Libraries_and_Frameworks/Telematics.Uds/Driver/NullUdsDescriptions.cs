using System;
using System.Linq;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// The tables-free <see cref="IUdsDescriptionProvider"/>: every lookup falls back to a raw
/// hex rendering. Lets the library run standalone (on firmware, or in tests) with no data files.
/// </summary>
public class NullUdsDescriptions : IUdsDescriptionProvider
{
    /// <summary>A shared instance — the provider holds no state.</summary>
    public static readonly NullUdsDescriptions Instance = new();

    /// <inheritdoc/>
    public string GetDidName(ushort did) => $"DID 0x{did:X4}";

    /// <summary>
    /// Renders printable ASCII as text and everything else as hex. That much is inferable from the
    /// bytes themselves and needs no vocabulary; a catalog-backed provider still decides per DID.
    /// </summary>
    public string FormatDidValue(ushort did, byte[] data)
    {
        if (data.Length == 0) return "";

        var text = System.Text.Encoding.ASCII.GetString(data).Trim('\0', ' ');
        if (text.Length > 0 && text.All(c => c >= 32 && c <= 126)) return text;

        return string.Join(" ", Array.ConvertAll(data, b => $"{b:X2}"));
    }

    /// <inheritdoc/>
    public string GetDtcDescription(string baseCode) => "";

    /// <inheritdoc/>
    public string GetFaultTypeDescription(byte ftb) => $"Failure Type 0x{ftb:X2}";

    /// <inheritdoc/>
    public string GetNrcDescription(byte nrc) => $"NRC 0x{nrc:X2}";
}
