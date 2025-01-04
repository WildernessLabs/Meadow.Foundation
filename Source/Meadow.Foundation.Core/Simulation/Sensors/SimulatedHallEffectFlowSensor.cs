using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors;

public class SimulatedHallEffectFlowSensor : IVolumetricFlowSensor
{
    private SimulatedDigitalSignalAnalyzer _analyzer;
    private double _flowCoefficient;

    public SimulatedHallEffectFlowSensor(Frequency simulatedPulseFrequency, double flowCoefficient = 80d)
    {
        _flowCoefficient = flowCoefficient;
        _analyzer = new SimulatedDigitalSignalAnalyzer(simulatedPulseFrequency);
    }

    public void SetSignalFrequency(Frequency frequency)
    {
        _analyzer.SetFrequency(frequency);
    }

    public VolumetricFlow Flow => new VolumetricFlow(
        _analyzer.GetFrequency().Hertz / _flowCoefficient,
        VolumetricFlow.UnitType.LitersPerMinute);

    public Task<VolumetricFlow> Read()
    {
        return Task.FromResult(Flow);
    }
}
