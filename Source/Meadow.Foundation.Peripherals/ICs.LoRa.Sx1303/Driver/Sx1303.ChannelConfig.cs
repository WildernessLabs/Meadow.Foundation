using System;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.LoRa;

/// <summary>
/// Describes one IF chain (receive channel) configuration
/// </summary>
public class IfChainConfig
{
    /// <summary>Which SX1250 RF front-end serves this channel</summary>
    public Sx1303.RadioPath Radio { get; set; }

    /// <summary>Frequency offset from the radio's centre frequency, in Hz (signed)</summary>
    public int FrequencyOffsetHz { get; set; }

    /// <summary>Whether this chain is enabled</summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Describes the channel plan programmed into the SX1303 concentrator
/// </summary>
public class ChannelPlan
{
    /// <summary>
    /// Centre frequency for radio A (SX1250 #0), in Hz
    /// </summary>
    public uint RadioAFrequencyHz { get; set; }

    /// <summary>
    /// Centre frequency for radio B (SX1250 #1), in Hz
    /// </summary>
    public uint RadioBFrequencyHz { get; set; }

    /// <summary>
    /// Multi-SF IF chains (0-7).  Each chain listens on all SF5-SF12.
    /// </summary>
    public IfChainConfig[] MultiSfChains { get; set; } = new IfChainConfig[8];

    /// <summary>
    /// Single-SF (STD) LoRa channel, used for BW500 / fast uplink
    /// </summary>
    public IfChainConfig? LoRaStdChain { get; set; }

    /// <summary>
    /// Spreading factor for the single-SF channel (ignored when LoRaStdChain is null)
    /// </summary>
    public Sx1303.SpreadingFactor LoRaStdSf { get; set; } = Sx1303.SpreadingFactor.SF8;

    /// <summary>
    /// Bandwidth for the single-SF channel
    /// </summary>
    public Sx1303.Bandwidth LoRaStdBw { get; set; } = Sx1303.Bandwidth.BW500;

    // ── Pre-built standard channel plans ────────────────────────────────────

    /// <summary>
    /// US915 sub-band 1 (channels 0-7 + one BW500 uplink channel)
    /// Radio A centred at 902.7 MHz, Radio B centred at 903.0 MHz.
    /// Matches the first 8-channel block used by LoRaWAN US915.
    /// </summary>
    public static ChannelPlan US915_SubBand1 { get; } = Build_US915_SubBand1();

    /// <summary>
    /// EU868 standard 8-channel plan.
    /// Radio A centred at 868.1 MHz, Radio B centred at 868.5 MHz.
    /// </summary>
    public static ChannelPlan EU868 { get; } = Build_EU868();

    // ── Factory helpers ──────────────────────────────────────────────────────

    private static ChannelPlan Build_US915_SubBand1()
    {
        // US915 channels 0-7: 902.3 + n*0.2 MHz, SF7-SF10, BW125 kHz
        // Radio A: 902.7 MHz (serves channels 0-3 with ±200 kHz offsets)
        // Radio B: 903.5 MHz (serves channels 4-7)
        const uint radioA = 902_700_000;
        const uint radioB = 903_500_000;

        var plan = new ChannelPlan
        {
            RadioAFrequencyHz = radioA,
            RadioBFrequencyHz = radioB,
            MultiSfChains = new IfChainConfig[8],
            LoRaStdChain = new IfChainConfig
            {
                Radio = Sx1303.RadioPath.RadioA,
                FrequencyOffsetHz = 300_000,  // 903.0 MHz BW500
                Enabled = true,
            },
            LoRaStdSf = Sx1303.SpreadingFactor.SF8,
            LoRaStdBw = Sx1303.Bandwidth.BW500,
        };

        // Channels 0-3 on Radio A
        int[] offsetsA = { -400_000, -200_000, 0, 200_000 };
        for (int i = 0; i < 4; i++)
        {
            plan.MultiSfChains[i] = new IfChainConfig
            {
                Radio = Sx1303.RadioPath.RadioA,
                FrequencyOffsetHz = offsetsA[i],
                Enabled = true,
            };
        }

        // Channels 4-7 on Radio B
        int[] offsetsB = { -400_000, -200_000, 0, 200_000 };
        for (int i = 0; i < 4; i++)
        {
            plan.MultiSfChains[i + 4] = new IfChainConfig
            {
                Radio = Sx1303.RadioPath.RadioB,
                FrequencyOffsetHz = offsetsB[i],
                Enabled = true,
            };
        }

        return plan;
    }

    private static ChannelPlan Build_EU868()
    {
        // EU868 default channels: 868.1, 868.3, 868.5 + extended
        const uint radioA = 868_100_000;
        const uint radioB = 868_500_000;

        var plan = new ChannelPlan
        {
            RadioAFrequencyHz = radioA,
            RadioBFrequencyHz = radioB,
            MultiSfChains = new IfChainConfig[8],
        };

        // Spread 8 channels across the two radios with 200 kHz spacing
        var offsets = new (Sx1303.RadioPath radio, int offset)[]
        {
            (Sx1303.RadioPath.RadioA,          0),
            (Sx1303.RadioPath.RadioA,  200_000),
            (Sx1303.RadioPath.RadioA,  400_000),
            (Sx1303.RadioPath.RadioA, -200_000),
            (Sx1303.RadioPath.RadioB,          0),
            (Sx1303.RadioPath.RadioB,  200_000),
            (Sx1303.RadioPath.RadioB, -200_000),
            (Sx1303.RadioPath.RadioB, -400_000),
        };

        for (int i = 0; i < 8; i++)
        {
            plan.MultiSfChains[i] = new IfChainConfig
            {
                Radio = offsets[i].radio,
                FrequencyOffsetHz = offsets[i].offset,
                Enabled = true,
            };
        }

        return plan;
    }
}
