using Meadow.Hardware;

namespace Meadow.Foundation.Sensors;

public class SimulatedDigitalOutputPort : IDigitalOutputPort
{
    private bool state;

    public bool InitialState { get; }
    public string Name { get; }

    public SimulatedDigitalOutputPort(string name, bool initialState)
    {
        Name = name;
        InitialState = state = initialState;
    }

    public virtual bool State
    {
        get => state;
        set
        {
            state = value;
            Resolver.Log.Info($"Output {Name} = {State}");
        }
    }

    public IDigitalChannelInfo? Channel => null;
    public IPin? Pin => null;

    public void Dispose()
    {
    }
}
