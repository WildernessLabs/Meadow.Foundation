using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Telematics.OBD2;

public class Obd2Client
{
    private ICanBus _canBus;

    public Obd2Client(ICanBus canBus)
    {
        _canBus = canBus;

        _canBus.FrameReceived += OnFrameReceived;
    }

    public async Task RequestVehicleInfo()
    {
        // service 9 request - get all supported PIDs
        var request = new SaeStandardQueryFrame(Service.VehicleInfo, 0);

        // send and wait for a response
        var response = await SendAndWaitForResponse(request, TimeSpan.FromSeconds(5));

        response.Payload
    }

    public async Task<Obd2ResponseFrame> SendAndWaitForResponse(SaeStandardQueryFrame queryFrame, TimeSpan timeout)
    {
        var cancellationTokenSource = new CancellationTokenSource(timeout);
        var tcs = new TaskCompletionSource<Obd2ResponseFrame>();

        _canBus.WriteFrame(queryFrame);

        async Task CheckFrame()
        {
            try
            {
                var searchKey = ((int)queryFrame.Pid << 8) | (int)queryFrame.Service;
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (_received.ContainsKey(searchKey))
                    {
                        var match = _received[searchKey];
                        _received.Remove(searchKey);
                        tcs.TrySetResult(match);
                        return;
                    }
                }
                tcs.TrySetException(new TimeoutException($"Response not received within {timeout.TotalSeconds} seconds."));
            }
            catch (OperationCanceledException)
            {
                tcs.TrySetException(new TimeoutException($"Response not received within {timeout.TotalSeconds} seconds."));
            }
        }

        // Start the frame checking task
        _ = CheckFrame();

        return await tcs.Task;
    }

    private Dictionary<int, Obd2ResponseFrame> _received = new();

    private void OnFrameReceived(object sender, ICanFrame e)
    {
        if (e is StandardDataFrame sdf)
        {
            Resolver.Log.Info("SAE Response received");
            var f = new Obd2ResponseFrame(sdf);

            var key = ((int)f.Pid << 8) | (int)f.Service;

            _received.Add(key, f);
        }
    }
}

public class EcuListener
{
}
