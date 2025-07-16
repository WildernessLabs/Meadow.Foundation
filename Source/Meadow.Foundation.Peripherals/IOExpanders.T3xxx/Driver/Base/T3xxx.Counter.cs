using Meadow.Hardware;

namespace Meadow.Foundation.IOExpanders;

public abstract partial class T3xxx
{
    /// <summary>
    /// Represents a dry contact pulse counter on a Temco Controls T3 module.
    /// </summary>
    public class Counter : ICounter
    {
        private readonly T3xxx _module;
        private readonly ushort _countRegister;

        internal Counter(
            T3xxx module,
            IPin pin,
            ushort countRegister)
        {
            _module = module;
            _countRegister = countRegister;

            Reset(); // reset the counter to zero on creation
        }

        /// <inheritdoc/>
        public bool Enabled
        {
            get => true; // T3xxx counters are always enabled
            set { }
        }

        /// <inheritdoc/>
        public ulong Count => _module.ReadHoldingRegister(_countRegister).GetAwaiter().GetResult();

        /// <inheritdoc/>
        public void Reset()
        {
            _module.WriteHoldingRegister(_countRegister, 0).GetAwaiter().GetResult();
        }
    }
}
