namespace Meadow.Foundation.Telematics.OBD2;

public interface IController
{
    string Vin { get; }
    Pid[] SupportedPids { get; }
    short ModuleAddress { get; }

    string? EcuName { get; }
    string? CalibrationId { get; }
    uint? CalibrationVerificationNumber { get; }

    void SetDtc(Dtc dtc);
    void ClearDtc(Dtc dtc);
    void SetPendingDtc(Dtc dtc);
    void ClearPendingDtc(Dtc dtc);
    void SetPermanentDtc(Dtc dtc);
    void ClearPermanentDtc(Dtc dtc);
    void ClearAllDtcs();
}
