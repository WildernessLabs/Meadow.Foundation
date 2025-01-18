namespace Meadow.Foundation.Sensors.Color;

public partial class Tcs3472x
{
    /// <summary>
    /// Valid I2C addresses for the sensor
    /// </summary>
    public enum Addresses : byte
    {
        /// <summary>
        /// Bus address 0x29
        /// </summary>
        Address_0x29 = 0x29,
        /// <summary>
        /// Default bus address
        /// </summary>
        Default = Address_0x29
    }

    /// <summary>
    /// The gain used to integrate the colors
    /// </summary>
    public enum Gain
    {
        /// <summary>1x gain</summary>
        Gain1x = 0x00,
        /// <summary>4x gain</summary>
        Gain4x = 0x01,
        /// <summary>16x gain</summary>
        Gain16x = 0x02,
        /// <summary>60x gain</summary>
        Gain60x = 0x03
    }

    /// <summary>
    /// This enum allows selecting how many cycles will be done measuring before
    /// raising an interrupt.
    /// </summary>
    public enum InterruptState
    {
        /// <summary>Every RGBC cycle generates an interrupt.</summary>
        All = 0x00,
        /// <summary>1 clear channel value outside of threshold range.</summary>
        Persistence01Cycle = 0x01,
        /// <summary>2 clear channel consecutive values out of range.</summary>
        Persistence02Cycle = 0x02,
        /// <summary>3 clear channel consecutive values out of range.</summary>
        Persistence03Cycle = 0x03,
        /// <summary>5 clear channel consecutive values out of range.</summary>
        Persistence05Cycle = 0x04,
        /// <summary>10 clear channel consecutive values out of range.</summary>
        Persistence10Cycle = 0x05,
        /// <summary>15 clear channel consecutive values out of range.</summary>
        Persistence15Cycle = 0x06,
        /// <summary>20 clear channel consecutive values out of range.</summary>
        Persistence20Cycle = 0x07,
        /// <summary>25 clear channel consecutive values out of range.</summary>
        Persistence25Cycle = 0x08,
        /// <summary>30 clear channel consecutive values out of range.</summary>
        Persistence30Cycle = 0x09,
        /// <summary>35 clear channel consecutive values out of range.</summary>
        Persistence35Cycle = 0x0A,
        /// <summary>40 clear channel consecutive values out of range.</summary>
        Persistence40Cycle = 0x0B,
        /// <summary>45 clear channel consecutive values out of range.</summary>
        Persistence45Cycle = 0x0C,
        /// <summary>50 clear channel consecutive values out of range.</summary>
        Persistence50Cycle = 0x0D,
        /// <summary>55 clear channel consecutive values out of range.</summary>
        Persistence55Cycle = 0x0E,
        /// <summary>60 clear channel consecutive values out of range.</summary>
        Persistence60Cycle = 0x0F
    }
}