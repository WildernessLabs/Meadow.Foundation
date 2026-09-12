using System;
using System.Collections.Generic;
using System.Linq;

namespace Meadow.Foundation.Telematics.Uds;

/// <summary>One band of addresses to probe, and what to call it while probing.</summary>
public sealed class UdsDiscoveryTier
{
    public string Name { get; set; } = "";

    /// <summary>Addressing width for every address in the tier.</summary>
    public bool IsExtended { get; set; }

    /// <summary>First request identifier, inclusive. For an extended tier this is the ECU address byte.</summary>
    public uint From { get; set; }

    /// <summary>Last request identifier, inclusive. For an extended tier this is the ECU address byte.</summary>
    public uint To { get; set; }

    /// <summary>
    /// Whether this tier runs by default. A tier can be defined but left out of the quick sweep,
    /// which is how a slow exhaustive band stays available without being paid for every scan.
    /// </summary>
    public bool Enabled { get; set; } = true;

    public IEnumerable<UdsAddress> Addresses()
    {
        if (To < From) yield break;

        for (var id = From; id <= To; id++)
        {
            yield return IsExtended
                ? UdsAddress.NormalFixed((byte)(id & 0xFF))
                : UdsAddress.Standard(id);

            if (id == uint.MaxValue) yield break;
        }
    }

    public int Count => To < From ? 0 : (int)(To - From + 1);
}

/// <summary>
/// The address bands a discovery sweep walks, plus friendly names for addresses known in advance.
/// </summary>
/// <remarks>
/// The defaults here are ISO-generic — the legislated window, the rest of the 11-bit diagnostic
/// range, and the whole 29-bit normal-fixed space. No manufacturer's addresses are compiled in;
/// a vehicle-specific address map belongs in a JSON overlay supplied at runtime, the same way DID
/// names do, so adding support for a make is a config edit rather than a release.
/// </remarks>
public sealed class UdsDiscoveryPlan
{
    public List<UdsDiscoveryTier> Tiers { get; set; } = [];

    /// <summary>Optional display names, keyed by request identifier in hex (e.g. <c>0x760</c>).</summary>
    public Dictionary<string, string> Names { get; set; } = [];

    /// <summary>
    /// The built-in plan. Tier 1 is the legislated OBD-II window and is quick; tier 2 is the rest
    /// of the 11-bit diagnostic range, where most body and chassis controllers live; tier 3 is
    /// 29-bit normal-fixed addressing, which several manufacturers use exclusively for everything
    /// outside the powertrain.
    /// </summary>
    public static UdsDiscoveryPlan Default() => new()
    {
        Tiers =
        [
            new UdsDiscoveryTier { Name = "Legislated OBD-II", From = 0x7E0, To = 0x7E7, IsExtended = false },
            new UdsDiscoveryTier { Name = "11-bit manufacturer", From = 0x700, To = 0x7DE, IsExtended = false },
            new UdsDiscoveryTier { Name = "29-bit normal-fixed", From = 0x00, To = 0xFF, IsExtended = true }
        ]
    };

    /// <summary>Just the legislated window — the fast sweep, and what the tool did before.</summary>
    public static UdsDiscoveryPlan LegislatedOnly() => new()
    {
        Tiers = [new UdsDiscoveryTier { Name = "Legislated OBD-II", From = 0x7E0, To = 0x7E7, IsExtended = false }]
    };

    public IEnumerable<UdsDiscoveryTier> EnabledTiers => Tiers.Where(t => t.Enabled && t.Count > 0);

    public int TotalAddresses => EnabledTiers.Sum(t => t.Count);

    /// <summary>A configured name for an address, or null.</summary>
    public string? NameFor(UdsAddress address)
    {
        var key = address.IsExtended ? $"0x{address.TxId:X8}" : $"0x{address.TxId:X3}";
        return Names.TryGetValue(key, out var name) ? name : null;
    }
}

/// <summary>Progress from a discovery sweep, so a long scan can say what it is doing.</summary>
public sealed record UdsDiscoveryProgress(
    string TierName,
    int TierIndex,
    int TierCount,
    int AddressesProbed,
    int AddressesTotal,
    int ModulesFound)
{
    public double Fraction => AddressesTotal <= 0 ? 0 : (double)AddressesProbed / AddressesTotal;

    public string Summary =>
        $"{TierName} ({TierIndex + 1}/{TierCount}) — {AddressesProbed}/{AddressesTotal} addresses, {ModulesFound} found";
}
