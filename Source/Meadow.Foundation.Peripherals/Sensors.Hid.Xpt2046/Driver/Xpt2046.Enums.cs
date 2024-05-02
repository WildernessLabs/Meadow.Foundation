using System;

namespace Meadow.Foundation.Sensors.Hid;

public partial class Xpt2046
{
    private enum Mode
    {
        Bits_12 = 0,
        Bits_8 = 8
    }

    private enum VoltageReference
    {
        SingleEnded = 0,
        Differential = 4
    }

    [Flags]
    private enum PowerState
    {
        PowerDown = 0,
        Adc = 1,
        Reference = 2,
    }
}