namespace Meadow.Foundation.Transceivers.Waveshare;

public partial class Sx1303
{
    /// <summary>
    /// SX130x chip model as reported by OTP memory.
    /// </summary>
    public enum ChipModel : byte
    {
        /// <summary>SX1302 concentrator.</summary>
        Sx1302  = 0x02,
        /// <summary>SX1303 concentrator.</summary>
        Sx1303  = 0x03,
        /// <summary>Model could not be determined (OTP not accessible or unrecognized value).</summary>
        Unknown = 0xFF,
    }
}
