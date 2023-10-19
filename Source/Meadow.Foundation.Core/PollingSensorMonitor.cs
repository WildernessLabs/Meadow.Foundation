using Meadow;
using Meadow.Foundation;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class PollingSensorMonitor<UNIT> : ISensorMonitor
    where UNIT : struct
{
    public event EventHandler<object> SampleAvailable = default!;

    private PollingSensorBase<UNIT> _sensor;
    private CancellationTokenSource _cancellationSource;
    private bool _isSampling;

    public PollingSensorMonitor(PollingSensorBase<UNIT> sensor, CancellationTokenSource? cancellationTokenSource = null)
    {
        _sensor = sensor;
        _cancellationSource = cancellationTokenSource ?? new CancellationTokenSource();
    }

    public void StartSampling(ISamplingSensor sensor)
    {
        if (_isSampling) return;

        CancellationToken ct = _cancellationSource.Token;
        var t = new Task(async () =>
        {
            _isSampling = true;
            while (true)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    var c = await _sensor.Read();
                    SampleAvailable?.Invoke(_sensor, c);
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error($"Unhandled exception reading sensor type {sensor.GetType().Name}: {ex.Message}");
                }

                if (ct.IsCancellationRequested) break;

                await Task.Delay(sensor.UpdateInterval);
            }
            _isSampling = false;
        }, _cancellationSource.Token, TaskCreationOptions.LongRunning);
        t.Start();
    }

    public void StopSampling(ISamplingSensor sensor)
    {
        _cancellationSource.Cancel();
    }
}
