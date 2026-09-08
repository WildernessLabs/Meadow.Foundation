using System;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// ISO 14229-1 DTC Status Mask bits.
/// </summary>
[Flags]
public enum UdsDtcStatusMask : byte
{
    None                               = 0x00,
    TestFailed                         = 0x01,
    TestFailedThisOperationCycle       = 0x02,
    PendingDtc                         = 0x04,
    ConfirmedDtc                       = 0x08,
    TestNotCompletedSinceLastClear     = 0x10,
    TestFailedSinceLastClear           = 0x20,
    TestNotCompletedThisOperationCycle = 0x40,
    WarningIndicatorRequested          = 0x80
}
