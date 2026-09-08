namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// A DTC as an ECU stores and reports it: the two-byte base code, the fault type byte
/// and the ISO 14229-1 status mask. The encode-side counterpart to <see cref="UdsDtc"/>.
/// </summary>
public record UdsDtcRecord(byte High, byte Mid, byte FaultType, UdsDtcStatusMask Status)
{
    /// <summary>The four bytes as they appear in a Service $19 response record.</summary>
    public byte[] ToBytes() => [High, Mid, FaultType, (byte)Status];

    /// <summary>The base code in standard notation, e.g. "P0300".</summary>
    public string BaseCode => UdsProtocol.DecodeBaseDtc(High, Mid);

    /// <summary>
    /// Builds a record from a base code string ("P0300", or "P0300-13" to include a fault type)
    /// and a status mask. Returns null when the code is not parseable.
    /// </summary>
    public static UdsDtcRecord? FromCode(string code, UdsDtcStatusMask status)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;

        var text = code.Trim().ToUpperInvariant();
        byte ftb = 0;

        var dash = text.IndexOf('-');
        if (dash > 0)
        {
            var ftbText = text[(dash + 1)..];
            if (!byte.TryParse(ftbText, System.Globalization.NumberStyles.HexNumber, null, out ftb)) return null;
            text = text[..dash];
        }

        if (text.Length != 5) return null;

        int category = text[0] switch { 'P' => 0, 'C' => 1, 'B' => 2, 'U' => 3, _ => -1 };
        if (category < 0) return null;

        if (!byte.TryParse(text.Substring(1, 1), System.Globalization.NumberStyles.HexNumber, null, out var d1) ||
            !byte.TryParse(text.Substring(2, 1), System.Globalization.NumberStyles.HexNumber, null, out var d2) ||
            !byte.TryParse(text.Substring(3, 2), System.Globalization.NumberStyles.HexNumber, null, out var lo))
        {
            return null;
        }

        if (d1 > 3) return null;

        byte hi = (byte)((category << 6) | (d1 << 4) | d2);
        return new UdsDtcRecord(hi, lo, ftb, status);
    }
}
