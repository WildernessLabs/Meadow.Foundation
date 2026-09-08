namespace Meadow.Foundation.Telematics.J1979;

/// <summary>
/// OBD2 Service $09 (Vehicle Information) PID identifiers.
/// </summary>
public enum VehicleInfoPid : byte
{
    SupportedPids = 0x00,
    Vin           = 0x02,
    CalibrationId = 0x04,
    Cvn           = 0x06,
    EcuName       = 0x0A,
}
