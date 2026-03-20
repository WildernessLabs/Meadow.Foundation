namespace Meadow.Foundation.VFDs;

// DEV NOTE: these are purely reverse engineered from a drive
// The enum is absolutely missing values, and it's not obvious yet how to decode multiple errors at once

/// <summary>
/// Represents error codes for the Franklin Electric CerusXDrive Variable Frequency Drive.
/// These codes correspond to specific error conditions that can occur during operation.
/// </summary> 
public enum ErrorCodes
{
    /// <summary>
    /// Error code 82: Indicates that output phase U is missing or disconnected.
    /// </summary>
    OutputPhaseUMissing = 82,

    /// <summary>
    /// Error code 83: Indicates that output phase V is missing or disconnected.
    /// </summary>
    OutputPhaseVMissing = 83,

    /// <summary>
    /// Error code 84: Indicates that output phase W is missing or disconnected.
    /// </summary>
    OutputPhaseWMissing = 84,
}