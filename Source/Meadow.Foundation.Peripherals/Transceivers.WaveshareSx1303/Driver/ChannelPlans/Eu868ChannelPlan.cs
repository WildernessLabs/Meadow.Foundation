namespace Meadow.Foundation.Transceivers.Waveshare;

/// <summary>
/// EU868 channel plan (863-870 MHz).
/// Default 8 channels: 3 mandatory (868.1, 868.3, 868.5 MHz)
/// + 5 additional (867.1, 867.3, 867.5, 867.7, 867.9 MHz).
/// All 125 kHz bandwidth.
/// </summary>
public static class Eu868ChannelPlan
{
    /// <summary>
    /// Creates the standard EU868 8-channel plan.
    /// Radio A at 867.5 MHz covers 867.1-867.9 (5 channels).
    /// Radio B at 868.3 MHz covers 868.1-868.5 (3 channels).
    /// </summary>
    /// <returns>A configured channel plan for the EU868 default 8-channel layout.</returns>
    public static ChannelPlan Default()
    {
        uint radioAFreq = 867_500_000;
        uint radioBFreq = 868_300_000;

        var channels = new Sx1303.ChannelConfig[10];

        // Radio A: 867.1, 867.3, 867.5, 867.7, 867.9 MHz
        channels[0] = new Sx1303.ChannelConfig { Enabled = true, Radio = 0, FreqOffsetHz = -400_000 };
        channels[1] = new Sx1303.ChannelConfig { Enabled = true, Radio = 0, FreqOffsetHz = -200_000 };
        channels[2] = new Sx1303.ChannelConfig { Enabled = true, Radio = 0, FreqOffsetHz =        0 };
        channels[3] = new Sx1303.ChannelConfig { Enabled = true, Radio = 0, FreqOffsetHz =  200_000 };
        channels[4] = new Sx1303.ChannelConfig { Enabled = true, Radio = 0, FreqOffsetHz =  400_000 };

        // Radio B: 868.1, 868.3, 868.5 MHz
        channels[5] = new Sx1303.ChannelConfig { Enabled = true, Radio = 1, FreqOffsetHz = -200_000 };
        channels[6] = new Sx1303.ChannelConfig { Enabled = true, Radio = 1, FreqOffsetHz =        0 };
        channels[7] = new Sx1303.ChannelConfig { Enabled = true, Radio = 1, FreqOffsetHz =  200_000 };

        // Service and FSK disabled
        channels[8] = new Sx1303.ChannelConfig { Enabled = false, Radio = 0, FreqOffsetHz = 0 };
        channels[9] = new Sx1303.ChannelConfig { Enabled = false, Radio = 0, FreqOffsetHz = 0 };

        return new ChannelPlan
        {
            Name = "EU868 Default (867.1-868.5 MHz)",
            RadioAFreqHz = radioAFreq,
            RadioBFreqHz = radioBFreq,
            Channels = channels,
        };
    }
}
