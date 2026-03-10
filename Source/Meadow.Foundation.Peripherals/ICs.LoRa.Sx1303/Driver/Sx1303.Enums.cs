namespace Meadow.Foundation.ICs.LoRa;

public partial class Sx1303
{
    /// <summary>
    /// LoRa spreading factor
    /// </summary>
    public enum SpreadingFactor : byte
    {
        SF5  = 5,
        SF6  = 6,
        SF7  = 7,
        SF8  = 8,
        SF9  = 9,
        SF10 = 10,
        SF11 = 11,
        SF12 = 12,
    }

    /// <summary>
    /// LoRa signal bandwidth
    /// </summary>
    public enum Bandwidth : byte
    {
        /// <summary>125 kHz</summary>
        BW125 = 0x04,
        /// <summary>250 kHz</summary>
        BW250 = 0x05,
        /// <summary>500 kHz</summary>
        BW500 = 0x06,
    }

    /// <summary>
    /// LoRa coding rate
    /// </summary>
    public enum CodingRate : byte
    {
        CR4_5 = 1,
        CR4_6 = 2,
        CR4_7 = 3,
        CR4_8 = 4,
    }

    /// <summary>
    /// Regional frequency band / channel plan
    /// </summary>
    public enum LoRaBand
    {
        /// <summary>US 915 MHz band (902–928 MHz)</summary>
        US915,
        /// <summary>EU 868 MHz band (863–870 MHz)</summary>
        EU868,
        /// <summary>AU 915 MHz band (915–928 MHz)</summary>
        AU915,
        /// <summary>AS 923 MHz band</summary>
        AS923,
        /// <summary>IN 865 MHz band</summary>
        IN865,
    }

    /// <summary>
    /// Radio front-end selection (A or B, corresponding to the two SX1250 RF chips)
    /// </summary>
    public enum RadioPath : byte
    {
        RadioA = 0,
        RadioB = 1,
    }

    /// <summary>
    /// TX power class
    /// </summary>
    public enum TxPower : byte
    {
        /// <summary>Low power (+14 dBm typical)</summary>
        Low    = 0,
        /// <summary>Medium power (+20 dBm typical)</summary>
        Medium = 1,
        /// <summary>High power (+27 dBm typical)</summary>
        High   = 2,
    }
}
