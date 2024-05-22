using Meadow.Foundation.Helpers;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.RTCs
{
    /// <summary>
    /// Base class for DS323x family of real-time clocks
    /// </summary>
    public partial class Ds323x : PollingSensorBase<Temperature>, IRealTimeClock, II2cPeripheral, IDisposable
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <inheritdoc/>
        public bool IsRunning
        {
            get => true;
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Is the object disposed
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Number of registers that hold the date and time information.
        /// </summary>
        private const int DATE_TIME_REGISTERS_SIZE = 0x07;

        /// <summary>
        /// Bit mask to turn Alarm1 on
        /// </summary>
        private const byte ALARM1_ENABLE = 0x01;

        /// <summary>
        /// Bit mask to turn Alarm1 off
        /// </summary>
        private const byte ALARM1_DISABLE = 0xfe;

        /// <summary>
        /// Bit mask to turn Alarm2 on
        /// </summary>
        private const byte ALARM2_ENABLE = 0x02;

        /// <summary>
        /// Bit mask to turn Alarm2 off
        /// </summary>
        private const byte ALARM2_DISABLE = 0xfd;

        /// <summary>
        /// Interrupt flag for Alarm1
        /// </summary>
        private const byte ALARM1_INTERRUPT_FLAG = 0x01;

        /// <summary>
        /// Bit mask to clear the Alarm1 interrupt
        /// </summary>
        private const byte ALARM1_INTERRUPT_OFF = 0xfe;

        /// <summary>
        /// Interrupt flag for the Alarm2 interrupt
        /// </summary>
        private const byte ALARM2_INTERRUPT_FLAG = 0x02;

        /// <summary>
        /// Bit mask to clear the Alarm2 interrupt
        /// </summary>
        private const byte ALARM2_INTERRUPT_OFF = 0xfd;

        private AlarmRaised alarm1Delegate = default!;
        private AlarmRaised alarm2Delegate = default!;

        /// <summary>
        /// Did we create the port(s) used by the peripheral
        /// </summary>
        private bool createdPort;

        private readonly Memory<byte> readBuffer;

        /// <summary>
        /// Delegate for the alarm events.
        /// </summary>
        public delegate void AlarmRaised(object sender);

        /// <summary>
        /// Event raised when Alarm1 is triggered.
        /// </summary>
        public event AlarmRaised OnAlarm1Raised
        {
            add
            {
                if (InterruptPort == null)
                {
                    throw new DeviceConfigurationException("Alarm events require creating the Component with an Interrupt Port");
                }
                alarm1Delegate += value;
            }
            remove
            {
                if (alarm1Delegate != null)
                {
                    alarm1Delegate -= value;
                }
            }
        }

        /// <summary>
        /// Event raised when Alarm2 is triggered.
        /// </summary>
        public event AlarmRaised OnAlarm2Raised
        {
            add
            {
                if (InterruptPort == null)
                {
                    throw new DeviceConfigurationException("Alarm events require creating the Component with an Interrupt Port");
                }
                alarm2Delegate += value;
            }
            remove
            {
                if (alarm2Delegate != null)
                {
                    alarm2Delegate -= value;
                }
            }
        }

        /// <summary>
        /// Get / Set the current date and time.
        /// </summary>
        public DateTimeOffset CurrentDateTime
        {
            get => GetTime();
            set => SetTime(value);
        }

        /// <inheritdoc/>
        protected override Task<Temperature> ReadSensor()
        {
            var ctl = i2cComms.ReadRegister(Registers.Control);
            ctl |= 1 << 5;
            i2cComms.WriteRegister(Registers.Control, ctl);

            byte status;

            do
            {
                status = i2cComms.ReadRegister(Registers.ControlStatus);
            } while ((status & (1 << 2)) != (1 << 2));

            var data = readBuffer.Span[0..2];
            i2cComms.ReadRegister(Registers.TemperatureMSB, data);
            if ((data[0] & 0x80) != 0)
            {
                // negative
                data[0] = (byte)(data[0] | ~((1 << 8) - 1));
            }

            var temperature = 0.25 * (data[1] >> 6) + data[0];
            return Task.FromResult(new Temperature(temperature, Temperature.UnitType.Celsius));
        }

        /// <summary>
        /// Get the current die temperature.
        /// </summary>
        public Temperature Temperature => Conditions;

        /// <summary>
        /// I2C Communication bus used to communicate with the i2cComms
        /// </summary>
        protected II2cCommunications i2cComms;

        /// <summary>
        /// Interrupt port attached to the DS323x RTC module.
        /// </summary>
        protected IDigitalInterruptPort? InterruptPort { get; private set; }

        /// <summary>
        /// Control register.
        /// </summary>
        /// <remarks>
        /// Control register contains the following bit (in sequence b7 - b0):
        /// EOSC - BBSQW - CONV - RS1 - RS2 - INTCN - A2IE - A1IE
        /// </remarks>
        protected byte ControlRegister
        {
            get { return i2cComms.ReadRegister(Registers.Control); }
            set { i2cComms.WriteRegister(Registers.Control, value); }
        }

        /// <summary>
        /// Control and status register.
        /// </summary>
        /// <remarks>
        /// Control and status register contains the following bit (in sequence b7 - b0):
        /// OSF - 0 - 0 - 0 - EN32KHZ - BSY - A2F - A1F
        /// </remarks>
        protected byte ControlStatusRegister
        {
            get { return i2cComms.ReadRegister(Registers.ControlStatus); }
            set { i2cComms.WriteRegister(Registers.ControlStatus, value); }
        }

        /// <summary>
        /// Determine which alarm has been raised.
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

        /// <summary>
        /// Create a new Ds323x object
        /// </summary>
        protected Ds323x(I2cCommunications i2cComms, IPin? interruptPin)
        {
            this.i2cComms = i2cComms;

            if (interruptPin != null)
            {
                var interruptPort = interruptPin.CreateDigitalInterruptPort(InterruptMode.EdgeFalling, ResistorMode.InternalPullUp, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(10));
                createdPort = true;

                Initialize(interruptPort);
            }

            readBuffer = new byte[0x12];
        }

        /// <summary>
        /// Create a new Ds323x object
        /// </summary>
        protected Ds323x(I2cCommunications i2cComms, IDigitalInterruptPort? interruptPort)
        {
            this.i2cComms = i2cComms;

            if (interruptPort != null)
            {
                Initialize(interruptPort);
            }
        }

        private void Initialize(IDigitalInterruptPort interruptPort)
        {
            switch (interruptPort.InterruptMode)
            {
                case InterruptMode.EdgeFalling:
                case InterruptMode.EdgeBoth:
                    // we need a rising edge, so all good;
                    break;
                default:
                    throw new DeviceConfigurationException("RTC alarms require a falling-edge enabled interrupt port");
            }

            InterruptPort = interruptPort;
            InterruptPort.Changed += (s, cr) =>
            {
                //Alarm interrupt has been raised, work out which one and raise the necessary event.
                if ((alarm1Delegate != null) || (alarm2Delegate != null))
                {
                    var alarm = WhichAlarm;
                    if (((alarm == Alarm.Alarm1Raised) || (alarm == Alarm.BothAlarmsRaised)) && (alarm1Delegate != null))
                    {
                        alarm1Delegate(this);
                    }
                    if (((alarm == Alarm.Alarm2Raised) || (alarm == Alarm.BothAlarmsRaised)) && (alarm2Delegate != null))
                    {
                        alarm2Delegate(this);
                    }
                }
            };
        }

        /// <inheritdoc/>
        public DateTimeOffset GetTime()
        {
            var data = readBuffer.Span[0..DATE_TIME_REGISTERS_SIZE];
            i2cComms.ReadRegister(Registers.Seconds, data);
            return DecodeDateTimeRegisters(data);
        }

        /// <inheritdoc/>
        public void SetTime(DateTimeOffset time)
        {
            i2cComms.WriteRegister(Registers.Seconds, EncodeDateTimeRegisters(time));
        }

        /// <summary>
        /// Get the the date and time register contents
        /// </summary>
        /// <param name="data">Register contents.</param>
        /// <returns>DateTimeOffset object version of the data.</returns>
        protected DateTimeOffset DecodeDateTimeRegisters(Span<byte> data)
        {
            var seconds = Converters.BCDToByte(data[0]);
            var minutes = Converters.BCDToByte(data[1]);
            byte hour;

            if ((data[2] & 0x40) != 0)
            {
                hour = Converters.BCDToByte((byte)(data[2] & 0x1f));
                if ((data[2] & 0x20) != 0)
                {
                    hour += 12;
                }
            }
            else
            {
                hour = Converters.BCDToByte((byte)(data[2] & 0x3f));
            }

            var day = Converters.BCDToByte(data[4]);
            var month = Converters.BCDToByte((byte)(data[5] & 0x7f));
            var year = (ushort)(1900 + Converters.BCDToByte(data[6]));
            if ((data[5] & 0x80) != 0)
            {
                year += 100;
            }
            try
            {
                return new DateTime(year, month, day, hour, minutes, seconds);
            }
            catch
            {
                // uninitialized RTC will have zeros, which won't parse to a DateTimeOffset
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Encode the a DateTime object into the format used by the DS323x chips.
        /// </summary>
        /// <param name="dt">DateTime object to encode.</param>
        /// <returns>Bytes to send to the DS323x chip.</returns>
        protected byte[] EncodeDateTimeRegisters(DateTimeOffset dt)
        {
            var data = new byte[7];

            data[0] = Converters.ByteToBCD((byte)dt.Second);
            data[1] = Converters.ByteToBCD((byte)dt.Minute);
            data[2] = Converters.ByteToBCD((byte)dt.Hour);
            data[3] = (byte)dt.DayOfWeek;
            data[4] = Converters.ByteToBCD((byte)dt.Day);
            data[5] = Converters.ByteToBCD((byte)dt.Month);
            if (dt.Year > 1999)
            {
                data[5] |= 0x80;
            data[6] = Converters.ByteToBCD((byte)((dt.Year - 2000) & 0xff));
            }
            else
            {
                data[6] = Converters.ByteToBCD((byte)((dt.Year - 1900) & 0xff));
            }
            return data;
        }

        /// <summary>
        /// Convert the day of the week to a byte.
        /// </summary>
        /// <param name="day">Day of the week</param>
        /// <returns>Byte representation of the day of the week (Sunday = 1).</returns>
        protected byte DayOfWeekToByte(DayOfWeek day)
        {
            return (byte)(day + 1);
        }

        /// <summary>
        /// Set one of the two alarms on the DS323x module.
        /// </summary>
        /// <param name="alarm">Define the alarm to be set.</param>
        /// <param name="time">Date and time for the alarm.</param>
        /// <param name="type">Type of alarm to set.</param>
        public void SetAlarm(Alarm alarm, DateTimeOffset time, AlarmType type)
        {
            byte[] data;
            var register = Registers.Alarm1Seconds;
            var element = 0;

            if (alarm == Alarm.Alarm1Raised)
            {
                data = new byte[5];
                element = 1;
                data[0] = Converters.ByteToBCD((byte)(time.Second & 0xff));
            }
            else
            {
                data = new byte[4];
                register = Registers.Alarm2Minutes;
            }
            data[element++] = Converters.ByteToBCD((byte)(time.Minute & 0xff));
            data[element++] = Converters.ByteToBCD((byte)(time.Hour & 0xff));
            if ((type == AlarmType.WhenDayHoursMinutesMatch) || (type == AlarmType.WhenDayHoursMinutesSecondsMatch))
            {
                data[element] = (byte)(DayOfWeekToByte(time.DayOfWeek) | 0x40);
            }
            else
            {
                data[element] = Converters.ByteToBCD((byte)(time.Day & 0xff));
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
                //  Alarm 2 interrupts.
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
            i2cComms.WriteRegister(register, data);
            //
            //  Turn the relevant alarm on.
            //
            var controlRegister = ControlRegister;
            var bits = (byte)ControlRegisterBits.A1IE;
            if (alarm == Alarm.Alarm2Raised)
            {
                bits = (byte)ControlRegisterBits.A2IE;
            }
            controlRegister |= (byte)ControlRegisterBits.INTCON;
            controlRegister |= bits;
            ControlRegister = controlRegister;
        }

        /// <summary>
        /// Enable or disable the specified alarm.
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
        /// Clear the alarm interrupt flag for the specified alarm.
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
        /// Display the registers.
        /// </summary>
        public void DisplayRegisters()
        {
            var data = readBuffer.Span[0..0x12];
            i2cComms.ReadRegister(0, data);
            DebugInformation.DisplayRegisters(0, data);
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        /// <param name="disposing">Is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing && createdPort)
                {
                    InterruptPort?.Dispose();
                }

                IsDisposed = true;
            }
        }
    }
}