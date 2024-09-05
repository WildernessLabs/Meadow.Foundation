using Meadow.Hardware;
using Peak.Can.Basic.BackwardCompatibility;

namespace ICs.IOExpanders.PCanBasic;

public class PCanUsb : ICanController
{
    public PCanUsb()
    {
        // TODO: only supported on Windows
        // TODO: check for PCANBasic DLL
    }

    /// <inheritdoc/>
    public ICanBus CreateCanBus(CanBitrate bitrate, int busNumber = 0)
    {
        var config = new PCanConfiguration
        {
            Bitrate = bitrate,
            BusHandle = (ushort)(PCANBasic.PCAN_USBBUS1 + busNumber)
        };

        if (bitrate == CanBitrate.Can_FD)
        {
            return new PCanFdBus(config);
        }
        else
        {
            return new PCanBus(config);
        }
    }
}
