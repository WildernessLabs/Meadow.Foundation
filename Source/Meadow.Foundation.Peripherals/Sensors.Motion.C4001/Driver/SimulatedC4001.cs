using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;
using static Meadow.Foundation.Sensors.Motion.C4001;

namespace Meadow.Foundation.Sensors.Motion;

/// <summary>
/// Create a new simulated C4001 object
/// </summary>
public class SimulatedC4001 : IC4001, IDisposable
{
    private SensorStatus status = new();
    private byte targetNumber = 1;
    private Speed speed = new(0, Speed.UnitType.MetersPerSecond);
    private Length range = new(0, Length.UnitType.Meters);
    private uint energy;
    private bool motion;

    private CancellationTokenSource? simulationCTS;
    private Task? simulationTask;
    private readonly Random random = new();

    /// <inheritdoc/>
    public bool SetSensorMode(SensorMode mode)
    {
        return true;
    }

    /// <inheritdoc/>
    public SensorStatus GetStatus() => status;

    /// <inheritdoc/>
    public byte GetTargetNumber() => motion ? targetNumber : (byte)0;

    /// <inheritdoc/>
    public Speed GetTargetSpeed() { return speed; }

    /// <inheritdoc/>
    public Length GetTargetRange() { return range; }

    /// <inheritdoc/>
    public uint GetTargetEnergy() { return energy; }

    /// <inheritdoc/>
    public bool IsMotionDetected() { return motion; }

    /// <summary>
    /// Parameters that control the approach-and-depart cycle.
    /// </summary>
    public sealed class SimulationOptions
    {
        /// <summary>Shortest distance at closest approach.</summary>
        public double MinRangeMeters { get; set; } = 1.5;

        /// <summary>Farthest distance at the ends of the path.</summary>
        public double MaxRangeMeters { get; set; } = 12.0;

        /// <summary>Update frequency in Hz.</summary>
        public double UpdateHz { get; set; } = 2;
    }

    /// <summary>
    /// Start the approach/away simulation. Calling again restarts it with new options.
    /// </summary>
    public void StartSimulation(SimulationOptions? options = null)
    {
        options ??= new SimulationOptions();
        StopSimulation();

        simulationCTS = new CancellationTokenSource();
        var ct = simulationCTS.Token;

        status = new SensorStatus { InitStatus = 1, WorkStatus = 1, WorkMode = 0 };
        targetNumber = 1;
        range = new Length(options.MaxRangeMeters + 0.5, Length.UnitType.Meters);
        speed = new Speed(0, Speed.UnitType.MetersPerSecond);
        energy = 0;

        simulationTask = Task.Run(async () =>
        {
            while (ct.IsCancellationRequested == false)
            {
                if (range.Meters > 40)
                {
                    range = new Length(options.MaxRangeMeters + 1, Length.UnitType.Meters);
                }

                var rand = random.Next(50);

                range = new Length(range.Centimeters + random.Next(80) - 40, Length.UnitType.Centimeters);
                energy = (uint)rand * 1000;

                await Task.Delay((int)(1000 / options.UpdateHz));

                if (range.Meters < options.MaxRangeMeters)
                {
                    motion = true;
                }
                else
                {
                    motion = false;
                }
            }

        }, ct);
    }

    /// <summary>Stop the simulation and freeze values at their last state.</summary>
    public void StopSimulation()
    {
        try
        {
            simulationCTS?.Cancel();
            simulationTask?.Wait(50);
        }
        catch { /* swallow */ }
        finally
        {
            simulationCTS?.Dispose();
            simulationCTS = null;
            simulationTask = null;
            status = new SensorStatus();
        }
    }

    /// <summary>Manually set simulated status.</summary>
    public void SetStatus(SensorStatus status)
    {
        this.status = status;
    }

    /// <summary>Manually set target properties for deterministic tests.</summary>
    public void SetTarget(byte number, Speed speed, Length range, uint energy)
    {
        targetNumber = number;
        this.speed = speed;
        this.range = range;
        this.energy = energy;
    }

    /// <inheritdoc/>
    public void Dispose() => StopSimulation();

    /// <inheritdoc/>
    public bool SetDetectionRange(ushort min, ushort max, ushort trig) => true;

    /// <inheritdoc/>
    public bool SetTrigSensitivity(byte sensitivity) => true;

    /// <inheritdoc/>
    public bool SetKeepSensitivity(byte sensitivity) => true;

    /// <inheritdoc/>
    public bool SetDelay(byte trig, ushort keep) => true;

    /// <inheritdoc/>
    public ushort GetKeepTimeout() => 4;
}