namespace Meadow.Foundation.Transceivers.Waveshare;

/// <summary>
/// Configuration for starting the SX1303 concentrator as a LoRa gateway.
/// </summary>
public class GatewayConfig
{
    /// <summary>
    /// The channel plan defining radio frequencies and IF channel layout.
    /// Use Us915ChannelPlan.ForSubBand() or Eu868ChannelPlan.Default().
    /// </summary>
    public ChannelPlan ChannelPlan { get; set; } = null!;

    /// <summary>
    /// LoRa syncword: Public for LoRaWAN, Private for point-to-point.
    /// Default: Public.
    /// </summary>
    public Sx1303.SyncwordMode Syncword { get; set; } = Sx1303.SyncwordMode.Public;

    /// <summary>
    /// Which radio provides the 32 MHz reference clock.
    /// Default: RadioA.
    /// </summary>
    public Sx1303.ClockSource ClockSource { get; set; } = Sx1303.ClockSource.RadioA;
}
