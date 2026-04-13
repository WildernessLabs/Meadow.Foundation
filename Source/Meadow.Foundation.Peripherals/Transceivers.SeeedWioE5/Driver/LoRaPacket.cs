namespace Meadow.Foundation.Transceivers;

/// <summary>
/// Represents a received LoRa packet in P2P test mode
/// </summary>
public class LoRaPacket
{
    /// <summary>
    /// Raw payload bytes
    /// </summary>
    public byte[] Payload { get; }

    /// <summary>
    /// Received signal strength in dBm
    /// </summary>
    public int Rssi { get; }

    /// <summary>
    /// Signal-to-noise ratio in dB
    /// </summary>
    public int Snr { get; }

    /// <summary>
    /// Creates a new LoRaPacket
    /// </summary>
    public LoRaPacket(byte[] payload, int rssi, int snr)
    {
        Payload = payload;
        Rssi = rssi;
        Snr = snr;
    }
}
