namespace Meadow.Foundation.ICs.FanControllers
{
    public partial class Emc2101
    {
        const byte MaxLutTemperature = 0x7F; //in celcius - 7 bit value
        const byte MaxFanSpeed = 0x3F; //scaled to 100% - 6 bit value
        const double FanRpmNumerator = 5400000;
        const double TemperatureBit = 0.125;
    }
}
