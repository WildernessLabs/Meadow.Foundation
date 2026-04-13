using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Transceivers;

/// <summary>
/// Driver for the Seeed Wio-E5 LoRa/LoRaWAN module (AT command interface over UART)
/// </summary>
public partial class WioE5 : IDisposable
{
    private readonly ISerialMessagePort _port;
    private readonly bool _createdPort;
    private readonly SemaphoreSlim _commandLock = new SemaphoreSlim(1, 1);
    private TaskCompletionSource<string[]>? _pendingResponse;
    private readonly List<string> _responseLines = new List<string>();
    private bool _receiving;
    private int _pendingRxRssi;
    private int _pendingRxSnr;

    /// <summary>
    /// Raised when a LoRa packet is received in TEST mode
    /// </summary>
    public event EventHandler<LoRaPacket>? PacketReceived;

    /// <summary>
    /// Create a WioE5 from a pre-created serial message port
    /// </summary>
    public WioE5(ISerialMessagePort port)
    {
        _port = port;
        _port.MessageReceived += OnMessageReceived;
    }

    /// <summary>
    /// Create a WioE5 from a serial port name
    /// </summary>
    public WioE5(SerialPortName portName, int baudRate = 9600)
        : this(portName.CreateSerialMessagePort(
            Encoding.ASCII.GetBytes("\r\n"),
            preserveDelimiter: false,
            readBufferSize: 512))
    {
        _createdPort = true;
    }

    /// <summary>
    /// Open the serial port and verify communication with the module
    /// </summary>
    public async Task<bool> Initialize()
    {
        if (!_port.IsOpen)
        {
            _port.Open();
        }

        // Send a bare CR/LF to interrupt any active RX mode, then wait for the module to settle
        _port.Write(Encoding.ASCII.GetBytes("\r\n"));
        await Task.Delay(500);

        var response = await SendCommand("AT");
        return response.Length > 0 && response[0].Contains("OK");
    }

    /// <summary>
    /// Get the firmware version string
    /// </summary>
    public async Task<string> GetVersion()
    {
        var response = await SendCommand("AT+VER");
        // +VER: 4.0.11
        return response.Length > 0 ? response[0].Replace("+VER: ", "") : string.Empty;
    }

    /// <summary>
    /// Get device identifiers (DevAddr, DevEui, AppEui)
    /// </summary>
    public async Task<(string DevAddr, string DevEui, string AppEui)> GetIds()
    {
        var response = await SendCommand("AT+ID");
        string devAddr = string.Empty, devEui = string.Empty, appEui = string.Empty;
        foreach (var line in response)
        {
            if (line.Contains("DevAddr")) devAddr = line.Split(',')[1].Trim();
            else if (line.Contains("DevEui")) devEui = line.Split(',')[1].Trim();
            else if (line.Contains("AppEui")) appEui = line.Split(',')[1].Trim();
        }
        return (devAddr, devEui, appEui);
    }

    private void OnMessageReceived(object sender, SerialMessageData e)
    {
        var line = Encoding.ASCII.GetString(e.Message).Trim();
        if (string.IsNullOrEmpty(line)) return;

        if (_receiving)
        {
            // First line: +TEST: LEN:5, RSSI:-42, SNR:7
            if (line.StartsWith("+TEST: LEN:"))
            {
                HandleRxHeader(line);
                return;
            }
            // Second line: +TEST: RX "48656C6C6F"
            if (line.StartsWith("+TEST: RX "))
            {
                HandleRxData(line);
                return;
            }
        }

        // Accumulate response lines for the pending command
        if (_pendingResponse != null)
        {
            _responseLines.Add(line);

            if (IsTerminalLine(line))
            {
                var lines = _responseLines.ToArray();
                _responseLines.Clear();
                _pendingResponse.TrySetResult(lines);
            }
        }
    }

    private bool IsTerminalLine(string line)
    {
        // Single-line responses
        if (line.StartsWith("+AT:") ||
            line.StartsWith("+VER:") ||
            line.StartsWith("+MODE:") ||
            line.StartsWith("+DR:") ||
            line.StartsWith("+CH:") ||
            line.Contains("ERROR"))
        {
            return true;
        }

        // +ID returns three lines (DevAddr, DevEui, AppEui) — terminal on AppEui
        if (line.StartsWith("+ID:") && line.Contains("AppEui"))
        {
            return true;
        }

        // TEST mode multi-line responses — terminal lines
        if (line == "+TEST: TX DONE" ||
            line == "+TEST: CW" ||
            line.StartsWith("+TEST: RFCFG"))
        {
            return true;
        }

        return false;
    }

    internal async Task<string[]> SendCommand(string command, int timeoutMs = 5000)
    {
        await _commandLock.WaitAsync();
        try
        {
            _pendingResponse = new TaskCompletionSource<string[]>();
            _responseLines.Clear();

            _port.Write(Encoding.ASCII.GetBytes(command + "\r\n"));

            var tcs = _pendingResponse;
            var completed = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));

            _pendingResponse = null;

            if (completed == tcs.Task)
            {
                return await tcs.Task;
            }

            return Array.Empty<string>();
        }
        finally
        {
            _commandLock.Release();
        }
    }

    private void HandleRxHeader(string line)
    {
        // +TEST: LEN:5, RSSI:-42, SNR:7
        try
        {
            var parts = line.Substring("+TEST: ".Length).Split(',');
            _pendingRxRssi = int.Parse(parts[1].Split(':')[1].Trim());
            _pendingRxSnr = int.Parse(parts[2].Split(':')[1].Trim());
        }
        catch { }
    }

    private void HandleRxData(string line)
    {
        // +TEST: RX "48656C6C6F"
        try
        {
            var start = line.IndexOf('"') + 1;
            var hex = line.Substring(start, line.LastIndexOf('"') - start);
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            PacketReceived?.Invoke(this, new LoRaPacket(bytes, _pendingRxRssi, _pendingRxSnr));
        }
        catch { }
    }

    /// <summary>
    /// Releases resources used by the WioE5 driver
    /// </summary>
    public void Dispose()
    {
        _commandLock.Dispose();
        if (_createdPort && _port.IsOpen)
        {
            _port.Close();
        }
        GC.SuppressFinalize(this);
    }
}
