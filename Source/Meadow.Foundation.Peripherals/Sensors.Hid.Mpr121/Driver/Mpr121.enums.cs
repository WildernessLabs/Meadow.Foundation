using System;
namespace Meadow.Foundation.Sensors.Hid
{
    public partial class Mpr121
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            /// <summary>
            /// Bus address 0x5A
            /// </summary>
            Address_0x5A = 0x5A,
            /// <summary>
            /// Default bus address
            /// </summary>
            Default = Address_0x5A
        }

        /// <summary>
        /// Mpr121 channels
        /// </summary>
        public enum Channels
        {
            Channel00 = 0,
            Channel01,
            Channel02,
            Channel03,
            Channel04,
            Channel05,
            Channel06,
            Channel07,
            Channel08,
            Channel09,
            Channel10,
            Channel11
        }

        public class Mpr121Configuration
        {
            /// <summary>
            /// Determines the largest magnitude of variation to pass through the baseline filter (rising).
            /// </summary>
            public byte MaxHalfDeltaRising { get; set; }

            /// <summary>
            /// Determines the incremental change when non-noise drift is detected (rising).
            /// </summary>
            public byte NoiseHalfDeltaRising { get; set; }

            /// <summary>
            /// Determines the number of samples consecutively greater than the Max Half Delta value (rising).
            /// This is necessary to determine that it is not noise.
            /// </summary>
            public byte NoiseCountLimitRising { get; set; }

            /// <summary>
            /// Determines the operation rate of the filter. A larger count limit means the filter delay is operating more slowly (rising).
            /// </summary>
            public byte FilterDelayCountLimitRising { get; set; }

            /// <summary>
            /// Determines the largest magnitude of variation to pass through the baseline filter (falling).
            /// </summary>
            public byte MaxHalfDeltaFalling { get; set; }

            /// <summary>
            /// Determines the incremental change when non-noise drift is detected (falling).
            /// </summary>
            public byte NoiseHalfDeltaFalling { get; set; }

            /// <summary>
            /// Determines the number of samples consecutively greater than the Max Half Delta value (falling).
            /// This is necessary to determine that it is not noise.
            /// </summary>
            public byte NoiseCountLimitFalling { get; set; }

            /// <summary>
            /// Determines the operation rate of the filter. A larger count limit means the filter delay is operating more slowly (falling).
            /// </summary>
            public byte FilterDelayCountLimitFalling { get; set; }

            /// <summary>
            /// Electrode touch threshold.
            /// </summary>
            /// <remark>
            /// Threshold settings are dependant on the touch/release signal strength, system sensitivity and noise immunity requirements.
            /// In a typical touch detection application, threshold is typically in the range 0x04~0x10.
            /// The touch threshold is several counts larger than the release threshold. This is to provide hysteresis and to prevent noise and jitter.
            /// </remark>
            public byte ElectrodeTouchThreshold { get; set; }

            /// <summary>
            /// Electrode release threshold.
            /// </summary>
            /// <remark>
            /// Threshold settings are dependant on the touch/release signal strength, system sensitivity and noise immunity requirements.
            /// In a typical touch detection application, threshold is typically in the range 0x04~0x10.
            /// The touch threshold is several counts larger than the release threshold. This is to provide hysteresis and to prevent noise and jitter.
            /// </remark>
            public byte ElectrodeReleaseThreshold { get; set; }

            /// <summary>
            /// Filter/Global Charge Discharge Time Configuration (datasheet page 14).
            /// </summary>
            public byte ChargeDischargeTimeConfiguration { get; set; }

            /// <summary>
            /// Electrode Configuration (datasheet page 15).
            /// </summary>
            public byte ElectrodeConfiguration { get; set; }
        }

        // Registers of Mpr121. Please see datasheet page 8 for details.
        enum Registers : byte
        {
            /// <summary>
            /// MHD Rising.
            /// </summary>
            MHDR = 0x2B,

            /// <summary>
            /// NHD Amount Rising.
            /// </summary>
            NHDR = 0x2C,

            /// <summary>
            /// NCL Rising.
            /// </summary>
            NCLR = 0x2D,

            /// <summary>
            /// FDL Rising.
            /// </summary>
            FDLR = 0x2E,

            /// <summary>
            /// MHD Falling.
            /// </summary>
            MHDF = 0x2F,

            /// <summary>
            /// NHD Amount Falling.
            /// </summary>
            NHDF = 0x30,

            /// <summary>
            /// NCL Falling.
            /// </summary>
            NCLF = 0x31,

            /// <summary>
            /// FDL Falling.
            /// </summary>
            FDLF = 0x32,

            /// <summary>
            /// ELE0 Touch Threshold.
            /// </summary>
            E0TTH = 0x41,

            /// <summary>
            /// ELE0 Release Threshold.
            /// </summary>
            E0RTH = 0x42,

            /// <summary>
            /// ELE1 Touch Threshold.
            /// </summary>
            E1TTH = 0x43,

            /// <summary>
            /// ELE1 Release Threshold.
            /// </summary>
            E1RTH = 0x44,

            /// <summary>
            /// ELE2 Touch Threshold.
            /// </summary>
            E2TTH = 0x45,

            /// <summary>
            /// ELE2 Release Threshold.
            /// </summary>
            E2RTH = 0x46,

            /// <summary>
            /// ELE3 Touch Threshold.
            /// </summary>
            E3TTH = 0x47,

            /// <summary>
            /// ELE3 Release Threshold.
            /// </summary>
            E3RTH = 0x48,

            /// <summary>
            /// ELE4 Touch Threshold.
            /// </summary>
            E4TTH = 0x49,

            /// <summary>
            /// ELE4 Release Threshold.
            /// </summary>
            E4RTH = 0x4A,

            /// <summary>
            /// ELE5 Touch Threshold.
            /// </summary>
            E5TTH = 0x4B,

            /// <summary>
            /// ELE5 Release Threshold.
            /// </summary>
            E5RTH = 0x4C,

            /// <summary>
            /// ELE6 Touch Threshold.
            /// </summary>
            E6TTH = 0x4D,

            /// <summary>
            /// ELE6 Release Threshold.
            /// </summary>
            E6RTH = 0x4E,

            /// <summary>
            /// ELE7 Touch Threshold.
            /// </summary>
            E7TTH = 0x4F,

            /// <summary>
            /// ELE7 Release Threshold.
            /// </summary>
            E7RTH = 0x50,

            /// <summary>
            /// ELE8 Touch Threshold.
            /// </summary>
            E8TTH = 0x51,

            /// <summary>
            /// ELE8 Release Threshold.
            /// </summary>
            E8RTH = 0x52,

            /// <summary>
            /// ELE9 Touch Threshold.
            /// </summary>
            E9TTH = 0x53,

            /// <summary>
            /// ELE9 Release Threshold.
            /// </summary>
            E9RTH = 0x54,

            /// <summary>
            /// ELE10 Touch Threshold.
            /// </summary>
            E10TTH = 0x55,

            /// <summary>
            /// ELE10 Release Threshold.
            /// </summary>
            E10RTH = 0x56,

            /// <summary>
            ///
            /// </summary>
            E11TTH = 0x57,

            /// <summary>
            /// ELE11 Touch Threshold.
            /// </summary>
            E11RTH = 0x58,

            /// <summary>
            /// ELE11 Release Threshold.
            /// </summary>
            CDTC = 0x5D,

            /// <summary>
            /// Electrode Configuration.
            /// </summary>
            ELECONF = 0x5E
        }
    }
}
