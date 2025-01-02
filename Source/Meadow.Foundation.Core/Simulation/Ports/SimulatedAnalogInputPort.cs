using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// Represents a simulated analog input port
/// </summary>
public class SimulatedAnalogInputPort : SimulatedSensorBase, IAnalogInputPort
{
    private Timer simulationTimer;
    private Voltage oldVoltage;
    private List<IObserver<IChangeResult<Voltage>>> observers = new();

    /// <inheritdoc/>
    public Voltage[] VoltageSampleBuffer { get; }

    /// <inheritdoc/>
    public Voltage ReferenceVoltage { get; set; }

    /// <inheritdoc/>
    public Voltage Voltage { get; private set; }

    /// <inheritdoc/>
    public TimeSpan UpdateInterval { get; private set; }

    /// <inheritdoc/>
    public int SampleCount => 3;

    /// <inheritdoc/>
    public TimeSpan SampleInterval { get; }

    /// <inheritdoc/>
    public IAnalogChannelInfo Channel { get; }

    /// <inheritdoc/>
    public IPin Pin { get; } = default!;

    /// <inheritdoc/>
    public override Type ValueType => typeof(double);

    /// <inheritdoc/>
    public event EventHandler<IChangeResult<Voltage>>? Updated;

    /// <summary>
    /// Creates a SimulatedAnalogInputPort instance
    /// </summary>
    public SimulatedAnalogInputPort()
    {
        VoltageSampleBuffer = new Voltage[SampleCount];
        SampleInterval = TimeSpan.FromMilliseconds(10);
        ReferenceVoltage = 3.3.Volts();
        Channel = new AnalogChannelInfo("SIM", 16, true, false);
        simulationTimer = new Timer(SimulationTimerProc, null, -1, -1);
    }

    private void SimulationTimerProc(object _)
    {
        for (var i = 0; i < VoltageSampleBuffer.Length; i++)
        {
            switch (SimulationBehavior)
            {
                case Peripherals.Sensors.SimulationBehavior.RandomWalk:
                    VoltageSampleBuffer[i] = new Voltage(GetRandomDouble(0, ReferenceVoltage.Volts), Voltage.UnitType.Volts);
                    break;
            }
            Thread.Sleep(SampleInterval);
        }

        simulationTimer.Change(UpdateInterval, TimeSpan.FromMilliseconds(-1));

        RaiseChangedAndNotify();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <inheritdoc/>
    public Task<Voltage> Read()
    {
        Voltage = new Voltage(VoltageSampleBuffer.Average(s => s.Volts), Voltage.UnitType.Volts);
        return Task.FromResult(Voltage);
    }

    /// <inheritdoc/>
    public void StartUpdating(TimeSpan? updateInterval = null)
    {
        if (updateInterval != null)
        {
            UpdateInterval = updateInterval.Value;
        }

        simulationTimer.Change(UpdateInterval, TimeSpan.FromMilliseconds(-1));
    }

    /// <inheritdoc/>
    public void StopUpdating()
    {
        simulationTimer.Change(-1, -1);
    }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<IChangeResult<Voltage>> observer)
    {
        lock (observers)
        {
            observers.Add(observer);
            return new Unsubscriber(observers, observer);
        }
    }

    /// <inheritdoc/>
    public override void SetSensorValue(object value)
    {
        for (var i = 0; i < VoltageSampleBuffer.Length; i++)
        {
            VoltageSampleBuffer[i] = (Voltage)value;
        }

        RaiseChangedAndNotify();
    }

    private void RaiseChangedAndNotify()
    {
        var newVoltage = Read().Result;
        var changeResult = new ChangeResult<Voltage>
        {
            New = newVoltage,
            Old = oldVoltage,
        };

        Updated?.Invoke(this, changeResult);

        lock (observers)
        {
            observers.ForEach(x => x.OnNext(changeResult));
        }

        oldVoltage = newVoltage;
    }

    private class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<IChangeResult<Voltage>>> _observers;
        private readonly IObserver<IChangeResult<Voltage>> _observer;

        public Unsubscriber(List<IObserver<IChangeResult<Voltage>>> observers, IObserver<IChangeResult<Voltage>> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (!(_observer == null)) _observers.Remove(_observer);
        }
    }
}
