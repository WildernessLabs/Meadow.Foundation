using Meadow.Hardware;

namespace Meadow.Foundation.IOExpanders;

public abstract partial class T3xxx
{
}

public abstract partial class T3xxx
{
    /// <summary>
    /// Represents a digital input port on a Temco Controls T3 module.
    /// </summary>
    public class DigitalInputPort : IDigitalInputPort
    {
        private readonly T3xxx _module;
        private readonly ushort _stateRegister;

        internal DigitalInputPort(
            T3xxx module,
            IPin pin,
            ushort stateRegister)
        {
            _module = module;
            _stateRegister = stateRegister;
            Pin = pin;
        }

        /// <inheritdoc/>
        public bool State
        {
            get
            {
                var register = _module.ReadHoldingRegister(_stateRegister).GetAwaiter().GetResult();
                // electrically the register is 1 (high) when open, and drives to 0 when closed
                return register == 0;
            }
        }

        /// <inheritdoc/>
        public ResistorMode Resistor
        {
            get => ResistorMode.InternalPullUp;
            set { }
        }

        /// <inheritdoc/>
        public IDigitalChannelInfo Channel => throw new System.NotImplementedException();

        /// <inheritdoc/>
        public IPin Pin { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}