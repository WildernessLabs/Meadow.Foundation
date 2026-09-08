using System.Collections.Generic;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Represents a UDS diagnostic trouble code with 3-byte representation (base code + fault type byte)
/// and ISO 14229-1 status mask.
/// </summary>
public record UdsDtc(
    string BaseCode,
    byte FaultType,
    string FaultTypeDescription,
    string FullCode,
    string Description,
    UdsDtcStatusMask Status)
{
    public bool IsConfirmed => Status.HasFlag(UdsDtcStatusMask.ConfirmedDtc);
    public bool IsPending   => Status.HasFlag(UdsDtcStatusMask.PendingDtc);
    public bool IsActive    => Status.HasFlag(UdsDtcStatusMask.TestFailed);
    public bool IsWarning   => Status.HasFlag(UdsDtcStatusMask.WarningIndicatorRequested);

    public string StatusSummary
    {
        get
        {
            var flags = new List<string>();
            if (IsActive)    flags.Add("Active");
            if (IsConfirmed) flags.Add("Confirmed");
            if (IsPending)   flags.Add("Pending");
            if (IsWarning)   flags.Add("MIL");
            return flags.Count > 0 ? string.Join(", ", flags) : "None";
        }
    }
}
