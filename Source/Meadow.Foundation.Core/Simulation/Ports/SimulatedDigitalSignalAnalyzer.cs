using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Sensors;

public class SimulatedDigitalSignalAnalyzer : IDigitalSignalAnalyzer
{
    private Frequency frequency;
    private double dutyCycle = 0.5;

    public SimulatedDigitalSignalAnalyzer(Frequency frequency)
    {
        this.frequency = frequency;
    }

    public void SetDutyCycle(double dutyCycle)
    {
        this.dutyCycle = dutyCycle;
    }

    public double GetDutyCycle()
    {
        return dutyCycle;
    }

    public void SetFrequency(Frequency frequency)
    {
        this.frequency = frequency;
    }

    public Frequency GetFrequency()
    {
        return frequency;
    }

    public Frequency GetMeanFrequency()
    {
        return frequency;
    }
}
