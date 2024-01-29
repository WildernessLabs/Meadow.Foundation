namespace Meadow.Foundation.Sensors.Temperature;

public partial class Pct2075
{
    internal enum Registers
    {
        Temp = 0x00,
        Conf = 0x01,
        Thyst = 0x02,
        Tos = 0x03,
        Tidle = 0x04
    }
}