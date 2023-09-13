using System;

namespace Meadow.Foundation.ICs.IOExpanders;

internal interface IFt232Bus
{
    public IntPtr Handle { get; }
    public byte GpioDirectionMask { get; set; }
    public byte GpioState { get; set; }
}