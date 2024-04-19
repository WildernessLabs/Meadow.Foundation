using Meadow.Peripherals.Sensors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors;

public abstract class SimulatedSamplingSensorBase<UNIT> : ISimulatedSensor, ISamplingSensor<UNIT>
    where UNIT : struct
{
    public event EventHandler<IChangeResult<UNIT>>? Updated;

    private Timer updateTimer;
    private SimulationBehavior simulationBehavior;

    public virtual SimulationBehavior[] SupportedBehaviors => new[] { SimulationBehavior.None };
    public abstract Type ValueType { get; }
    public TimeSpan UpdateInterval { get; private set; }
    public bool IsSampling { get; private set; }
    protected UNIT? PreviousReading { get; private set; }

    protected abstract UNIT GenerateSimulatedValue(SimulationBehavior behavior);
    public abstract void SetSensorValue(object value);


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

    public Task<UNIT> Read()
    {
        return Task.FromResult(GenerateSimulatedValue(simulationBehavior));
    }

    public virtual void StartSimulation(SimulationBehavior behavior)
    {
        simulationBehavior = behavior;
    }

    public void StartUpdating(TimeSpan? updateInterval = null)
    {
        IsSampling = true;

        if (updateInterval != null)
        {
            UpdateInterval = updateInterval.Value;
        }

        updateTimer.Change(UpdateInterval, TimeSpan.FromMilliseconds(-1));
    }

    public void StopUpdating()
    {
        IsSampling = false;
        updateTimer.Change(TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
    }
}
