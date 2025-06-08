using Meadow.Foundation.Sensors;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Flow;
using Meadow.Units;
using System;
using System.Threading;

namespace Sensors.Flow.HallEffect.Simulation;

public class SimulatedHallEffectFlowSensor : HallEffectFlowSensor, ISimulatedSensor<VolumetricFlow>
{
    private readonly SimulatedDigitalSignalAnalyzer analyzer = new SimulatedDigitalSignalAnalyzer(Frequency.Zero);

    public VolumetricFlow Flow { get; private set; }

    public SimulationBehavior[] SupportedBehaviors =>
        [
        SimulationBehavior.Sawtooth,
        SimulationBehavior.RandomWalk,
        SimulationBehavior.Sine
        ];

    public Type ValueType => typeof(VolumetricFlow);

    public SimulatedHallEffectFlowSensor()
        : this(new SimulatedDigitalSignalAnalyzer(Frequency.Zero))
    {
        Flow = VolumetricFlow.Zero;
    }

    private SimulatedHallEffectFlowSensor(SimulatedDigitalSignalAnalyzer analyzer)
        : base(analyzer, 10d) // 10Hz/L/min
    {
        this.analyzer = analyzer;
    }

    public void SetSensorValue(object value)
    {
        if (value is VolumetricFlow flow)
        {
            Flow = flow;
        }
        else
        {
            throw new ArgumentException("Value must be a VolumetricFlow");
        }
    }

    public VolumetricFlow MinimumSimulatedValue { get; set; } = VolumetricFlow.Zero;
    public VolumetricFlow MaximumSimulatedValue { get; set; } = new VolumetricFlow(10, VolumetricFlow.UnitType.GallonsPerMinute);

    private Timer? _simulationTimer;
    private SimulationBehavior _currentBehavior = SimulationBehavior.None;
    private double _simulationStep = 0;
    private readonly Random _random = new Random();

    public void StartSimulation(SimulationBehavior behavior)
    {
        _currentBehavior = behavior;

        if (_simulationTimer == null)
        {
            TimerCallback? timerProc = behavior switch
            {
                SimulationBehavior.Sawtooth => SawtoothTimerProc,
                SimulationBehavior.RandomWalk => RandomTimerProc,
                SimulationBehavior.Sine => SineTimerProc,
                SimulationBehavior.None => null,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (timerProc != null)
            {
                _simulationTimer = new Timer(timerProc, null, 0, 1000);
            }
        }
    }

    private double CalculateFrequencyFromFlow(VolumetricFlow flow)
    {
        // Using the formula: F = (S * Q - O)
        // Where S = 10 (from the base constructor), O = 0 (default offset)
        double litersPerMinute = flow.LitersPerMinute;
        return Scale * litersPerMinute;
    }

    private void RandomTimerProc(object _)
    {
        // Generate a random walk by adding/subtracting a small random value
        double minValue = MinimumSimulatedValue.LitersPerMinute;
        double maxValue = MaximumSimulatedValue.LitersPerMinute;
        double range = maxValue - minValue;

        // Get current flow value
        VolumetricFlow currentFlow = Flow;

        // Generate a random change (between -10% and +10% of the total range)
        double change = ((_random.NextDouble() * 0.2) - 0.1) * range;

        // Calculate new value
        double newValue = currentFlow.LitersPerMinute + change;

        // Ensure value stays within bounds
        newValue = Math.Max(minValue, Math.Min(newValue, maxValue));

        // Update the flow value by setting the appropriate frequency on the analyzer
        VolumetricFlow newFlow = new VolumetricFlow(newValue, VolumetricFlow.UnitType.LitersPerMinute);
        double frequencyHz = CalculateFrequencyFromFlow(newFlow);
        analyzer.SetFrequency(new Frequency(frequencyHz, Frequency.UnitType.Hertz));

        // Update the cached Flow property
        Flow = newFlow;
    }

    private void SawtoothTimerProc(object _)
    {
        // Sawtooth pattern - gradually increases from min to max, then resets to min
        double minValue = MinimumSimulatedValue.LitersPerMinute;
        double maxValue = MaximumSimulatedValue.LitersPerMinute;
        double range = maxValue - minValue;

        // Increment step (0.1 means it takes 10 steps to go from min to max)
        _simulationStep += 0.1;

        // Reset when we reach 1.0
        if (_simulationStep >= 1.0)
        {
            _simulationStep = 0.0;
        }

        // Calculate new value based on step
        double newValue = minValue + (_simulationStep * range);

        // Update by setting the frequency on the analyzer
        VolumetricFlow newFlow = new VolumetricFlow(newValue, VolumetricFlow.UnitType.LitersPerMinute);
        double frequencyHz = CalculateFrequencyFromFlow(newFlow);
        analyzer.SetFrequency(new Frequency(frequencyHz, Frequency.UnitType.Hertz));

        // Update the cached Flow property
        Flow = newFlow;
    }

    private void SineTimerProc(object _)
    {
        // Sine wave pattern - smooth oscillation between min and max
        double minValue = MinimumSimulatedValue.LitersPerMinute;
        double maxValue = MaximumSimulatedValue.LitersPerMinute;
        double range = maxValue - minValue;
        double midPoint = minValue + (range / 2);
        double amplitude = range / 2;

        // Increment step (0.05 means it takes about 20 steps to complete one cycle)
        _simulationStep += 0.05;

        // Calculate new value using sine function
        double newValue = midPoint + (amplitude * Math.Sin(_simulationStep * 2 * Math.PI));

        // Update by setting the frequency on the analyzer
        VolumetricFlow newFlow = new VolumetricFlow(newValue, VolumetricFlow.UnitType.LitersPerMinute);
        double frequencyHz = CalculateFrequencyFromFlow(newFlow);
        analyzer.SetFrequency(new Frequency(frequencyHz, Frequency.UnitType.Hertz));

        // Update the cached Flow property
        Flow = newFlow;
    }
}
