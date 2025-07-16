using Meadow.Units;
using System.Threading.Tasks;

namespace Meadow.Foundation.CurrentLoop;

/// <summary>
/// Interface for transmitting current loop signals to emulate industrial sensors
/// </summary>
public interface ICurrentLoopGenerator
{
    /// <summary>
    /// Sets the output current for the transmitter
    /// </summary>
    /// <param name="current">The current to output</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SetOutputCurrent(Current current);

    /// <summary>
    /// Gets the last output current that was set
    /// </summary>
    /// <returns>The current that was last output by this sensor</returns>
    Current GetOutputCurrent();
}
