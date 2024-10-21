using System.Runtime.InteropServices;

namespace FTD2XX;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct FT_EEPROM_HEADER
{
    public int deviceType;       // FTxxxx device type to be programmed
                                 // Device descriptor options
    public short VendorId;             // 0x0403
    public short ProductId;                // 0x6001
    public byte SerNumEnable;           // non-zero if serial number to be used
                                        // Config descriptor options
    public short MaxPower;             // 0 < MaxPower <= 500
    public byte SelfPowered;            // 0 = bus powered, 1 = self powered
    public byte RemoteWakeup;           // 0 = not capable, 1 = capable
                                        // Hardware options
    public byte PullDownEnable;     // non-zero if pull down in suspend enabled
}
