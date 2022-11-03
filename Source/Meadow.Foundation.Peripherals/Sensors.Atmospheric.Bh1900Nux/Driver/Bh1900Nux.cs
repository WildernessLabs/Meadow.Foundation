using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// Represents a Bh1900Nux temperature sensor
    /// </summary>
    public partial class Bh1900Nux : ByteCommsSensorBase<Units.Temperature>, ITemperatureSensor
    {
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        /// <summary>
        /// The current temperature
        /// </summary>
        public Units.Temperature? Temperature => Conditions;

        public Bh1900Nux(II2cBus i2cBus, Address address)
            : base(i2cBus, (byte)address, 2, 2)
        {
            if (address < Address.Address_0x48 || address > Address.Address_0x4f)
            {
                throw new ArgumentOutOfRangeException("Bh1900Nux address must be in the range of 0x48-0x4f");
            }

            Reset();
        }

        public Bh1900Nux(II2cBus i2cBus, byte address)
            : this(i2cBus, (Address)address)
        { }

        /// <summary>
        /// Reset the sensor
        /// </summary>
        public void Reset()
        {
            Peripheral?.WriteRegister((byte)Register.Reset, 0x01);
        }

        int GetConfig()
        {
            Peripheral?.ReadRegister((byte)Register.Configuration, ReadBuffer.Span[0..2]);
            return ReadBuffer.Span[0] << 8 | ReadBuffer.Span[1];
        }

        void SetConfig(int cfg)
        {
            Peripheral?.WriteRegister((byte)Register.Configuration, new byte[] { (byte)(cfg >> 8), (byte)(cfg & 0xff) });
        }

        /// <summary>
        /// The measurement mode
        /// </summary>
        public Mode MeasurementMode
        {
            get => (Mode)(GetConfig() >> 15);
            set
            {
                var currentMode = GetConfig();
                if (value == Mode.Single)
                {
                    currentMode |= (1 << 15);
                }
                else
                {
                    currentMode &= ~(1 << 15);
                }
                SetConfig(currentMode);
            }
        }

        /// <summary>
        /// Set the sensor to sleep state
        /// </summary>
        public void Sleep()
        {
            var currentMode = GetConfig();
            currentMode |= (1 << 10);
            SetConfig(currentMode);
        }

        public void Wake()
        {
            var currentMode = GetConfig();
            currentMode &= ~(1 << 10);
            SetConfig(currentMode);
        }

        public FaultQueue FaultQueueDepth
        {
            get => (FaultQueue)((GetConfig() >> 12) & 0x03);
            set
            {
                var currentMode = GetConfig();
                currentMode |= ((int)value << 12);
                SetConfig(currentMode);
            }
        }

        public Polarity AlertPolarity
        {
            get => (Polarity)((GetConfig() >> 11) & 0x01);
            set
            {
                var currentMode = GetConfig();
                if (value == Polarity.ActiveHigh)
                {
                    currentMode |= (1 << 11);
                }
                else
                {
                    currentMode &= ~(1 << 11);
                }
                SetConfig(currentMode);
            }
        }

        public bool AlertIsActive
        {
            get => ((GetConfig() >> 14) & 0x01) != 0;
        }

        public Units.Temperature LowLimit
        {
            get
            {
                Peripheral?.ReadRegister((byte)Register.TLow, ReadBuffer.Span[0..2]);

                return RegisterToTemp(ReadBuffer);
            }
            set
            {
                Peripheral?.WriteRegister((byte)Register.TLow, TempToBytes(value));
            }
        }

        public Units.Temperature HighLimit
        {
            get
            {
                Peripheral?.ReadRegister((byte)Register.THigh, ReadBuffer.Span[0..2]);

                return RegisterToTemp(ReadBuffer);
            }
            set
            {
                Peripheral?.WriteRegister((byte)Register.THigh, TempToBytes(value));
            }
        }

        private byte[] TempToBytes(Units.Temperature t)
        {
            var binary = (int)(t.Celsius * 16);
            binary <<= 4;
            return new byte[] { (byte)(binary >> 8), (byte)(binary & 0xff) };
        }

        private Units.Temperature RegisterToTemp(Memory<byte> data)
        {
            var c = (data.Span[0] << 8 | ReadBuffer.Span[1]) >> 4; // if that's confusing, see the data sheet for more info

            if ((c & 0x800) != 0)
            {
                // high-bit is on, it's a negative number
                // twos-complement, so invert and add 1, but only 12 bits
                c = ((~c & 0xffff) + 1) * -1;
            }

            return new Units.Temperature(c * 0.0625d, Units.Temperature.UnitType.Celsius);
        }

        /// <summary>
        /// Read the temperature
        /// </summary>
        /// <returns>The current temperature value</returns>
        protected override async Task<Units.Temperature> ReadSensor()
        {
            return await Task.Run(() =>
            {
                // 12-bit data
                // Negative numbers are represented in binary twos complement format. The
                // Temperature Register is 0x0000 until the first conversion complete after a software
                // reset or power - on.
                // Measurement Temperature Value [°C] = Temperature Data [11:0] x 0.0625
                Peripheral?.ReadRegister((byte)Register.Temperature, ReadBuffer.Span[0..2]);

                return RegisterToTemp(ReadBuffer);
            });
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Units.Temperature> changeResult)
        {
            TemperatureUpdated?.Invoke(this, changeResult);

            base.RaiseEventsAndNotify(changeResult);
        }
    }
}