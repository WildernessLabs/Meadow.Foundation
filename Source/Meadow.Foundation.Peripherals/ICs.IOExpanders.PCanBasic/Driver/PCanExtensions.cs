using Meadow.Hardware;
using Peak.Can.Basic.BackwardCompatibility;

namespace ICs.IOExpanders.PCanBasic;

internal static class PCanExtensions
{
    public static TPCANBaudrate ToPCANBaudrate(this CanBusBitrate bitrate)
    {
        return bitrate switch
        {
            CanBusBitrate.Baud_1M => TPCANBaudrate.PCAN_BAUD_1M,
            CanBusBitrate.Baud_800k => TPCANBaudrate.PCAN_BAUD_800K,
            CanBusBitrate.Baud_500k => TPCANBaudrate.PCAN_BAUD_500K,
            CanBusBitrate.Baud_250k => TPCANBaudrate.PCAN_BAUD_250K,
            _ => throw new ArgumentException()
        };
    }
}
