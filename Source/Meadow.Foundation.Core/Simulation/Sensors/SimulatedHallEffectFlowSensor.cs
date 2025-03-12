using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors;

/// <summary>
/// A simulated hall-effect flow sensor that provides volumetric flow readings
/// without requiring actual hardware.
/// </summary>
/// <remarks>
/// This class implements <see cref="IVolumetricFlowSensor"/> and uses a
/// <see cref="SimulatedDigitalSignalAnalyzer"/> to simulate signal pulses
/// that correlate to volumetric flow measurements.
/// </remarks>
public class SimulatedHallEffectFlowSensor : IVolumetricFlowSensor
{
    private SimulatedDigitalSignalAnalyzer _analyzer;
    private double _flowCoefficient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulatedHallEffectFlowSensor"/> class
    /// with a specified pulse frequency and flow coefficient.
    /// </summary>
    /// <param name="simulatedPulseFrequency">
    /// The frequency of simulated pulses that represent flow.
    /// </param>
    /// <param name="flowCoefficient">
    /// A numeric factor used to convert pulses per unit time into 
    /// <see cref="VolumetricFlow"/> (defaults to 80).
    /// </param>
    public SimulatedHallEffectFlowSensor(Frequency simulatedPulseFrequency, double flowCoefficient = 80d)
    {
        _flowCoefficient = flowCoefficient;
        _analyzer = new SimulatedDigitalSignalAnalyzer(simulatedPulseFrequency);
    }

    /// <summary>
    /// Sets the frequency of the simulated signal, effectively changing the 
    /// flow rate in the simulation.
    /// </summary>
    /// <param name="frequency">The new simulated pulse <see cref="Frequency"/>.</param>
    public void SetSignalFrequency(Frequency frequency)
    {
        _analyzer.SetFrequency(frequency);
    }

    /// <summary>
    /// Gets the current flow measurement.
    /// </summary>
    /// <remarks>
    /// The flow is calculated as the current simulated frequency divided by
    /// the flow coefficient, expressed in liters per minute.
    /// </remarks>
    public VolumetricFlow Flow => new VolumetricFlow(
        _analyzer.GetFrequency().Hertz / _flowCoefficient,
        VolumetricFlow.UnitType.LitersPerMinute);

    /// <summary>
    /// Reads the simulated flow value asynchronously.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> whose result is the current 
    /// <see cref="VolumetricFlow"/> reading.
    /// </returns>
    public Task<VolumetricFlow> Read()
    {
        return Task.FromResult(Flow);
    }
}
