using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>
/// Interface for UDS (ISO 14229) diagnostic client operations.
/// </summary>
public interface IUdsScanner
{
    /// <summary>
    /// Scans for responsive UDS modules across standard and extended CAN addresses.
    /// </summary>
    Task<IReadOnlyList<UdsModuleInfo>> DiscoverModulesAsync(CancellationToken ct = default);

    /// <summary>
    /// Scans the address bands in <paramref name="plan"/>, reporting progress as it goes.
    /// A full three-tier sweep visits several hundred addresses and takes tens of seconds, so the
    /// caller needs both the progress and a cancellation token that is actually honoured.
    /// </summary>
    Task<IReadOnlyList<UdsModuleInfo>> DiscoverModulesAsync(
        UdsDiscoveryPlan plan,
        IProgress<UdsDiscoveryProgress>? progress,
        CancellationToken ct = default);

    /// <summary>
    /// Probes an explicit list of addresses rather than a range — the fast path for a vehicle whose
    /// modules are already known from a previous visit.
    /// </summary>
    Task<IReadOnlyList<UdsModuleInfo>> ProbeAddressesAsync(
        IEnumerable<UdsAddress> addresses,
        IProgress<UdsDiscoveryProgress>? progress = null,
        CancellationToken ct = default);

    /// <summary>Reads DTCs from a module at any addressing width.</summary>
    Task<IReadOnlyList<UdsDtc>> ReadModuleDtcsAsync(UdsAddress address, CancellationToken ct = default);

    /// <summary>Clears DTCs on a module at any addressing width.</summary>
    Task<bool> ClearModuleDtcsAsync(UdsAddress address, CancellationToken ct = default);

    /// <summary>Reads a DID from a module at any addressing width.</summary>
    Task<UdsDidValue?> ReadDidAsync(UdsAddress address, ushort did, CancellationToken ct = default);

    /// <summary>Switches the diagnostic session on a module at any addressing width.</summary>
    Task<bool> SetDiagnosticSessionAsync(UdsAddress address, UdsSessionType session, CancellationToken ct = default);

    /// <summary>Sends a Tester Present frame to a module at any addressing width.</summary>
    Task SendTesterPresentAsync(UdsAddress address, bool suppressResponse = true, CancellationToken ct = default);

    /// <summary>
    /// Reads UDS DTCs with full 3-byte codes and status masks from a specific module using Service $19 $02.
    /// </summary>
    Task<IReadOnlyList<UdsDtc>> ReadModuleDtcsAsync(ushort txId, ushort rxId, CancellationToken ct = default);

    /// <summary>
    /// Clears DTCs on a specific module using Service $14 $FF $FF $FF.
    /// </summary>
    Task<bool> ClearModuleDtcsAsync(ushort txId, ushort rxId, CancellationToken ct = default);

    /// <summary>
    /// Clears DTCs across all modules using functional broadcast address $7DF.
    /// </summary>
    Task<bool> ClearAllDtcsAsync(CancellationToken ct = default);

    /// <summary>
    /// Reads a Data Identifier (DID) from a specific module using Service $22.
    /// </summary>
    Task<UdsDidValue?> ReadDidAsync(ushort txId, ushort rxId, ushort did, CancellationToken ct = default);

    /// <summary>
    /// Switches the active diagnostic session using Service $10.
    /// </summary>
    Task<bool> SetDiagnosticSessionAsync(ushort txId, ushort rxId, UdsSessionType session, CancellationToken ct = default);

    /// <summary>
    /// Sends a Tester Present frame (Service $3E) to keep an active diagnostic session alive.
    /// </summary>
    Task SendTesterPresentAsync(ushort txId, bool suppressResponse = true, CancellationToken ct = default);
}
