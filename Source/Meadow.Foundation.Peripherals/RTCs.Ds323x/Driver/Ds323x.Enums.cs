namespace Meadow.Foundation.RTCs
{
    public partial class Ds323x
    {
        /// <summary>
        /// Possible values for the alarm that can be set or alarm that has been raised
        /// </summary>
        public enum Alarm
        {
            /// <summary>
            /// Alarm 1 raised
            /// </summary>
            Alarm1Raised,
            /// <summary>
            /// Alarm 2 raised
            /// </summary>
            Alarm2Raised,
            /// <summary>
            /// Alarm 1 and 2 raised
            /// </summary>
            BothAlarmsRaised,
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown
        }

        /// <summary>
        /// Registers bits in the control register
        /// </summary>
        enum ControlRegisterBits
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
        enum StatusRegisterBits
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
            /// <summary>
            /// 1Hz
            /// </summary>
            OneHz = 0,
            /// <summary>
            /// 1kHz
            /// </summary>
            OnekHz = 1,
            /// <summary>
            /// 4kHz
            /// </summary>
            FourkHz = 2,
            /// <summary>
            /// 8kHz
            /// </summary>
            EightkHz = 3
        }

        /// <summary>
        /// Determine which alarm should be raised
        /// </summary>
        public enum AlarmType
        {
            /// <summary>
            /// Alarm 1 - once per second
            /// </summary>
            OncePerSecond,
            /// <summary>
            /// Alarm 1 - when seconds match
            /// </summary>
            WhenSecondsMatch,
            /// <summary>
            /// Alarm 1 - when seconds and minutes match
            /// </summary>
            WhenMinutesSecondsMatch,
            /// <summary>
            /// Alarm 1 - when seconds, minutes and hours match
            /// </summary>
            WhenHoursMinutesSecondsMatch,
            /// <summary>
            /// Alarm 1 - when date and time match
            /// </summary>
            WhenDateHoursMinutesSecondsMatch,
            /// <summary>
            /// Alarm 1 - when days and time match
            /// </summary>
            WhenDayHoursMinutesSecondsMatch,

            /// <summary>
            /// Alarm 2 - once per minute
            /// </summary>
            OncePerMinute,
            /// <summary>
            /// Alarm 2 - when minutes match
            /// </summary>
            WhenMinutesMatch,
            /// <summary>
            /// Alarm 2 - when hours and minutes match
            /// </summary>
            WhenHoursMinutesMatch,
            /// <summary>
            /// Alarm 2 - when date, hours and minutes match
            /// </summary>
            WhenDateHoursMinutesMatch,
            /// <summary>
            /// Alarm 2 - when days, hours and minutes match
            /// </summary>
            WhenDayHoursMinutesMatch
        }
    }
}