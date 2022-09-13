namespace Meadow.Foundation.ICs.FanControllers
{
    public partial class Emc2101
    {
        const byte MaxLutTemperature = 0x7F; //in celius - 7 bit value
        const byte MaxFanSpeed = 0x3F; //scaled to 100% - 6 bit value
    }
}