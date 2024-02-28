using Meadow.Hardware;
using Peak.Can.Basic.BackwardCompatibility;

namespace ICs.IOExpanders.PCanBasic;

public class PCanConfiguration : ICanBusConfiguration
{
    public ushort BusHandle { get; set; } = PCANBasic.PCAN_USBBUS1;
    public bool IsFD { get; set; } = false;
    public CanBusBitrate Bitrate { get; set; } = CanBusBitrate.Baud_250k;
}
