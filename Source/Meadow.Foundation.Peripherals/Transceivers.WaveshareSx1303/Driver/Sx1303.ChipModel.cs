namespace Meadow.Foundation.Transceivers.Waveshare;

public partial class Sx1303
{
    public enum ChipModel : byte
    {
        Sx1302  = 0x02, // SX1302 (may also read as 0x00 on some revisions)
        Sx1303  = 0x03,
        Unknown = 0xFF,
    }
}
