namespace Meadow.Foundation.Sensors.Battery;

public partial class Max1704x
{
    internal enum Registers
    {
        VCell = 0x02,
        SoC = 0x04,
        Mode = 0x06,
        Version = 0x08,
        Config = 0x0C,
        Command = 0XFE
    }
}
