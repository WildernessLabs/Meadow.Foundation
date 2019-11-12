using System;
using Meadow.Foundation.Helpers;
using Meadow.Hardware;

namespace Meadow.Foundation.RTCs
{
    public class DS323x
    {
        #region Classes / structures

        /// <summary>
        ///     Register addresses in the sensor.
        /// </summary>
        protected static class Registers
        {
            public static readonly byte Seconds = 0x00;
            public static readonly byte Minutes = 0x01;
            public static readonly byte Hours = 0x02;
            public static readonly byte Day = 0x03;
            public static readonly byte Date = 0x04;
            public static readonly byte Month = 0x05;
            public static readonly byte Year = 0x06;
            public static readonly byte Alarm1Seconds = 0x07;
            public static readonly byte Alarm1Minutes = 0x08;
            public static readonly byte Alarm1Hours = 0x09;
            public static readonly byte Alarm1DayDate = 0x0a;
            public static readonly byte Alarm2Minutes = 0x0b;
            public static readonly byte Alarm2Hours = 0x0c;
            public static readonly byte Alarm2DayDate = 0x0d;
            public static readonly byte Control = 0x0e;
            public static readonly byte ControlStatus = 0x0f;
            public static readonly byte AgingOffset = 0x10;
            public static readonly byte TemperatureMSB = 0x11;
            public static readonly byte TemperatureLSB = 0x12;
        }

        #endregion Classes / structures

        #region Constants

        /// <summary>
        ///     Number of registers that hold the date and time information.
        /// </summary>
        private const int DATE_TIME_REGISTERS_SIZE = 0x07;

        /// <summary>
        ///     Bit mask to turn Alarm1 on.
        /// </summary>
        private const byte ALARM1_ENABLE = 0x01;

        /// <summary>
        ///     Bit mask to turn Alarm1 off.
        /// </summary>
        private const byte ALARM1_DISABLE = 0xfe;

        /// <summary>
        ///     Bit mask to turn Alarm2 on.
        /// </summary>
        private const byte ALARM2_ENABLE = 0x02;

        /// <summary>
        ///     Bit mask to turn Alarm2 off.
        /// </summary>
        private const byte ALARM2_DISABLE = 0xfd;

        /// <summary>
        ///     Interrupt flag for Alarm1.
        /// </summary>
        private const byte ALARM1_INTERRUPT_FLAG = 0x01;

        /// <summary>
        ///     Bit mask to clear the Alarm1 interrupt.
        /// </summary>
        private const byte ALARM1_INTERRUPT_OFF = 0xfe;

        /// <summary>
        ///     Interrupt flag for the Alarm2 interrupt.
        /// </summary>
        private const byte ALARM2_INTERRUPT_FLAG = 0x02;

        /// <summary>
        ///     Bit mask to clear the Alarm2 interrupt.
        /// </summary>
        private const byte ALARM2_INTERRUPT_OFF = 0xfd;

        #endregion Constants

        #region Enums

        /// <summary>
        ///     Possible values for the alarm that can be set or alarm that has been raised.
        /// </summary>
        public enum Alarm
        {
            Alarm1Raised,
            Alarm2Raised,
            BothAlarmsRaised,
            Unknown
        }

        /// <summary>
        ///     Registers bits in the control register.
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
        ///     Register bits in the control / status register.
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
        ///     Possible frequency for the square wave output.
        /// </summary>
        public enum RateSelect
        {
            OneHz = 0,
            OnekHz = 1,
            FourkHz = 2,
            EightkHz = 3
        }

        /// <summary>
        ///     Determine which alarm should be raised.
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

        #endregion Enums

        #region Member variables / fields

        /// <summary>
        ///     DS323x Real Time Clock object.
        /// </summary>
        protected II2cPeripheral _ds323x = null;

        /// <summary>
        ///     Interrupt port attached to the DS323x RTC module.
        /// </summary>
        protected IDigitalInputPort _interruptPort;

        #endregion Member variables / fields

        #region Delegate and events

