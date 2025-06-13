using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sensors.Camera.MaixSense;

public class A010
{
    private readonly ISerialPort _serialPort;
    private readonly List<byte> _buffer = new List<byte>();
    private static readonly byte[] PacketHeader = { 0x00, 0xFF };
    private const byte PacketEnd = 0xDD;
    private const int HeaderSize = 2;
    private const int LengthFieldSize = 2;
    private const int MetadataSize = 16;
    private const int ChecksumSize = 1;
    private const int EndMarkerSize = 1;
    private const int MinPacketOverhead = HeaderSize + LengthFieldSize + MetadataSize + ChecksumSize + EndMarkerSize; // 22 bytes
    private const int MaxBufferSize = 2 * 1024 * 1024; // 2MB buffer limit

    // Event fired when a complete and valid image packet is received
    public event EventHandler<ImagePacketEventArgs> ImagePacketReceived;
    public event EventHandler<PacketErrorEventArgs> PacketError;

    public A010(ISerialPort serialPort)
    {
        _serialPort = serialPort;
        _serialPort.DataReceived += OnSerialDataReceived;
    }

    private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            // Read all available data
            int bytesToRead = _serialPort.BytesToRead;
            if (bytesToRead == 0) return;

            byte[] newData = new byte[bytesToRead];
            int bytesRead = _serialPort.Read(newData, 0, bytesToRead);

            // Add to buffer
            _buffer.AddRange(newData.Take(bytesRead));

            // Prevent buffer overflow
            if (_buffer.Count > MaxBufferSize)
            {
                int keepSize = MaxBufferSize / 2;
                _buffer.RemoveRange(0, _buffer.Count - keepSize);
                OnPacketError("Buffer overflow - discarded old data");
            }

