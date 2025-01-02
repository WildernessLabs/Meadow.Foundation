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
/// </remarks>
public abstract class HallEffectFlowSensor : PollingSensorBase<VolumetricFlow>, IVolumetricFlowSensor
{
    private IDigitalSignalAnalyzer analyzer;
    private double flowCoefficient;

    /// <summary>
    /// Initializes a new instance of a Hall effect flow sensor.
    /// </summary>
    /// <param name="pin">The digital input pin connected to the sensor's signal line.</param>
    /// <param name="flowCoefficient">The calibration coefficient in Hz per L/min used to convert frequency to flow rate.</param>
    protected HallEffectFlowSensor(IPin pin, double flowCoefficient)
    {
        this.flowCoefficient = flowCoefficient;
        analyzer = pin.CreateDigitalSignalAnalyzer(false);
    }

    /// <inheritdoc/>
    public VolumetricFlow Flow => GetInstantaneousFlowRate();

    /// <inheritdoc/>
    protected override Task<VolumetricFlow> ReadSensor()
    {
        return Task.FromResult(GetInstantaneousFlowRate());
    }

    private VolumetricFlow GetInstantaneousFlowRate()
    {
        return new VolumetricFlow(
            analyzer.GetFrequency().Hertz / flowCoefficient,
             VolumetricFlow.UnitType.LitersPerMinute);
    }
}