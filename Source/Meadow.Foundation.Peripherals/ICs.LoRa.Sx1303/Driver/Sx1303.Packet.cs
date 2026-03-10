using System;

namespace Meadow.Foundation.ICs.LoRa;

/// <summary>
/// Represents a LoRa packet received by the SX1303 concentrator
/// </summary>
public class LoRaRxPacket
{
    /// <summary>
    /// Centre frequency of the channel on which the packet was received, in Hz
    /// </summary>
    public uint FrequencyHz { get; init; }

    /// <summary>
    /// Spreading factor used
    /// </summary>
    public Sx1303.SpreadingFactor SpreadingFactor { get; init; }

    /// <summary>
    /// Signal bandwidth
    /// </summary>
    public Sx1303.Bandwidth Bandwidth { get; init; }

    /// <summary>
    /// Coding rate
    /// </summary>
    public Sx1303.CodingRate CodingRate { get; init; }

    /// <summary>
    /// RSSI of the channel in dBm
    /// </summary>
    public float Rssi { get; init; }

    /// <summary>
    /// Signal-to-noise ratio in dB
    /// </summary>
    public float Snr { get; init; }

    /// <summary>
    /// Raw packet payload bytes (excluding LoRa PHY header and CRC)
    /// </summary>
    public byte[] Payload { get; init; } = Array.Empty<byte>();

    /// <summary>
    /// Timestamp (internal concentrator counter, 1 µs resolution)
    /// </summary>
    public uint TimestampUs { get; init; }

    /// <inheritdoc/>
    public override string ToString()
        => $"RX {FrequencyHz / 1e6:F3} MHz SF{(int)SpreadingFactor} BW{(int)Bandwidth * 125} " +
           $"RSSI={Rssi:F1} dBm SNR={Snr:F1} dB Len={Payload.Length}";
}

/// <summary>
/// Represents a LoRa packet to transmit via the SX1303 concentrator
/// </summary>
public class LoRaTxPacket
{
    /// <summary>
    /// Centre frequency in Hz (e.g. 923_300_000 for 923.3 MHz)
    /// </summary>
    public uint FrequencyHz { get; set; }

    /// <summary>
    /// Spreading factor to use
    /// </summary>
    public Sx1303.SpreadingFactor SpreadingFactor { get; set; } = Sx1303.SpreadingFactor.SF10;

    /// <summary>
    /// Signal bandwidth
    /// </summary>
    public Sx1303.Bandwidth Bandwidth { get; set; } = Sx1303.Bandwidth.BW125;

    /// <summary>
    /// Coding rate
    /// </summary>
    public Sx1303.CodingRate CodingRate { get; set; } = Sx1303.CodingRate.CR4_5;

    /// <summary>
    /// Transmit power class
    /// </summary>
    public Sx1303.TxPower Power { get; set; } = Sx1303.TxPower.Medium;

    /// <summary>
    /// Preamble length in symbols (default 8)
    /// </summary>
    public ushort PreambleLength { get; set; } = 8;

    /// <summary>
    /// Whether to invert I/Q (set true for LoRaWAN downlinks)
    /// </summary>
    public bool InvertIq { get; set; } = true;

    /// <summary>
    /// Payload bytes to transmit
    /// </summary>
    public byte[] Payload { get; set; } = Array.Empty<byte>();
}
