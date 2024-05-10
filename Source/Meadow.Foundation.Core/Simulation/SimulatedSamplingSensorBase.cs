using Meadow.Peripherals.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Represents the base logic for a simulated sample sensor
/// </summary>
public abstract class SimulatedSamplingSensorBase<UNIT> : SimulatedSensorBase, ISamplingSensor<UNIT>
    where UNIT : struct
{
    /// <inheritdoc/>
    public event EventHandler<IChangeResult<UNIT>>? Updated;

    private Timer updateTimer;

    /// <inheritdoc/>
    public TimeSpan UpdateInterval { get; private set; }
    /// <inheritdoc/>
    public bool IsSampling { get; private set; }
    /// <inheritdoc/>
    protected UNIT? PreviousReading { get; private set; }

    /// <summary>
    /// Generates a value based on the provided behavior
    /// </summary>
    /// <param name="behavior">The behavior to use when generating a value</param>
    protected abstract UNIT GenerateSimulatedValue(SimulationBehavior behavior);

    /// <summary>
    /// Called from derived classes
    /// </summary>
    protected SimulatedSamplingSensorBase()
    {
        UpdateInterval = TimeSpan.FromSeconds(5);
        updateTimer = new Timer(UpdateTimerProc, null, -1, -1);
    }

    private async void UpdateTimerProc(object _)
    {
        var newVal = await Read();

        Updated?.Invoke(this, new ChangeResult<UNIT>(newVal, PreviousReading));
        PreviousReading = newVal;

        updateTimer.Change(UpdateInterval, TimeSpan.FromMilliseconds(-1));
    }

    /// <inheritdoc/>
    public Task<UNIT> Read()
    {
        return Task.FromResult(GenerateSimulatedValue(SimulationBehavior));
    }

    /// <inheritdoc/>
    public void StartUpdating(TimeSpan? updateInterval = null)
    {
        IsSampling = true;

        if (updateInterval != null)
        {
            UpdateInterval = updateInterval.Value;
        }

        updateTimer.Change(UpdateInterval, TimeSpan.FromMilliseconds(-1));
    }

    /// <inheritdoc/>
    public void StopUpdating()
    {
        IsSampling = false;
        updateTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
    }
}
