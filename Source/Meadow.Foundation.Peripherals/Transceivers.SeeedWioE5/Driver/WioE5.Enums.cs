namespace Meadow.Foundation.Transceivers;

/// <summary>
/// LoRa operating mode
/// </summary>
public enum LoRaMode
{
    /// <summary>LoRaWAN ABP (Activation by Personalization)</summary>
    LoRaWanAbp,
    /// <summary>LoRaWAN OTAA (Over-the-Air Activation)</summary>
    LoRaWanOtaa,
    /// <summary>P2P test mode</summary>
    Test
}

/// <summary>
/// LoRa spreading factor
/// </summary>
public enum SpreadingFactor
{
    /// <summary>Spreading factor 7 (highest data rate)</summary>
    SF7 = 7,
    /// <summary>Spreading factor 8</summary>
    SF8 = 8,
    /// <summary>Spreading factor 9</summary>
    SF9 = 9,
    /// <summary>Spreading factor 10</summary>
    SF10 = 10,
    /// <summary>Spreading factor 11</summary>
    SF11 = 11,
    /// <summary>Spreading factor 12 (longest range)</summary>
    SF12 = 12
}

/// <summary>
/// LoRa signal bandwidth
/// </summary>
public enum Bandwidth
{
    /// <summary>125 kHz bandwidth</summary>
    BW125 = 125,
    /// <summary>250 kHz bandwidth</summary>
    BW250 = 250,
    /// <summary>500 kHz bandwidth</summary>
    BW500 = 500
}
