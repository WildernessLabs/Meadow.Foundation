namespace Meadow.Foundation.Sensors.Atmospheric;

public partial class Ahtx0
{
    /// <summary>
    /// Ahtx0 Commands
    /// </summary>
    internal enum Commands : byte
    {
        INITIALIZE = 0b111_0001,
        TRIGGER_MEAS = 0b1010_1100,
        SOFT_RESET = 0b1011_1010
    }
}
