using System;
using System.Collections.Generic;

namespace Meadow.Foundation.Transceivers.Waveshare;

public partial class Sx1303
{
    private const ushort RX_BUFFER_ADDR = 0x4000;
    private const int PKT_HEAD_METADATA = 9;
    private const int PKT_TAIL_METADATA = 13; // status through checksum (excluding ts metrics)

    /// <summary>
    /// Represents a received LoRa or FSK packet.
    /// </summary>
    public class RxPacket
    {
        /// <summary>IF channel index (0-7 multi-SF, 8 LoRa service, 9 FSK).</summary>
        public byte Channel { get; set; }
        /// <summary>LoRa spreading factor (5-12).</summary>
        public byte SpreadingFactor { get; set; }
        /// <summary>LoRa coding rate (1=4/5, 2=4/6, 3=4/7, 4=4/8).</summary>
        public byte CodingRate { get; set; }
        /// <summary>Whether CRC was enabled for this packet.</summary>
        public bool CrcEnabled { get; set; }
        /// <summary>Demodulator ID that received this packet (0-15 multi-SF, 16 service, 17 FSK).</summary>
        public byte ModemId { get; set; }
        /// <summary>Measured frequency offset from the channel center, in Hz.</summary>
        public int FreqOffsetHz { get; set; }
        /// <summary>Raw payload bytes.</summary>
        public byte[] Payload { get; set; } = Array.Empty<byte>();
        /// <summary>True if the payload CRC check failed.</summary>
        public bool CrcError { get; set; }
        /// <summary>True if the syncword was not detected correctly.</summary>
        public bool SyncError { get; set; }
        /// <summary>True if the LoRa header could not be decoded.</summary>
        public bool HeaderError { get; set; }
        /// <summary>Signal-to-noise ratio in dB (0.25 dB resolution).</summary>
        public float SnrDb { get; set; }
        /// <summary>RSSI of the channel in dBm (negative value).</summary>
        public int RssiChannel { get; set; }
        /// <summary>RSSI of the signal in dBm (negative value).</summary>
        public int RssiSignal { get; set; }
        /// <summary>Internal concentrator timestamp in microseconds (32-bit counter).</summary>
        public uint Timestamp { get; set; }
    }

    /// <summary>
    /// Fetches all available packets from the RX buffer.
    /// </summary>
    /// <returns>A list of received packets, or an empty list if none are available.</returns>
    public List<RxPacket> Receive()
    {
        var packets = new List<RxPacket>();

        // Read number of bytes in the RX FIFO (read twice, take larger — per HAL workaround)
        int nbBytes1 = ReadRxBufferByteCount();
        int nbBytes2 = ReadRxBufferByteCount();
        int nbBytes = Math.Max(nbBytes1, nbBytes2);

        if (nbBytes == 0)
            return packets;

        // Bulk read from RX FIFO (address 0x4000, FIFO mode)
        byte[] buffer = MemoryReadFifo(RX_BUFFER_ADDR, nbBytes);

        // Parse packets from buffer
        int offset = 0;
        while (offset < buffer.Length - PKT_HEAD_METADATA)
        {
            // Check syncword
            if (buffer[offset] != 0xA5 || buffer[offset + 1] != 0xC0)
            {
                // No more valid packets
                break;
            }

            byte payloadLen = buffer[offset + 2];

            // Verify we have enough data for the full packet
            int minPacketLen = PKT_HEAD_METADATA + payloadLen + PKT_TAIL_METADATA;
            if (offset + minPacketLen > buffer.Length)
                break;

            var pkt = new RxPacket();
            pkt.Channel = buffer[offset + 3];
            pkt.CrcEnabled = (buffer[offset + 4] & 0x01) != 0;
            pkt.CodingRate = (byte)((buffer[offset + 4] >> 1) & 0x07);
            pkt.SpreadingFactor = (byte)((buffer[offset + 4] >> 4) & 0x0F);
            pkt.ModemId = buffer[offset + 5];

            // 20-bit signed frequency offset
            int freqRaw = buffer[offset + 6]
                        | (buffer[offset + 7] << 8)
                        | ((buffer[offset + 8] & 0x0F) << 16);
            if ((freqRaw & 0x80000) != 0) // sign-extend from 20 bits
                freqRaw |= unchecked((int)0xFFF00000);
            // Convert to Hz (for 125 kHz BW): raw * 125000 * 32 / 64 / 524288
            pkt.FreqOffsetHz = (int)(freqRaw * 0.11920929);

            // Payload
            pkt.Payload = new byte[payloadLen];
            Array.Copy(buffer, offset + PKT_HEAD_METADATA, pkt.Payload, 0, payloadLen);

            // Tail metadata (after payload)
            int tailOffset = offset + PKT_HEAD_METADATA + payloadLen;
            pkt.CrcError = (buffer[tailOffset] & 0x01) != 0;
            pkt.SyncError = (buffer[tailOffset] & 0x04) != 0;
            pkt.HeaderError = (buffer[tailOffset] & 0x08) != 0;

            // SNR: signed byte in 0.25 dB steps
            pkt.SnrDb = (sbyte)buffer[tailOffset + 1] * 0.25f;

            // RSSI (unsigned byte, actual dBm is roughly -raw)
            pkt.RssiChannel = -buffer[tailOffset + 2];
            pkt.RssiSignal = -buffer[tailOffset + 3];

            // Timestamp (32-bit, little-endian, microseconds)
            pkt.Timestamp = (uint)(buffer[tailOffset + 6]
                          | (buffer[tailOffset + 7] << 8)
                          | (buffer[tailOffset + 8] << 16)
                          | (buffer[tailOffset + 9] << 24));

            // CRC payload (2 bytes) + num_ts_metrics (1 byte)
            byte numTsMetrics = buffer[tailOffset + 12];
            int totalPktLen = PKT_HEAD_METADATA + payloadLen + PKT_TAIL_METADATA + (2 * numTsMetrics);

            packets.Add(pkt);
            offset += totalPktLen;
        }

        return packets;
    }

    private int ReadRxBufferByteCount()
    {
        byte msb = ReadRegister(Registers.RxTopRxBufferNbBytesMsb);
        byte lsb = ReadRegister(Registers.RxTopRxBufferNbBytesLsb);
        return (msb << 8) | lsb;
    }

    /// <summary>
    /// Reads from the RX FIFO at 0x4000. This is a FIFO read (auto-incrementing),
    /// distinct from a normal memory read.
    /// </summary>
    private byte[] MemoryReadFifo(ushort addr, int length)
    {
        // FIFO read uses the same SPI frame as memory read
        return MemoryRead(addr, length);
    }

    /// <summary>
    /// Enables the concentrator modems and global RX, loads firmware, and starts receiving.
    /// Call this after ConfigureChannelizer, ConfigureSyncword, and InitializeRadios.
    /// </summary>
    public void StartConcentrator()
    {
        EnableModems();
        LoadAgcFirmware();
        LoadArbFirmware();

        // GPS timestamp enable
        WriteBitField(Registers.TimestampGpsCtrl, 0, 1, 1);  // GPS_EN
        WriteBitField(Registers.TimestampGpsCtrl, 1, 1, 1);  // GPS_POL

        // Config done GPIO
        WriteRegister(Registers.GpioGpioOutL, 0x01);
    }
}
