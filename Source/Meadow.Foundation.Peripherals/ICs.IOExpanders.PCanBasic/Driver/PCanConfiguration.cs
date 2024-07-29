using Meadow.Hardware;
using Peak.Can.Basic.BackwardCompatibility;

namespace ICs.IOExpanders.PCanBasic;

public class PCanConfiguration
{
    public ushort BusHandle { get; set; } = PCANBasic.PCAN_USBBUS1;
    public CanBitrate Bitrate { get; set; } = CanBitrate.Can_250kbps;
}
