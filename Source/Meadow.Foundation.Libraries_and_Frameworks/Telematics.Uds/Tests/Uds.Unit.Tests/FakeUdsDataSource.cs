using Meadow.Foundation.Telematics.Uds;
using System.Text;

namespace Uds.Unit.Tests;

/// <summary>An in-memory module for server tests.</summary>
public class FakeUdsDataSource : IUdsDataSource
{
    public UdsDtcStatusMask AvailabilityMask { get; set; } =
        UdsDtcStatusMask.TestFailed | UdsDtcStatusMask.PendingDtc | UdsDtcStatusMask.ConfirmedDtc;

    public List<UdsDtcRecord> Dtcs { get; } = [];
    public Dictionary<ushort, byte[]> Dids { get; } = [];
    public int ClearCount { get; private set; }

    public IReadOnlyList<UdsDtcRecord> GetDtcs() => Dtcs;

    public bool TryGetDid(ushort did, out byte[] data)
    {
        if (Dids.TryGetValue(did, out var value))
        {
            data = value;
            return true;
        }
        data = [];
        return false;
    }

    public void ClearDtcs()
    {
        ClearCount++;
        Dtcs.Clear();
    }

    /// <summary>Adds an ASCII DID value.</summary>
    public FakeUdsDataSource WithText(ushort did, string value)
    {
        Dids[did] = Encoding.ASCII.GetBytes(value);
        return this;
    }
}
