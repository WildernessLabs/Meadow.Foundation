using Meadow.Hardware;
using System.Linq;

namespace Meadow.Foundation.IOExpanders;

public abstract partial class T3xxx
{
    /// <summary>
    /// Represents a digital output port on a Temco Controls T3 module.
    /// Provides functionality to control digital output states (on/off) on output channels.
    /// </summary>
    public class DigitalOutputPort : IDigitalOutputPort
    {
        private readonly ushort _outputRegister;
        private readonly T3xxx _module;
        private bool? _lastSetState = null;

        /// <summary>
        /// Gets the initial state that was set when the port was created.
        /// </summary>
        /// <value>The boolean state that was applied during port initialization.</value>
        public bool InitialState { get; }

        /// <summary>
        /// Gets the digital channel information for this output port.
        /// </summary>
        /// <value>The IDigitalChannelInfo describing the capabilities of this digital output.</value>
        public IDigitalChannelInfo Channel { get; }

        /// <summary>
        /// Gets the pin associated with this digital output port.
        /// </summary>
        /// <value>The IPin representing the physical connection for this digital output.</value>
        public IPin Pin { get; }

        /// <summary>
        /// Initializes a new instance of the DigitalOutputPort class and configures the output channel.
        /// Sets the channel to manual mode and configures it for on/off operation.
        /// </summary>
        /// <param name="module">The parent T3xxx module that owns this port.</param>
        /// <param name="pin">The pin associated with this digital output port.</param>
        /// <param name="initialState">The initial state to set on the digital output.</param>
        /// <param name="autoManualRegister">The register address for setting manual/auto mode.</param>
        /// <param name="rangeRegister">The register address for configuring the output range.</param>
        /// <param name="outputRegister">The register address for setting the output state.</param>
        internal DigitalOutputPort(
            T3xxx module,
            IPin pin,
            bool initialState,
            ushort autoManualRegister,
            ushort rangeRegister,
            ushort outputRegister)
        {
            _module = module;
            Pin = pin;
            _outputRegister = outputRegister;

            Channel = (IDigitalChannelInfo)pin.SupportedChannels.First(c => c is IDigitalChannelInfo);

            // range set to on/off
            _module.WriteHoldingRegister(rangeRegister, 1).GetAwaiter().GetResult();

            // mode must be set to 1 (manual)
            _module.WriteHoldingRegister(autoManualRegister, 1).GetAwaiter().GetResult();

            State = InitialState = initialState;
        }

        /// <summary>
        /// Gets or sets the current state of the digital output port.
        /// Setting the state immediately writes the value to the module's output register.
        /// Getting the state returns the last set value or reads from the module if no value has been set.
        /// </summary>
        /// <value>True for high/on state, false for low/off state.</value>
        public bool State
        {
            set
            {
                _module.WriteHoldingRegister(_outputRegister, (ushort)(value ? 1 : 0)).GetAwaiter().GetResult();
                _lastSetState = value;
            }
            get
            {
                if (_lastSetState == null)
                {
                    _lastSetState = _module.ReadHoldingRegister(_outputRegister).GetAwaiter().GetResult() != 0;
                }
                return _lastSetState.Value;
            }
        }

        /// <summary>
        /// Releases any resources used by the digital output port.
        /// </summary>
        public void Dispose()
        {
        }
    }
}