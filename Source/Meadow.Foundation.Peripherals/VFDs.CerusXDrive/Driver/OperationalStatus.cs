using System;

namespace Meadow.Foundation.VFDs;

// DEV NOTE: it's not at all obvious how these work with the device
// testing does not at all match the brief, terrible description of decoding in the data sheet
//
// standard boot status:                8832 = 8192 + 512 + 128        = 0b00100010_10000000
// turn on just HAND and it changes to: 9472 = 8192 + 1024 + 256       = 0b00100101_00000000

/// <summary>
/// Represents the operational status of the Franklin Electric CerusXDrive Variable Frequency Drive.
/// The values indicate different combinations of Run and Stop LED states on the drive.
/// </summary>
[Flags]
public enum OperationalStatus
{
    /// <summary>
    /// Run LED is off, Stop LED is on. 
    /// Indicates the drive is stopped and ready.
    /// </summary>
    RunOff_StopOn = 0,

    /// <summary>
    /// Run LED is blinking, Stop LED is on.
    /// Indicates the drive is in a transitional state, typically preparing to run.
    /// </summary>
    RunBlink_StopOn = 1,

    /// <summary>
    /// Run LED is on, Stop LED is blinking.
    /// Indicates the drive is running but preparing to stop.
    /// </summary>
    RunOn_StopBlink = 2,

    /// <summary>
    /// Run LED is on, Stop LED is off.
    /// Indicates the drive is running normally.
    /// </summary>
    RunOn_StopOff = 3,

    /// <summary>
    /// Indicates that the drive is in Jog mode, 
    /// which allows manual control of motor speed.
    /// </summary>
    JogActive = 4,
}