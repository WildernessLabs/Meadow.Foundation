using System.Collections.Generic;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders.Ports
{
    public interface IMcpGpioPorts : IPinDefinitions, IReadOnlyList<McpGpioPort>
    {
        int GetPortIndex(McpGpioPort port);

        int GetPortIndexOfPin(IPin pin);

        McpGpioPort GetPortOfPin(IPin pin);
    }
}
