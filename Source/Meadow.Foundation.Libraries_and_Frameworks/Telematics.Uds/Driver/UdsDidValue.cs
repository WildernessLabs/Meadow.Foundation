namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Decoded UDS Data Identifier (DID) value from Service $22.
/// </summary>
public record UdsDidValue(
    ushort Did,
    string Name,
    byte[] RawBytes,
    string DisplayValue)
{
    public string DidHex => $"0x{Did:X4}";
}
