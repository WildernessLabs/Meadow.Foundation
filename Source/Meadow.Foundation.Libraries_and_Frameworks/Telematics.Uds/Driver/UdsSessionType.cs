namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Diagnostic session types for Service $10.
/// </summary>
public enum UdsSessionType : byte
{
    DefaultSession                     = 0x01,
    ProgrammingSession                 = 0x02,
    ExtendedDiagnosticSession          = 0x03,
    SafetySystemDiagnosticSession      = 0x04
}
