using System.Collections.Generic;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders.Ports
{
    public interface IMcpGpioPorts : IPinDefinitions, IReadOnlyList<IMcpGpioPort>
    {
        int GetPortIndex(IMcpGpioPort port);

        int GetPortIndexOfPin(IPin pin);

        IMcpGpioPort GetPortOfPin(IPin pin);
    }
}
