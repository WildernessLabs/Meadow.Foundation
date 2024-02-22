using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    public abstract class SpiBus : ISpiBus
    {
        /// <inheritdoc/>
        public abstract Frequency[] SupportedSpeeds { get; }
        /// <inheritdoc/>
        public abstract SpiClockConfiguration Configuration { get; }

        internal abstract void Configure();

        /// <inheritdoc/>
        public abstract void Exchange(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow);
        /// <inheritdoc/>
        public abstract void Read(IDigitalOutputPort? chipSelect, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow);
        /// <inheritdoc/>
        public abstract void Write(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow);
    }
}