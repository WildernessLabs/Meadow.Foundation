using Meadow.Hardware;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

/// <summary>
/// A simulated version of the driver for Temco Controls T322ai analog input module.
/// </summary>
public class SimulatedT322ai
    : IT322ai
{
    /// <inheritdoc/>
    public T322ai.PinDefinitions Pins { get; }

    /// <summary>
    /// Initializes a new instance of the T322ai simulator
    /// </summary>
    public SimulatedT322ai()
    {
        Pins = new T322ai.PinDefinitions(this);
    }

    /// <inheritdoc/>
    public ICurrentInputPort CreateCurrentInputPort(IPin pin)
    {
        return new SimulatedCurrentInputPort(this, pin);
    }

    /// <inheritdoc/>
    public IVoltageInputPort CreateVoltageInputPort(IPin pin)
    {
        return new SimulatedVoltageInputPort(this, pin);
    }

    /// <inheritdoc/>
    public Task<int> ReadSerialNumber()
    {
        return Task.FromResult(12345678);
    }

    /// <inheritdoc/>
    public Task<float> ReadFirmwareVersion()
    {
        return Task.FromResult(1.0f);
    }

    /// <inheritdoc/>
    public Task<T3ModuleModel> ReadModel()
    {
        return Task.FromResult(T3ModuleModel.SimulatedT322ai);
    }

    /// <inheritdoc/>
    public Task<byte> ReadHardwareRevision()
    {
        return Task.FromResult((byte)1);
    }
}
