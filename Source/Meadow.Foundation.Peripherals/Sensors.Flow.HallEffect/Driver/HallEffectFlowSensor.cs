using Meadow.Foundation;
using Meadow.Hardware;
using Meadow.Units;

using System.Threading.Tasks;

namespace Meadow.Peripherals.Sensors.Flow;

/// <summary>
/// Base class for Hall effect flow sensors that measure volumetric flow based on pulse frequency.
/// </summary>
/// <remarks>
/// Hall effect flow sensors typically output a frequency proportional to the flow rate.
/// Each sensor model has a specific calibration coefficient (Hz per L/min) used to convert
/// the measured frequency to a flow rate.
/// Sensors will have a flow formula in the form: 
/// F = (S * Q - O)
/// e.g. F = (7.5 * Q - 4)
/// where:
/// F = Frequency
/// Q = Flow
/// S = Scale
/// O = Offset
/// </remarks>
public abstract class HallEffectFlowSensor : PollingSensorBase<VolumetricFlow>, IVolumetricFlowSensor
{
    private readonly IDigitalSignalAnalyzer analyzer;
    private readonly double scale;
    private readonly double offset;

    /// <summary>
    /// Initializes a new instance of a Hall effect flow sensor.
    /// </summary>
    /// <param name="pin">The digital input pin connected to the sensor's signal line.</param>
    /// <param name="scale">The sensor scale factor (S) in Hz per L/min</param>
    /// <param name="offset">The sensor offset (O) in Hz</param>
    protected HallEffectFlowSensor(IPin pin, double scale, double offset = 0)
    {
        this.scale = scale;
        this.offset = offset;
        analyzer = pin.CreateDigitalSignalAnalyzer(false);
    }

    /// <inheritdoc/>
    public VolumetricFlow Flow => CalculateFlow(analyzer.GetFrequency(), scale, offset);

    /// <inheritdoc/>
    protected override Task<VolumetricFlow> ReadSensor()
    {
        return Task.FromResult(CalculateFlow(analyzer.GetFrequency(), scale, offset));
    }

    /// <summary>
    /// Calculates volumetric flow rate from frequency using the formula F = (S * Q - O), solving for Q.
    /// </summary>
    /// <param name="frequency">The measured frequency in Hz</param>
    /// <param name="scale">The scale factor (S) in Hz per L/min</param>
    /// <param name="offset">The offset (O) in Hz</param>
    /// <returns>The flow rate in cubic meters per second</returns>
    private VolumetricFlow CalculateFlow(Frequency frequency, double scale, double offset)
    {
        // First solve for Q (L/min): F = (S * Q - O)
        // F + O = S * Q
        // Q = (F + O) / S
        double litersPerMinute = (frequency.Hertz + offset) / scale;

        return new VolumetricFlow(litersPerMinute, VolumetricFlow.UnitType.LitersPerMinute);
    }
}