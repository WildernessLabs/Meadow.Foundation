namespace Meadow.Foundation.Telematics.OBD2;

public enum DtcCategory : byte
{
    /// <summary>
    /// Powertain
    /// </summary>
    P = 0b0000_0000,
    /// <summary>
    /// Chassis
    /// </summary>
    C = 0b0100_0000,
    /// <summary>
    /// Body
    /// </summary>
    B = 0b1000_0000,
    /// <summary>
    /// Network
    /// </summary>
    U = 0b1100_0000,
}
