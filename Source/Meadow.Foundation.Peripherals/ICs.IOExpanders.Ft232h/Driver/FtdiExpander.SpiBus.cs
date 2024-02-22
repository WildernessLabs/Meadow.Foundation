using Meadow.Hardware;
using Meadow.Units;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public abstract partial class FtdiExpander
{
    public abstract class SpiBus : ISpiBus
    {
        public abstract Frequency[] SupportedSpeeds { get; }
        public abstract SpiClockConfiguration Configuration { get; }

        public abstract void Configure();
        public abstract void Exchange(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow);
        public abstract void Read(IDigitalOutputPort? chipSelect, Span<byte> readBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow);
        public abstract void Write(IDigitalOutputPort? chipSelect, Span<byte> writeBuffer, ChipSelectMode csMode = ChipSelectMode.ActiveLow);
    }
}