using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Transceivers;

/// <summary>
/// Represents a GroveWioE5 LoRaWAN transceiver module
/// </summary>
/// <remarks>
/// The WioE5 is a low-cost LoRaWAN module supporting Class A/B/C operation
/// with AT command interface over UART
/// </remarks>
public class WioE5Modem :
    IModem,
    ITemperatureSensor,
    IVoltageSensor,
    IDisposable
{
    private readonly IAtTransport _transport;
    private readonly object _locker = new object();

    private bool _disposed = false;

    public Voltage? Voltage => throw new NotImplementedException();

    /// <summary>
    /// Raised when the module successfully joins a LoRaWAN network
    /// </summary>
    public event EventHandler<EventArgs>? Joined;

    /// <summary>
    /// Raised when network join attempt fails
    /// </summary>
    public event EventHandler<EventArgs>? JoinFailed;

    /// <summary>
    /// Raised when data is received from the LoRaWAN network
    /// </summary>
    public event EventHandler<DataReceivedEventArgs>? DataReceived;

    public WioE5Modem(ISerialMessagePort messagePort)
        : this(new SerialAtTransport(messagePort))
    {
    }

    internal WioE5Modem(IAtTransport transport)
    {
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _transport.UnsolicitedMessage += OnUnsolicited;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _transport.UnsolicitedMessage -= OnUnsolicited;
        (_transport as IDisposable)?.Dispose();
        _disposed = true;
    }

    private void OnUnsolicited(object? sender, string line)
    {
        Resolver.Log?.Debug($"[Unsolicited Message] {line}", "WioE5");

        // Parse critical device events from AT command spec
        if (line.Contains("+JOIN: Done", StringComparison.OrdinalIgnoreCase))
        {
            Joined?.Invoke(this, EventArgs.Empty);
        }
        else if (line.Contains("+JOIN: Join failed", StringComparison.OrdinalIgnoreCase))
        {
            JoinFailed?.Invoke(this, EventArgs.Empty);
        }
        else if (line.StartsWith("+MSG: PORT:", StringComparison.OrdinalIgnoreCase))
        {
            // Parse format: +MSG: PORT: 8; RX: "data"
            var parts = line.Split(';');
            if (parts.Length >= 2)
            {
                var portPart = parts[0].Replace("+MSG: PORT:", "").Trim();
                var dataPart = parts[1].Replace("RX:", "").Trim().Trim('"');

                if (int.TryParse(portPart, out var port))
                {
                    DataReceived?.Invoke(this, new DataReceivedEventArgs
                    {
                        Port = port,
                        Data = dataPart
                    });
                }
            }
        }
    }

    private async Task<AtResponse> SendAtAsync(string cmd,
                                               CancellationToken cancellationToken = default,
                                               TimeSpan? timeout = null)
    {
        return await _transport.SendCommand(cmd, cancellationToken, timeout);
    }

    /// <summary>
    /// Gets the firmware version of the WioE5 module
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The firmware version string</returns>
    /// <exception cref="InvalidOperationException">Thrown if the command fails or returns an unexpected response</exception>
    public async Task<string> GetFirmwareVersion(CancellationToken cancellationToken = default)
    {
        var resp = await SendAtAsync($"{Commands.VER}?", cancellationToken);
        if (!resp.IsSuccess)
        {
            throw new InvalidOperationException($"AT+VER failed: {resp.ErrorMessage}");
        }

        // Typical response:
        // +VER: 1.0.6
        // OK
        var verLine = resp.Lines.FirstOrDefault(l => l.StartsWith("+VER", StringComparison.OrdinalIgnoreCase));
        if (verLine == null)
        {
            throw new InvalidOperationException($"Unexpected {Commands.VER} response: {string.Join(",", resp.Lines)}");
        }
        var parts = verLine.Split(':', 2);
        return parts.Length == 2 ? parts[1].Trim() : verLine;
    }

    /// <summary>
    /// Sets the work mode of the LoRaWAN module
    /// </summary>
    /// <param name="mode">The desired work mode (LWABP, LWOTAA, or Test)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown if mode change fails</exception>
    public async Task SetWorkMode(WorkMode mode, CancellationToken cancellationToken = default)
    {
        var resp = await SendAtAsync($"{Commands.MODE}={mode}", cancellationToken);
        if (!resp.IsSuccess)
        {
            throw new InvalidOperationException($"AT+MODE failed: {resp.ErrorMessage}");
        }
    }

    /// <summary>
    /// Joins a LoRaWAN network using OTAA (Over-The-Air Activation)
    /// </summary>
    /// <param name="devEui">Device EUI (hexadecimal string)</param>
    /// <param name="appEui">Application EUI (hexadecimal string)</param>
    /// <param name="appKey">Application Key (hexadecimal string)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="InvalidOperationException">Thrown if join operation fails</exception>
    public async Task JoinNetwork(string devEui, string appEui, string appKey,
                                    CancellationToken cancellationToken = default)
    {
        await SendAtAsync($"{Commands.DEVEUI}={devEui}", cancellationToken);
        await SendAtAsync($"{Commands.APPEUI}={appEui}", cancellationToken);
        await SendAtAsync($"{Commands.APPKEY}={appKey}", cancellationToken);

        var resp = await SendAtAsync($"{Commands.JOIN}=OTAA", cancellationToken, TimeSpan.FromSeconds(30));
        if (!resp.IsSuccess)
        {
            throw new InvalidOperationException($"AT+JOIN failed: {resp.ErrorMessage}");
        }

        // TODO: wait for unsolicited +EVT:JOINED
    }

    /// <summary>
    /// Sends data over the LoRaWAN network
    /// </summary>
    /// <param name="data">The data to send as a byte array</param>
    /// <param name="confirm">Whether to request confirmation from the network</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status code (0 on success)</returns>
    /// <exception cref="InvalidOperationException">Thrown if send operation fails</exception>
    public async Task<int> SendData(byte[] data, bool confirm = false,
                                              CancellationToken cancellationToken = default)
    {
        var hex = BitConverter.ToString(data).Replace("-", "");
        var cmd = confirm
            ? $"{Commands.SEND}=CONFIRMED,{hex}"
            : $"{Commands.SEND}=UNCONFIRMED,{hex}";

        var resp = await SendAtAsync(cmd, cancellationToken, TimeSpan.FromSeconds(30));
        if (!resp.IsSuccess)
        {
            throw new InvalidOperationException($"{Commands.SEND} failed: {resp.ErrorMessage}");
        }

        // Response example might include a message ID or status; parse as needed.
        // For now, return 0 on success.
        return 0;
    }

    /// <inheritdoc/>
    public async Task<Temperature> Read()
    {
        var resp = await SendAtAsync($"{Commands.TEMP}?", CancellationToken.None);
        if (!resp.IsSuccess)
        {
            throw new InvalidOperationException($"{Commands.TEMP} failed: {resp.ErrorMessage}");
        }

        // Typical response:
        // +VER: 1.0.6
        var verLine = resp.Lines.FirstOrDefault(l => l.StartsWith("+TEMP", StringComparison.OrdinalIgnoreCase));
        if (verLine == null)
        {
            throw new InvalidOperationException($"Unexpected {Commands.TEMP} response: {string.Join(",", resp.Lines)}");
        }
        var parts = verLine.Split(':', 2);
        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"Unexpected {Commands.TEMP} response: {string.Join(",", resp.Lines)}");
        }
        return new Temperature(double.Parse(parts[1].Trim()), Temperature.UnitType.Celsius);
    }

    /// <inheritdoc/>
    public async ValueTask<Voltage> ReadVoltage()
    {
        var resp = await SendAtAsync($"{Commands.VDD}?", CancellationToken.None);
        if (!resp.IsSuccess)
        {
            throw new InvalidOperationException($"{Commands.VDD} failed: {resp.ErrorMessage}");
        }

        // Typical response:
        // +VDD: 3.30V
        var verLine = resp.Lines.FirstOrDefault(l => l.StartsWith("+VDD", StringComparison.OrdinalIgnoreCase));
        if (verLine == null)
        {
            throw new InvalidOperationException($"Unexpected {Commands.VDD} response: {string.Join(",", resp.Lines)}");
        }
        var parts = verLine.Split(':', 2);
        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"Unexpected {Commands.VDD} response: {string.Join(",", resp.Lines)}");
        }
        return new Voltage(double.Parse(parts[1].Trim().TrimEnd('V')), Units.Voltage.UnitType.Volts);
    }

    [Obsolete("Use ReadVoltage", true)]
    Task<Voltage> ISensor<Voltage>.Read()
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetMaxPayloadLength()
    {
        var resp = await SendAtAsync($"{Commands.LW_LEN}", CancellationToken.None);
        if (!resp.IsSuccess)
        {
            throw new InvalidOperationException($"{Commands.LW_LEN} failed: {resp.ErrorMessage}");
        }

        // Typical response:
        // +VDD: 3.30V
        var verLine = resp.Lines.FirstOrDefault(l => l.StartsWith("+LW", StringComparison.OrdinalIgnoreCase));
        if (verLine == null)
        {
            throw new InvalidOperationException($"Unexpected {Commands.LW_LEN} response: {string.Join(",", resp.Lines)}");
        }
        var parts = verLine.Split(',', 2);
        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"Unexpected {Commands.LW_LEN} response: {string.Join(",", resp.Lines)}");
        }
        return int.Parse(parts[1].Trim());
    }
}