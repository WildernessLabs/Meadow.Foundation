namespace Meadow.Foundation.IOExpanders;

public abstract partial class T3xxx
{
    /// <summary>
    /// Defines the available analog input ranges for T3 modules.
    /// </summary>
    internal enum AnalogInputRange
    {
        Disabled = 0,

        /// <summary>
        /// 0-5 volt input range.
        /// </summary>
        Voltage_0_5 = 11,

        /// <summary>
        /// 4-20 milliamp current input range.
        /// </summary>
        Current_4_20 = 13,

        /// <summary>
        /// 0-10 volt input range.
        /// </summary>
        Voltage_0_10 = 19,

        pulseCountSlow = 15,
        PulseCountFast = 25,
    }
}
