namespace Meadow.Foundation.Telematics.J1979;

/// <summary>
/// SAE J1979 Mode $01 PID $01 bit positions for readiness monitor status.
/// Byte B covers continuous monitors; bytes C and D cover non-continuous monitors.
/// In bytes C/D, each bit position corresponds to the same monitor (C=supported, D=incomplete).
/// </summary>
public static class ReadinessMonitorBits
{
    // ── Byte B — continuous monitors ────────────────────────────────────────
    public const byte MisfireSupported        = 0x01;
    public const byte FuelSystemSupported     = 0x02;
    public const byte ComprehensiveSupported  = 0x04;

    public const byte MisfireIncomplete       = 0x10;
    public const byte FuelSystemIncomplete    = 0x20;
    public const byte ComprehensiveIncomplete = 0x40;

    // ── Bytes C / D — non-continuous monitors (same bit per byte) ───────────
    public const byte CatalystBit            = 0x01;
    public const byte HeatedCatalystBit      = 0x02;
    public const byte EvapSystemBit          = 0x04;
    public const byte SecondaryAirBit        = 0x08;
    public const byte AcRefrigerantBit       = 0x10;
    public const byte OxygenSensorBit        = 0x20;
    public const byte OxygenSensorHeaterBit  = 0x40;
    public const byte EgrSystemBit           = 0x80;
}
