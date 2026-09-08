namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Unified Diagnostic Services (ISO 14229-1) service identifiers.
/// </summary>
public enum UdsService : byte
{
    DiagnosticSessionControl       = 0x10,
    EcuReset                       = 0x11,
    ClearDiagnosticInformation     = 0x14,
    ReadDtcInformation             = 0x19,
    ReadDataByIdentifier           = 0x22,
    ReadMemoryByAddress            = 0x23,
    ReadScalingDataByIdentifier    = 0x24,
    SecurityAccess                 = 0x27,
    CommunicationControl           = 0x28,
    ReadDataByPeriodicIdentifier   = 0x2A,
    DynamicallyDefineDataIdentifier= 0x2C,
    WriteDataByIdentifier          = 0x2E,
    InputOutputControlByIdentifier = 0x2F,
    RoutineControl                 = 0x31,
    RequestDownload                = 0x34,
    RequestUpload                  = 0x35,
    TransferData                   = 0x36,
    RequestTransferExit            = 0x37,
    WriteMemoryByAddress           = 0x3D,
    TesterPresent                  = 0x3E,
    ControlDtcSetting              = 0x85,
    NegativeResponse               = 0x7F
}
