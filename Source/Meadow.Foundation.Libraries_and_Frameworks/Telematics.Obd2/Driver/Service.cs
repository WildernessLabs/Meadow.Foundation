namespace Meadow.Foundation.Telematics.OBD2;

public enum Service : byte
{
    Current = 0x01,
    FreezeFrame = 0x02,
    StoredDtcs = 0x03,
    ClearDtcs = 0x04,
    TestResusltsO2 = 0x05,
    TestResultsOther = 0x06,
    PendingDtcs = 0x07,
    ControlOperations = 0x08,
    VehicleInfo = 0x09,
    PermanentDtcs = 0x0a
}
