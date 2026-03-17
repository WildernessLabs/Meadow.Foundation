namespace Meadow.Foundation.Telematics.OBD2;

/// <summary>
/// Models the OBD-II PID 0x01 emissions readiness monitor status.
/// </summary>
public class EmissionsReadinessStatus
{
    // ── Continuous monitors ──────────────────────────────────────────────────
    public bool MisfireSupported             { get; set; } = true;
    public bool MisfireComplete              { get; set; } = true;

    public bool FuelSystemSupported          { get; set; } = true;
    public bool FuelSystemComplete           { get; set; } = true;

    public bool ComprehensiveComponentSupported { get; set; } = true;
    public bool ComprehensiveComponentComplete  { get; set; } = true;

    // ── Non-continuous monitors ──────────────────────────────────────────────
    public bool CatalystSupported            { get; set; } = true;
    public bool CatalystComplete             { get; set; } = true;

    public bool HeatedCatalystSupported      { get; set; } = false;
    public bool HeatedCatalystComplete       { get; set; } = true;

    public bool EvapSystemSupported          { get; set; } = true;
    public bool EvapSystemComplete           { get; set; } = true;

    public bool SecondaryAirSupported        { get; set; } = false;
    public bool SecondaryAirComplete         { get; set; } = true;

    public bool AcRefrigerantSupported       { get; set; } = false;
    public bool AcRefrigerantComplete        { get; set; } = true;

    public bool OxygenSensorSupported        { get; set; } = true;
    public bool OxygenSensorComplete         { get; set; } = true;

    public bool OxygenSensorHeaterSupported  { get; set; } = true;
    public bool OxygenSensorHeaterComplete   { get; set; } = true;

    public bool EgrSystemSupported           { get; set; } = true;
    public bool EgrSystemComplete            { get; set; } = true;

    /// <summary>
    /// Encodes the status into the 4-byte PID 0x01 wire format.
    /// </summary>
    /// <param name="milOn">MIL (Check Engine Light) state.</param>
    /// <param name="dtcCount">Number of confirmed stored DTCs.</param>
    public byte[] ToBytes(bool milOn, byte dtcCount)
    {
        // Byte A: bit7=MIL, bits6-0=DTC count
        byte a = (byte)((milOn ? 0x80 : 0x00) | (dtcCount & 0x7F));

        // Byte B: continuous monitors
        //   Bits 0-2: supported flags
        //   Bits 4-6: incomplete flags (0=complete, 1=incomplete)
        byte b = 0;
        if (MisfireSupported)                b |= 0x01;
        if (FuelSystemSupported)             b |= 0x02;
        if (ComprehensiveComponentSupported) b |= 0x04;
        if (!MisfireComplete)                b |= 0x10;
        if (!FuelSystemComplete)             b |= 0x20;
        if (!ComprehensiveComponentComplete) b |= 0x40;

        // Byte C: non-continuous supported
        byte c = 0;
        if (CatalystSupported)           c |= 0x01;
        if (HeatedCatalystSupported)     c |= 0x02;
        if (EvapSystemSupported)         c |= 0x04;
        if (SecondaryAirSupported)       c |= 0x08;
        if (AcRefrigerantSupported)      c |= 0x10;
        if (OxygenSensorSupported)       c |= 0x20;
        if (OxygenSensorHeaterSupported) c |= 0x40;
        if (EgrSystemSupported)          c |= 0x80;

        // Byte D: non-continuous incomplete (only meaningful if supported)
        byte d = 0;
        if (CatalystSupported           && !CatalystComplete)           d |= 0x01;
        if (HeatedCatalystSupported     && !HeatedCatalystComplete)     d |= 0x02;
        if (EvapSystemSupported         && !EvapSystemComplete)         d |= 0x04;
        if (SecondaryAirSupported       && !SecondaryAirComplete)       d |= 0x08;
        if (AcRefrigerantSupported      && !AcRefrigerantComplete)      d |= 0x10;
        if (OxygenSensorSupported       && !OxygenSensorComplete)       d |= 0x20;
        if (OxygenSensorHeaterSupported && !OxygenSensorHeaterComplete) d |= 0x40;
        if (EgrSystemSupported          && !EgrSystemComplete)          d |= 0x80;

        return [a, b, c, d];
    }
}
