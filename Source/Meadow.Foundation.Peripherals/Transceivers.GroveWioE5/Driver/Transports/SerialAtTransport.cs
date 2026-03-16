using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Transceivers;

internal class SerialAtTransport : IAtTransport, IDisposable
{
    private readonly ISerialMessagePort _serial;
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(5);

    private readonly List<string> _responseBuffer = new();
    private TaskCompletionSource<string[]>? _pendingResponse;

    public event EventHandler<string>? UnsolicitedMessage;

    private readonly object _lock = new();

    public SerialAtTransport(ISerialMessagePort serial)
    {
        _serial = serial;
        _serial.MessageReceived += SerialMessageReceived;
        if (!_serial.IsOpen)
        {
            _serial.Open();
        }
    }

    public async Task<AtResponse> SendCommand(string command,
                                            CancellationToken cancellationToken = default,
                                            TimeSpan? timeout = null)
    {
        lock (_lock)
        {
            if (_pendingResponse != null)
                throw new InvalidOperationException("Command already in progress");

            _pendingResponse = new TaskCompletionSource<string[]>(TaskCreationOptions.RunContinuationsAsynchronously);
            _responseBuffer.Clear();
        }

        // ensure termination
        var cmdBytes = Encoding.UTF8.GetBytes($"{command}\r\n");
        _serial.Write(cmdBytes);

        using var timeoutCts = new CancellationTokenSource(timeout ?? _defaultTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);

        using var reg = linkedCts.Token.Register(() =>
        {
            lock (_lock)
            {
                if (_pendingResponse != null && !_pendingResponse.Task.IsCompleted)
                {
                    if (timeoutCts.IsCancellationRequested)
                    {
                        _pendingResponse.TrySetException(new TimeoutException());
                    }
                    else
                    {
                        _pendingResponse.TrySetCanceled(cancellationToken);
                    }
                }
            }
        });

        var r = await _pendingResponse.Task.ConfigureAwait(false);
        var lastLine = r[^1];
        return new AtResponse
        {
            Lines = r,
            // Success if: ends with "OK" OR ends with a data response line starting with "+"
            IsSuccess = lastLine.Equals("OK", StringComparison.OrdinalIgnoreCase) || lastLine.StartsWith("+"),
            ErrorMessage = lastLine.StartsWith("ERR", StringComparison.OrdinalIgnoreCase) ? lastLine : null
        };
    }

    private void SerialMessageReceived(object sender, SerialMessageData e)
    {
        lock (_lock)
        {
            var line = e.GetMessageString(System.Text.Encoding.UTF8);

            if (_pendingResponse == null)
            {
                // unsolicited (join events, etc)
                UnsolicitedMessage?.Invoke(this, line);
            }
            else
            {
                // normal line collection
                _responseBuffer.Add(line);

                if (line == "OK" || line.StartsWith("ERR") || line.StartsWith("+"))
                {
                    _pendingResponse.TrySetResult(_responseBuffer.ToArray());
                    _pendingResponse = null;
                }
            }
        }
    }

    public void Dispose()
    {
        _serial.MessageReceived -= SerialMessageReceived;
    }
}