        /// <summary>
        ///     Delegate for the alarm events.
        /// </summary>
        public delegate void AlarmRaised(object sender);

        /// <summary>
        ///     Event raised when Alarm1 is triggered.
        /// </summary>
        public event AlarmRaised OnAlarm1Raised;

        /// <summary>
        ///     Event raised when Alarm2 is triggered.
        /// </summary>
        public event AlarmRaised OnAlarm2Raised;

        #endregion Delegate and events

        #region Properties

        /// <summary>
        ///     Get / Set the current date and time.
        /// </summary>
        public DateTime CurrentDateTime
        {
            get
            {
                var data = _ds323x.ReadRegisters(Registers.Seconds, DATE_TIME_REGISTERS_SIZE);
                return DecodeDateTimeRegisters(data);
            }
            set { _ds323x.WriteRegisters(Registers.Seconds, EncodeDateTimeRegisters(value)); }
        }

        /// <summary>
        ///     Get the current die temperature.
        /// </summary>
        public double Temperature
        {
            get
            {
                var data = _ds323x.ReadRegisters(Registers.TemperatureMSB, 2);
                var temperature = (ushort) ((data[0] << 2) | (data[1] >> 6));
                return temperature * 0.25;
            }
        }

        /// <summary>
        ///     Control register.
        /// </summary>
        /// <remarks>
        ///     Control register contains the following bit (in sequence b7 - b0):
        ///     EOSC - BBSQW - CONV - RS1 - RS2 - INTCN - A2IE - A1IE
        /// </remarks>
        protected byte ControlRegister
        {
            get { return _ds323x.ReadRegister(Registers.Control); }
            set { _ds323x.WriteRegister(Registers.Control, value); }
        }

        /// <summary>
        ///     Control and status register.
        /// </summary>
        /// <remarks>
        ///     Control and status register contains the following bit (in sequence b7 - b0):
        ///     OSF - 0 - 0 - 0 - EN32KHZ - BSY - A2F - A1F
        /// </remarks>
        protected byte ControlStatusRegister
        {
            get { return _ds323x.ReadRegister(Registers.ControlStatus); }
            set { _ds323x.WriteRegister(Registers.ControlStatus, value); }
        }

