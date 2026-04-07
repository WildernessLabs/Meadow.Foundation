namespace Meadow.Foundation.Transceivers.Waveshare;

/// <summary>
/// Defines a LoRa gateway channel plan — radio center frequencies and IF channel offsets.
/// </summary>
public class ChannelPlan
{
    /// <summary>Center frequency in Hz for Radio A.</summary>
    public uint RadioAFreqHz { get; set; }

    /// <summary>Center frequency in Hz for Radio B.</summary>
    public uint RadioBFreqHz { get; set; }

    /// <summary>
    /// The 10 IF channel configurations (0-7 multi-SF, 8 LoRa service, 9 FSK).
    /// </summary>
    public Sx1303.ChannelConfig[] Channels { get; set; } = new Sx1303.ChannelConfig[10];

    /// <summary>Short description of this plan (e.g. "US915 Sub-band 1").</summary>
    public string Name { get; set; } = string.Empty;
}
