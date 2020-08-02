using System;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders.Ports
{
    public interface IMcpGpioPort : IPinDefinitions
    {
        /// <summary>
        /// Raised when the value of a pin configured for input changes. Use in
        /// conjunction with parallel port reads via ReadFromPorts(). When using
        /// individual `DigitalInputPort` objects, each one will have their own
        /// `Changed` event
        /// </summary>
        event EventHandler<IOExpanderPortInputChangedEventArgs> InputChanged;

        IPin GP0 { get; }
        IPin GP1 { get; }
        IPin GP2 { get; }
        IPin GP3 { get; }
        IPin GP4 { get; }
        IPin GP5 { get; }
        IPin GP6 { get; }
        IPin GP7 { get; }

        /// <summary>
        /// Invoke the input changed event. Called from <see cref="Mcp23x" />.
        /// </summary>
        /// <param name="e"></param>
        void InvokeInputChanged(IOExpanderPortInputChangedEventArgs e);
    }
}
