using System;

namespace Meadow.Foundation.Transceivers.Waveshare;

/// <summary>
/// US915 channel plan (902-928 MHz).
/// 64 x 125 kHz uplink channels (902.3–914.9 MHz, 200 kHz spacing)
/// + 8 x 500 kHz uplink channels (903.0–914.2 MHz, 1.6 MHz spacing)
/// organized into 8 sub-bands of 8 x 125 kHz + 1 x 500 kHz each.
///
/// The SX1303 has 8 multi-SF demodulators, so one sub-band of 8 channels
/// fits perfectly. The two radios are centered to cover the sub-band with
/// minimal IF offset.
/// </summary>
public static class Us915ChannelPlan
{
    // US915 125 kHz uplink channels: 902.3 + n*0.2 MHz, n=0..63
    private const uint BASE_FREQ_125K = 902_300_000;
    private const uint STEP_125K = 200_000;

    // US915 500 kHz uplink channels: 903.0 + n*1.6 MHz, n=0..7
    private const uint BASE_FREQ_500K = 903_000_000;
    private const uint STEP_500K = 1_600_000;

    /// <summary>
    /// Creates a channel plan for the specified US915 sub-band (1-8).
    /// Each sub-band covers 8 x 125 kHz channels + 1 x 500 kHz channel.
    ///
    /// Sub-band 1: channels 0-7   (902.3–903.7 MHz) + 500k ch0 (903.0 MHz)
    /// Sub-band 2: channels 8-15  (903.9–905.3 MHz) + 500k ch1 (904.6 MHz)
    /// ...
    /// Sub-band 8: channels 56-63 (913.5–914.9 MHz) + 500k ch7 (914.2 MHz)
    /// </summary>
    /// <param name="subBand">Sub-band number (1-8).</param>
    /// <returns>A configured channel plan for the requested sub-band.</returns>
    public static ChannelPlan ForSubBand(int subBand)
    {
        if (subBand < 1 || subBand > 8)
            throw new ArgumentOutOfRangeException(nameof(subBand), "Sub-band must be 1-8");

        int sbIndex = subBand - 1;
        int firstChannel = sbIndex * 8;

        // Calculate the 8 channel center frequencies
        uint[] channelFreqs = new uint[8];
        for (int i = 0; i < 8; i++)
        {
            channelFreqs[i] = BASE_FREQ_125K + (uint)(firstChannel + i) * STEP_125K;
        }

        // 500 kHz channel for this sub-band
        uint freq500k = BASE_FREQ_500K + (uint)sbIndex * STEP_500K;

        // Place Radio A and Radio B to minimize IF offsets.
        // Radio A covers channels 0-3, Radio B covers channels 4-7.
        // Center each radio between its 4 channels.
        uint radioAFreq = (channelFreqs[0] + channelFreqs[3]) / 2;
        uint radioBFreq = (channelFreqs[4] + channelFreqs[7]) / 2;

        // Snap to nearest 100 Hz to avoid rounding artifacts
        radioAFreq = (radioAFreq / 100) * 100;
        radioBFreq = (radioBFreq / 100) * 100;

        var channels = new Sx1303.ChannelConfig[10];

        // Channels 0-3 on Radio A
        for (int i = 0; i < 4; i++)
        {
            channels[i] = new Sx1303.ChannelConfig
            {
                Enabled = true,
                Radio = 0,
                FreqOffsetHz = (int)(channelFreqs[i] - radioAFreq),
            };
        }

        // Channels 4-7 on Radio B
        for (int i = 4; i < 8; i++)
        {
            channels[i] = new Sx1303.ChannelConfig
            {
                Enabled = true,
                Radio = 1,
                FreqOffsetHz = (int)(channelFreqs[i] - radioBFreq),
            };
        }

        // Channel 8: LoRa service — 500 kHz channel on whichever radio is closer
        int radio500k = Math.Abs((long)freq500k - radioAFreq) <= Math.Abs((long)freq500k - radioBFreq) ? 0 : 1;
        uint radioRef500k = radio500k == 0 ? radioAFreq : radioBFreq;
        channels[8] = new Sx1303.ChannelConfig
        {
            Enabled = true,
            Radio = radio500k,
            FreqOffsetHz = (int)(freq500k - radioRef500k),
        };

        // Channel 9: FSK — disabled
        channels[9] = new Sx1303.ChannelConfig
        {
            Enabled = false,
            Radio = 0,
            FreqOffsetHz = 0,
        };

        return new ChannelPlan
        {
            Name = $"US915 Sub-band {subBand} (ch{firstChannel}-{firstChannel + 7}, 500k ch{sbIndex})",
            RadioAFreqHz = radioAFreq,
            RadioBFreqHz = radioBFreq,
            Channels = channels,
        };
    }
}
