using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// A simulated digital signal analyzer that can track and report
/// a signal's frequency, duty cycle, and pulse count for testing purposes.
/// </summary>
/// <remarks>
/// This class implements <see cref="IDigitalSignalAnalyzer"/> and allows
/// developers to simulate signal measurements without physical hardware.
/// Useful for unit tests or offline development.
/// </remarks>
public class SimulatedDigitalSignalAnalyzer : IDigitalSignalAnalyzer
{
    private Frequency frequency;
    private double dutyCycle = 0.5;
    private ulong count;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedDigitalSignalAnalyzer"/> class
    /// with a specified initial frequency.
    /// </summary>
    /// <param name="frequency">
    /// The <see cref="Frequency"/> to initialize the analyzer with.
    /// </param>
    public SimulatedDigitalSignalAnalyzer(Frequency frequency)
    {
        this.frequency = frequency;
    }

    /// <summary>
    /// Sets the simulated duty cycle for the analyzer.
    /// </summary>
    /// <param name="dutyCycle">
    /// The duty cycle to simulate, typically between 0.0 and 1.0.
    /// </param>
    public void SetDutyCycle(double dutyCycle)
    {
        this.dutyCycle = dutyCycle;
    }

    /// <summary>
    /// Gets the current simulated duty cycle.
    /// </summary>
    /// <returns>
    /// A <see cref="double"/> representing the duty cycle (0.0 to 1.0).
    /// </returns>
    public double GetDutyCycle()
    {
        return dutyCycle;
    }

    /// <summary>
    /// Sets the simulated frequency for the analyzer.
    /// </summary>
    /// <param name="frequency">
    /// The <see cref="Frequency"/> value to simulate.
    /// </param>
    public void SetFrequency(Frequency frequency)
    {
        this.frequency = frequency;
    }

    /// <summary>
    /// Gets the current simulated frequency.
    /// </summary>
    /// <returns>
    /// The <see cref="Frequency"/> currently set in the simulation.
    /// </returns>
    public Frequency GetFrequency()
    {
        return frequency;
    }

    /// <summary>
    /// Gets the mean frequency of the simulated signal.
    /// </summary>
    /// <returns>
    /// The <see cref="Frequency"/> representing the average frequency.
    /// In this simulation, it returns the same frequency set by <see cref="SetFrequency"/>.
    /// </returns>
    public Frequency GetMeanFrequency()
    {
        return frequency;
    }

    /// <summary>
    /// Sets the pulse count (or edge count) for the simulated signal.
    /// </summary>
    /// <param name="count">
    /// The <see cref="ulong"/> value representing the total pulses counted.
    /// </param>
    public void SetCount(ulong count)
    {
        this.count = count;
    }

    /// <summary>
    /// Gets the total pulse count (or edge count) from the simulation.
    /// </summary>
    /// <returns>
    /// A <see cref="ulong"/> representing the total number of pulses.
    /// </returns>
    public ulong GetCount()
    {
        return count;
    }
}
