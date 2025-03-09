using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Represents the base logic for a simulated sample sensor
/// </summary>
public abstract class SimulatedSamplingSensorBase<TUNIT> : SimulatedSensorBase<TUNIT>, ISamplingSensor<TUNIT>
    where TUNIT : struct, IComparable
{
    /// <inheritdoc/>
    public event EventHandler<IChangeResult<TUNIT>>? Updated;

    private Timer updateTimer;

    /// <inheritdoc/>
    public TimeSpan UpdateInterval { get; private set; }
    /// <inheritdoc/>
    public bool IsSampling { get; private set; }
    /// <inheritdoc/>
    protected TUNIT? PreviousReading { get; private set; }

    /// <summary>
    /// Generates a value based on the provided behavior
    /// </summary>
    /// <param name="behavior">The behavior to use when generating a value</param>
    protected abstract TUNIT GenerateSimulatedValue(SimulationBehavior behavior);

    /// <summary>
    /// Called from derived classes
    /// </summary>
    protected SimulatedSamplingSensorBase(
        TUNIT initialCondition,
        TUNIT minimumCondition,
        TUNIT maximumCondition,
        IDigitalInterruptPort incrementPort,
        IDigitalInterruptPort decrementPort)
        : base(initialCondition, minimumCondition, maximumCondition, incrementPort, decrementPort)
    {
        UpdateInterval = TimeSpan.FromSeconds(5);
        updateTimer = new Timer(UpdateTimerProc, null, -1, -1);
    }

    private async void UpdateTimerProc(object _)
    {
        var newVal = await Read();

        Updated?.Invoke(this, new ChangeResult<TUNIT>(newVal, PreviousReading));
        PreviousReading = newVal;

        updateTimer.Change(UpdateInterval, TimeSpan.FromMilliseconds(-1));
    }

    /// <inheritdoc/>
    public Task<TUNIT> Read()
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
