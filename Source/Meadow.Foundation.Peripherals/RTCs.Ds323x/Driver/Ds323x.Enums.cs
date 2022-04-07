namespace Meadow.Foundation.RTCs
{
    public partial class Ds323x
    {
        /// <summary>
        /// Possible values for the alarm that can be set or alarm that has been raised.
        /// </summary>
        public enum Alarm
        {
            Alarm1Raised,
            Alarm2Raised,
            BothAlarmsRaised,
            Unknown
        }

        /// <summary>
        /// Registers bits in the control register.
        /// </summary>
        private enum ControlRegisterBits
        {
            A1IE = 0x01,
            A2IE = 0x02,
            INTCON = 0x04,
            RS1 = 0x08,
            RS2 = 0x10,
            Conv = 0x20,
            BBSQW = 0x40,
            NotEOSC = 0x80
        }

        /// <summary>
        /// Register bits in the control / status register.
        /// </summary>
        private enum StatusRegisterBits
        {
            A1F = 0x02,
            A2F = 0x02,
            BSY = 0x04,
            EN32Khz = 0x08,
            Crate0 = 0x10,
            Crate1 = 0x20,
            BB32kHz = 0x40,
            OSF = 0x80
        }

        /// <summary>
        /// Possible frequency for the square wave output.
        /// </summary>
        public enum RateSelect
        {
            OneHz = 0,
            OnekHz = 1,
            FourkHz = 2,
            EightkHz = 3
        }

        /// <summary>
        /// Determine which alarm should be raised.
        /// </summary>
        public enum AlarmType
        {
            //
            //  Alarm 1 options.
            //
            OncePerSecond,
            WhenSecondsMatch,
            WhenMinutesSecondsMatch,
            WhenHoursMinutesSecondsMatch,
            WhenDateHoursMinutesSecondsMatch,
            WhenDayHoursMinutesSecondsMatch,

            //
            //  Alarm 2 options.
            //
            OncePerMinute,
            WhenMinutesMatch,
            WhenHoursMinutesMatch,
            WhenDateHoursMinutesMatch,
            WhenDayHoursMinutesMatch
        }
    }
}
