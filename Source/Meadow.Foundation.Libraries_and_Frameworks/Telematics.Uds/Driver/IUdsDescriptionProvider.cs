namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Supplies the human-readable half of UDS decoding — DID names and value formatting, DTC text,
/// fault-type and negative-response descriptions.
/// </summary>
/// <remarks>
/// The protocol code in this assembly never hardcodes these strings: a host application supplies a
/// provider (typically backed by an editable catalog file) so the vocabulary can grow without a
/// rebuild. <see cref="NullUdsDescriptions"/> is the tables-free fallback.
/// </remarks>
public interface IUdsDescriptionProvider
{
    /// <summary>Returns a display name for a Data Identifier.</summary>
    string GetDidName(ushort did);

    /// <summary>Formats raw DID bytes for display (ASCII, hex, enumerated or scaled numeric).</summary>
    string FormatDidValue(ushort did, byte[] data);

    /// <summary>Returns the description for a base DTC code such as "P0300".</summary>
    string GetDtcDescription(string baseCode);

    /// <summary>Returns the ISO 15031-6 / SAE J2012 fault-type byte (FTB) description.</summary>
    string GetFaultTypeDescription(byte ftb);

    /// <summary>Returns the ISO 14229-1 negative response code description.</summary>
    string GetNrcDescription(byte nrc);
}
