namespace Meadow.Foundation.Sensors.Distance
{
    /// <summary>
    /// Represents the Vl53l0x distance sensor
    /// </summary>
    public partial class Vl53l0x
    {
        /// <summary>
		///     Valid addresses for the sensor.
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

        public enum UnitType
        {
            mm,
            cm,
            inches
        }

        protected enum Register:byte
        {
            RangeStart = 0x00,
            SystemThreahHigh = 0x0C,
            SystemThreshLow = 0x0E,
            SystemSequenceConfig = 0x01,
            SystemRangeConfig = 0x09,
            SystemIntermeasurementPeriod = 0x04,
            SystemInterruptConfigGpio = 0x0A,
            GpioHvMuxActiveHigh = 0x84,
            SystemInterruptClear = 0x0B,
            ResultInterruptStatus = 0x13,
            ResultRangeStatus = 0x14,
            ResultCoreAmbientWindowEventsRtn = 0xBC,
            ResultCoreRangingTotalEventsRtn = 0xC0,
            ResultCoreAmbientWindowEventsRef = 0xD0,
            ResultCoreRangingTotalEventsRef = 0xD4,
            ResultPeakSignalRateRef = 0xB6,
            AlgoPartToPartRangeOffsetMm = 0x28,
            I2CSlaveDeviceAddress = 0x8A,
            MsrcConfigControl = 0x60,
            PreRangeConfigMinSnr = 0x27,
            PreRangeConfigValidPhaseLow = 0x56,
            PreRangeConfigValidPhaseHigh = 0x57,
            PreRangeMinCountRateRtnLimit = 0x64,
            FinalRangeConfigMinSnr = 0x67,
            FinalRangeConfigValidPhaseLow = 0x47,
            FinalRangeConfigValidPhaseHigh = 0x48,
            FinalRangeConfigMinCountRateRtnLimit = 0x44,
            PreRangeConfigSigmaThreshHi = 0x61,
            PreRangeConfigSigmaThreshLo = 0x62,
            PreRangeConfigVcselPeriod = 0x50,
            PreRangeConfigTimeoutMacropHi = 0x51,
            PreRangeConfigTimeoutMacropLo = 0x52,
            SystemHistogramBin = 0x81,
            HistogramConfigInitialPhaseSelect = 0x33,
            HistogramConfigReadoutCtrl = 0x55,
            FinalRangeConfigVcselPeriod = 0x70,
            FinalRangeConfigTimeoutMacropHi = 0x71,
            FinalRangeConfigTimeoutMacropLo = 0x72,
            CrosstalkCompensationPeakRateMcps = 0x20,
            MsrcConfigTimeoutMacrop = 0x46,
            SoftResetGo2SoftResetN = 0xBF,
            IdentificationModelId = 0xC0,
            IdentificationRevisionId = 0xC2,
            OscCalibrateVal = 0xF8,
            GlobalConfigVcselWidth = 0x32,
            GlobalConfigSpadEnablesRef0 = 0xB0,
            GlobalConfigSpadEnablesRef1 = 0xB1,
            GlobalConfigSpadEnablesRef2 = 0xB2,
            GlobalConfigSpadEnablesRef3 = 0xB3,
            GlobalConfigSpadEnablesRef4 = 0xB4,
            GlobalConfigSpadEnablesRef5 = 0xB5,
            GlobalConfigRefEnStartSelect = 0xB6,
            DynamicSpadNumRequestedRefSpad = 0x4E,
            DynamicSpadRefEnStartOffset = 0x4F,
            PowerManagementGo1PowerForce = 0x80,
            VhvConfigPadSclSdaExtsupHv = 0x89,
            AlgoPhasecalLim = 0x30,
            AlgoPhasecalConfigTimeout = 0x30,
        }

        public enum VcselPeriodRange : int
        {
            Pri = 0,
            Final = 1,
        }
    }
}