        /// <summary>
        ///     Determine which alarm has been raised.
        /// </summary>
        protected Alarm WhichAlarm
        {
            get
            {
                var result = Alarm.Unknown;

                byte controlStatusRegister = ControlStatusRegister;
                if (((controlStatusRegister & 0x01) != 0) && ((controlStatusRegister & 0x02) != 0))
                {
                    result = Alarm.BothAlarmsRaised;
                }
                if ((controlStatusRegister & 0x01) != 0)
                {
                    result = Alarm.Alarm1Raised;
                }
                if ((controlStatusRegister & 0x02) != 0)
                {
                    result = Alarm.Alarm2Raised;
                }
                return result;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        ///     Alarm interrupt has been raised, work out which one and raise the necessary event.
        /// </summary>
        protected void InterruptPort_Changed(object sender, EventArgs e)
        {
            if ((OnAlarm1Raised != null) || (OnAlarm2Raised != null))
            {
                var alarm = WhichAlarm;
                if (((alarm == Alarm.Alarm1Raised) || (alarm == Alarm.BothAlarmsRaised)) && (OnAlarm1Raised != null))
                {
                    OnAlarm1Raised(this);
                }
                if (((alarm == Alarm.Alarm2Raised) || (alarm == Alarm.BothAlarmsRaised)) && (OnAlarm2Raised != null))
                {
                    OnAlarm2Raised(this);
                }
            }
        }

        /// <summary>
        ///     Decode the register contents and create a DateTime version of the
        ///     register contents.
        /// </summary>
        /// <param name="data">Register contents.</param>
        /// <returns>DateTime object version of the data.</returns>
        protected DateTime DecodeDateTimeRegisters(byte[] data)
        {
            var seconds = Converters.BCDToByte(data[0]);
            var minutes = Converters.BCDToByte(data[1]);
            byte hour = 0;
            if ((data[2] & 0x40) != 0)
            {
                hour = Converters.BCDToByte((byte) (data[2] & 0x1f));
                if ((data[2] & 0x20) != 0)
                {
                    hour += 12;
                }
            }
            else
            {
                hour = Converters.BCDToByte((byte) (data[2] & 0x3f));
            }
            var wday = data[3];
            var day = Converters.BCDToByte(data[4]);
            var month = Converters.BCDToByte((byte) (data[5] & 0x7f));
            var year = (ushort) (1900 + Converters.BCDToByte(data[6]));
            if ((data[5] & 0x80) != 0)
            {
                year += 100;
            }
            return new DateTime(year, month, day, hour, minutes, seconds);
        }

        /// <summary>
        ///     Encode the a DateTime object into the format used by the DS323x chips.
        /// </summary>
        /// <param name="dt">DateTime object to encode.</param>
        /// <returns>Bytes to send to the DS323x chip.</returns>
        protected byte[] EncodeDateTimeRegisters(DateTime dt)
        {
            var data = new byte[7];

            data[0] = Converters.ByteToBCD((byte) dt.Second);
            data[1] = Converters.ByteToBCD((byte) dt.Minute);
            data[2] = Converters.ByteToBCD((byte) dt.Hour);
            data[3] = (byte) dt.DayOfWeek;
            data[4] = Converters.ByteToBCD((byte) dt.Day);
            data[5] = Converters.ByteToBCD((byte) dt.Month);
            if (dt.Year > 1999)
            {
                data[5] |= 0x80;
                data[6] = Converters.ByteToBCD((byte) ((dt.Year - 2000) & 0xff));
            }
            else
            {
                data[6] = Converters.ByteToBCD((byte) ((dt.Year - 1900) & 0xff));
            }
            return data;
        }

        /// <summary>
        ///     Convert the day of the week to a byte.
        /// </summary>
        /// <param name="day">Day of the week</param>
        /// <returns>Byte representation of the day of the week (Sunday = 1).</returns>
        protected byte DayOfWeekToByte(DayOfWeek day)
        {
            byte result = 1;
            switch (day)
            {
                case DayOfWeek.Sunday:
                    result = 1;
                    break;
                case DayOfWeek.Monday:
                    result = 2;
                    break;
                case DayOfWeek.Tuesday:
                    result = 3;
                    break;
                case DayOfWeek.Wednesday:
                    result = 4;
                    break;
                case DayOfWeek.Thursday:
                    result = 5;
                    break;
                case DayOfWeek.Friday:
                    result = 6;
                    break;
                case DayOfWeek.Saturday:
                    result = 7;
                    break;
            }
            return result;
        }

        /// <summary>
        ///     Set one of the two alarms on the DS323x module.
        /// </summary>
        /// <param name="alarm">Define the alarm to be set.</param>
        /// <param name="time">Date and time for the alarm.</param>
        /// <param name="type">Type of alarm to set.</param>
        public void SetAlarm(Alarm alarm, DateTime time, AlarmType type)
        {
            byte[] data = null;
            var register = Registers.Alarm1Seconds;
            var element = 0;

            if (alarm == Alarm.Alarm1Raised)
            {
                data = new byte[5];
                element = 1;
                data[0] = Converters.ByteToBCD((byte) (time.Second & 0xff));
            }
            else
            {
                data = new byte[4];
                register = Registers.Alarm2Minutes;
            }
            data[element++] = Converters.ByteToBCD((byte) (time.Minute & 0xff));
            data[element++] = Converters.ByteToBCD((byte) (time.Hour & 0xff));
            if ((type == AlarmType.WhenDayHoursMinutesMatch) || (type == AlarmType.WhenDayHoursMinutesSecondsMatch))
            {
                data[element] = (byte) (DayOfWeekToByte(time.DayOfWeek) | 0x40);
            }
            else
            {
                data[element] = Converters.ByteToBCD((byte) (time.Day & 0xff));
            }
            switch (type)
            {
                //
                //  Alarm 1 interrupts.
                //
                case AlarmType.OncePerSecond:
                    data[0] |= 0x80;
                    data[1] |= 0x80;
                    data[2] |= 0x80;
                    data[3] |= 0x80;
                    break;
                case AlarmType.WhenSecondsMatch:
                    data[1] |= 0x80;
                    data[2] |= 0x80;
                    data[3] |= 0x80;
                    break;
                case AlarmType.WhenMinutesSecondsMatch:
                    data[2] |= 0x80;
                    data[3] |= 0x80;
                    break;
                case AlarmType.WhenHoursMinutesSecondsMatch:
                    data[3] |= 0x80;
                    break;
                case AlarmType.WhenDateHoursMinutesSecondsMatch:
                    break;
                case AlarmType.WhenDayHoursMinutesSecondsMatch:
                    data[3] |= 0x40;
                    break;
                //
                //  Alarm 2 interupts.
                //
                case AlarmType.OncePerMinute:
                    data[0] |= 0x80;
                    data[1] |= 0x80;
                    data[2] |= 0x80;
                    break;
                case AlarmType.WhenMinutesMatch:
                    data[1] |= 0x80;
                    data[2] |= 0x80;
                    break;
                case AlarmType.WhenHoursMinutesMatch:
                    data[2] |= 0x80;
                    break;
                case AlarmType.WhenDateHoursMinutesMatch:
                    break;
                case AlarmType.WhenDayHoursMinutesMatch:
                    data[2] |= 0x40;
                    break;
            }
            _ds323x.WriteRegisters(register, data);
            //
            //  Turn the relevant alarm on.
            //
            var controlRegister = ControlRegister;
            var bits = (byte) ControlRegisterBits.A1IE;
            if (alarm == Alarm.Alarm2Raised)
            {
                bits = (byte) ControlRegisterBits.A2IE;
            }
            controlRegister |= (byte) ControlRegisterBits.INTCON;
            controlRegister |= bits;
            ControlRegister = controlRegister;
        }

        /// <summary>
        ///     Enable or disable the specified alarm.
        /// </summary>
        /// <param name="alarm">Alarm to enable / disable.</param>
        /// <param name="enable">Alarm state, true = on, false = off.</param>
        public void EnableDisableAlarm(Alarm alarm, bool enable)
        {
            byte controlRegister = ControlRegister;
            if (alarm == Alarm.Alarm1Raised)
            {
                if (enable)
                {
                    controlRegister |= ALARM1_ENABLE;
                }
                else
                {
                    controlRegister &= ALARM1_DISABLE;
                }
            }
            else
            {
                if (enable)
                {
                    controlRegister |= ALARM2_ENABLE;
                }
                else
                {
                    controlRegister &= ALARM2_DISABLE;
                }
            }
            ControlRegister = controlRegister;
        }

        /// <summary>
        ///     Clear the alarm interrupt flag for the specified alarm.
        /// </summary>
        /// <param name="alarm">Alarm to clear.</param>
        public void ClearInterrupt(Alarm alarm)
        {
            var controlStatusRegister = ControlStatusRegister;
            switch (alarm)
            {
                case Alarm.Alarm1Raised:
                    controlStatusRegister &= ALARM1_INTERRUPT_OFF;
                    break;
                case Alarm.Alarm2Raised:
                    controlStatusRegister &= ALARM2_INTERRUPT_OFF;
                    break;
                case Alarm.BothAlarmsRaised:
                    controlStatusRegister &= ALARM1_INTERRUPT_OFF;
                    controlStatusRegister &= ALARM2_INTERRUPT_OFF;
                    break;
            }
            ControlStatusRegister = controlStatusRegister;
        }

        /// <summary>
        ///     Display the registers.
        /// </summary>
        public void DisplayRegisters()
        {
            var data = _ds323x.ReadRegisters(0, 0x12);
            DebugInformation.DisplayRegisters(0, data);
        }

        #endregion
    }
}