            // Extract complete packets
            ExtractPackets();
        }
        catch (Exception ex)
        {
            OnPacketError($"Error in serial data reception: {ex.Message}");
        }
    }

    private void ExtractPackets()
    {
        while (true)
        {
            // Need at least header + length field to proceed
            if (_buffer.Count < HeaderSize + LengthFieldSize)
                break;

            // Find start of next packet
            int headerIndex = FindPacketHeader();
            if (headerIndex == -1)
            {
                // No header found, clean up buffer
                CleanupBuffer();
                break;
            }

            // Remove any data before the header (garbage data)
            if (headerIndex > 0)
            {
                _buffer.RemoveRange(0, headerIndex);
            }

            // Check if we have enough data to read the length field
            if (_buffer.Count < HeaderSize + LengthFieldSize)
                break;

            // Parse packet length (remaining bytes after length field)
            ushort remainingBytes = ParseLengthField();
            int totalPacketSize = HeaderSize + LengthFieldSize + remainingBytes;

            // Validate packet length
            if (remainingBytes < MetadataSize + ChecksumSize + EndMarkerSize)
            {
                OnPacketError($"Invalid packet length: {remainingBytes} bytes (too small)");
                _buffer.RemoveRange(0, HeaderSize + LengthFieldSize); // Skip this invalid packet
                continue;
            }

            // Check if complete packet is available
            if (_buffer.Count < totalPacketSize)
                break; // Wait for more data

            // Extract the complete packet
            byte[] packetBytes = new byte[totalPacketSize];
            _buffer.CopyTo(0, packetBytes, 0, totalPacketSize);

            // Validate packet structure and checksum
            if (ValidatePacket(packetBytes, totalPacketSize))
            {
                // Extract packet components
                var packet = ParsePacket(packetBytes, remainingBytes);

                // Notify subscribers about the received packet
                ImagePacketReceived?.Invoke(this, new ImagePacketEventArgs(packet));
            }

            // Remove the processed packet from buffer
            _buffer.RemoveRange(0, totalPacketSize);
        }
    }

    private int FindPacketHeader()
    {
        for (int i = 0; i <= _buffer.Count - HeaderSize; i++)
        {
            if (IsHeaderAt(i))
            {
                return i;
            }
        }
        return -1;
    }

    private bool IsHeaderAt(int index)
    {
        return _buffer[index] == PacketHeader[0] && _buffer[index + 1] == PacketHeader[1];
    }

    private ushort ParseLengthField()
    {
        // Assuming little-endian format - adjust if needed
        return (ushort)(_buffer[HeaderSize] | (_buffer[HeaderSize + 1] << 8));
    }

    private bool ValidatePacket(byte[] packetBytes, int totalSize)
    {
        // Check end marker
        if (packetBytes[totalSize - 1] != PacketEnd)
        {
            OnPacketError($"Invalid end marker: 0x{packetBytes[totalSize - 1]:X2} (expected 0x{PacketEnd:X2})");
            return false;
        }

        // Calculate and verify checksum
        int calculatedSum = 0;
        for (int i = 0; i < totalSize - ChecksumSize - EndMarkerSize; i++)
        {
            calculatedSum += packetBytes[i];
        }

        byte expectedChecksum = (byte)(calculatedSum & 0xFF);
        byte actualChecksum = packetBytes[totalSize - ChecksumSize - EndMarkerSize];

        if (expectedChecksum != actualChecksum)
        {
            OnPacketError($"Checksum mismatch: calculated 0x{expectedChecksum:X2}, received 0x{actualChecksum:X2}");
            return false;
        }

        return true;
    }

    private ImagePacket ParsePacket(byte[] packetBytes, ushort remainingBytes)
    {
        // Extract metadata (16 bytes after length field)
        byte[] metadata = new byte[MetadataSize];
        Array.Copy(packetBytes, HeaderSize + LengthFieldSize, metadata, 0, MetadataSize);

        // Extract image data (between metadata and checksum+end)
        int imageDataLength = remainingBytes - MetadataSize - ChecksumSize - EndMarkerSize;
        byte[] imageData = new byte[imageDataLength];
        if (imageDataLength > 0)
        {
            Array.Copy(packetBytes, HeaderSize + LengthFieldSize + MetadataSize, imageData, 0, imageDataLength);
        }

        // Parse metadata fields (you may want to adjust these based on actual format)
        uint packetSerialNumber = BitConverter.ToUInt32(metadata, 0);
        uint packetLength = BitConverter.ToUInt32(metadata, 4);
        ushort width = BitConverter.ToUInt16(metadata, 8);
        ushort height = BitConverter.ToUInt16(metadata, 10);
        // Remaining 4 bytes for other metadata

        return new ImagePacket
        {
            SerialNumber = packetSerialNumber,
            PacketLength = packetLength,
            Width = width,
            Height = height,
            ImageData = imageData,
            Metadata = metadata,
            TotalPacketSize = packetBytes.Length
        };
    }

    private void CleanupBuffer()
    {
        // Keep potential partial header at the end of buffer
        if (_buffer.Count > 0)
        {
            if (_buffer.Count >= 1 && _buffer[_buffer.Count - 1] == PacketHeader[0])
            {
                byte lastByte = _buffer[_buffer.Count - 1];
                _buffer.Clear();
                _buffer.Add(lastByte);
            }
            else
            {
                _buffer.Clear();
            }
        }
    }

    private void OnPacketError(string message)
    {
        PacketError?.Invoke(this, new PacketErrorEventArgs(message));
        Console.WriteLine($"A010 Packet Error: {message}");
    }

    public void Connect()
    {
        if (!_serialPort.IsOpen)
        {
            _serialPort.Open();
        }
    }

    public void Disconnect()
    {
        if (_serialPort.IsOpen)
        {
            _serialPort.Close();
        }
        _buffer.Clear();
    }

    // Get current buffer statistics for debugging
    public BufferStats GetBufferStats()
    {
        return new BufferStats
        {
            BufferSize = _buffer.Count,
            MaxBufferSize = MaxBufferSize
        };
    }
}

// Complete image packet with parsed metadata
public class ImagePacket
{
    public uint SerialNumber { get; set; }
    public uint PacketLength { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }
    public byte[] ImageData { get; set; }
    public byte[] Metadata { get; set; }
    public int TotalPacketSize { get; set; }
}

// Event arguments for image packet reception
public class ImagePacketEventArgs : EventArgs
{
    public ImagePacket Packet { get; }
    public DateTime Timestamp { get; }

    public ImagePacketEventArgs(ImagePacket packet)
    {
        Packet = packet;
        Timestamp = DateTime.Now;
    }
}

// Event arguments for packet errors
public class PacketErrorEventArgs : EventArgs
{
    public string ErrorMessage { get; }
    public DateTime Timestamp { get; }

    public PacketErrorEventArgs(string errorMessage)
    {
        ErrorMessage = errorMessage;
        Timestamp = DateTime.Now;
    }
}

// Buffer statistics for monitoring
public class BufferStats
{
    public int BufferSize { get; set; }
    public int MaxBufferSize { get; set; }
    public double BufferUsagePercentage => (double)BufferSize / MaxBufferSize * 100;
}