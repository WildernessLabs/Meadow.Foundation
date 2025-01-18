using Meadow.Hardware;
using Peak.Can.Basic.BackwardCompatibility;

namespace Meadow.Foundation.ICs.CAN;

internal static class PCanExtensions
{
    internal static TPCANBaudrate ToPCANBaudrate(this CanBitrate bitrate)
    {
        return bitrate switch
        {
            CanBitrate.Can_1Mbps => TPCANBaudrate.PCAN_BAUD_1M,
            CanBitrate.Can_800kbps => TPCANBaudrate.PCAN_BAUD_800K,
            CanBitrate.Can_500kbps => TPCANBaudrate.PCAN_BAUD_500K,
            CanBitrate.Can_250kbps => TPCANBaudrate.PCAN_BAUD_250K,
            CanBitrate.Can_125kbps => TPCANBaudrate.PCAN_BAUD_125K,
            CanBitrate.Can_47kbps => TPCANBaudrate.PCAN_BAUD_47K,
            CanBitrate.Can_100kbps => TPCANBaudrate.PCAN_BAUD_100K,
            CanBitrate.Can_50kbps => TPCANBaudrate.PCAN_BAUD_50K,
            CanBitrate.Can_20kbps => TPCANBaudrate.PCAN_BAUD_20K,
            CanBitrate.Can_10kbps => TPCANBaudrate.PCAN_BAUD_10K,
            CanBitrate.Can_5kbps => TPCANBaudrate.PCAN_BAUD_5K,
            CanBitrate.Can_83kbps => TPCANBaudrate.PCAN_BAUD_83K,
            CanBitrate.Can_33kbps => TPCANBaudrate.PCAN_BAUD_33K,
            CanBitrate.Can_95kbps => TPCANBaudrate.PCAN_BAUD_95K,
            _ => throw new NotSupportedException()
        };
    }
}