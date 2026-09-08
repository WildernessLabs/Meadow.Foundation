using System.Collections.Generic;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// The data a <see cref="UdsServer"/> serves: the module's DTC store and its Data Identifiers.
/// Implementations decide where that comes from — live ECU state, a simulator, a config profile.
/// </summary>
public interface IUdsDataSource
{
    /// <summary>The DTCStatusAvailabilityMask this module reports (which status bits it maintains).</summary>
    UdsDtcStatusMask AvailabilityMask { get; }

    /// <summary>The module's current DTCs.</summary>
    IReadOnlyList<UdsDtcRecord> GetDtcs();

    /// <summary>Looks up a Data Identifier. Return false to have the server answer NRC $31.</summary>
    bool TryGetDid(ushort did, out byte[] data);

    /// <summary>Clears the module's DTCs (Service $14).</summary>
    void ClearDtcs();
}
