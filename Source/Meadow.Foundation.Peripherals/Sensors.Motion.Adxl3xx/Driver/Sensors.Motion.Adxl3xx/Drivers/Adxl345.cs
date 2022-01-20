using Meadow.Foundation.Helpers;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    ///     Driver for the ADXL345 triple axis accelerometer.
    ///     +/- 16g
    /// </summary>
    public partial class Adxl345 : ByteCommsSensorBase<Acceleration3D>, IAccelerometer
    {
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated;

        static double ADXL345_MG2G_MULTIPLIER = (0.004);

        /// <summary>
        /// Minimum value that can be used for the update interval when the
        /// sensor is being configured to generate interrupts.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        public Acceleration3D? Acceleration3D => Conditions;

        /// <summary>
        ///     Values stored in this register are automatically added to the X reading.
        /// </summary>
        /// <remarks>
        ///     Scale factor is 15.6 mg/LSB so 0x7f represents an offset of 2g.
        /// </remarks>
        public sbyte OffsetX
        {
            get { return (sbyte)ReadRegister(Register.OFFSET_X); }
            set { WriteRegister(Register.OFFSET_X, (byte)value); }
        }

        /// <summary>
        ///     Values stored in this register are automatically added to the Y reading.
        /// </summary>
        /// <remarks>
        ///     Scale factor is 15.6 mg/LSB so 0x7f represents an offset of 2g.
        /// </remarks>
        public sbyte OffsetY
        {
            get { return (sbyte)ReadRegister(Register.OFFSET_Y); }
            set { WriteRegister(Register.OFFSET_Y, (byte)value); }
        }

        /// <summary>
        ///     Values stored in this register are automatically added to the Z reading.
        /// </summary>
        /// <remarks>
        ///     Scale factor is 15.6 mg/LSB so 0x7f represents an offset of 2g.
        /// </remarks>
        public sbyte OffsetZ
        {
            get { return (sbyte)ReadRegister(Register.OFFSET_Z); }
            set { WriteRegister(Register.OFFSET_Z, (byte)value); }
        }

        /// <summary>
        ///     Create a new instance of the ADXL345 communicating over the I2C interface.
        /// </summary>
        /// <param name="address">Address of the I2C sensor</param>
        /// <param name="i2cBus">I2C bus</param>
        public Adxl345(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        {
        }

        /// <summary>
        ///     Create a new instance of the ADXL345 communicating over the I2C interface.
        /// </summary>
        /// <param name="address">Address of the I2C sensor</param>
        /// <param name="i2cBus">I2C bus</param>
        public Adxl345(II2cBus i2cBus, byte address)
            : base(i2cBus, address)
        {
            var deviceID = ReadRegister(Register.DEVICE_ID);

            if (deviceID != 0xe5)
            {
                throw new Exception("Invalid device ID.");
            }
        }

        protected override Task<Acceleration3D> ReadSensor()
        {
            return Task.Run(() =>
            {
                // read the data from the sensor starting at the X0 register
                Peripheral.ReadRegister((byte)Register.X0, ReadBuffer.Span[0..6]);

                return new Acceleration3D(
                    new Acceleration(ADXL345_MG2G_MULTIPLIER * (short)(ReadBuffer.Span[0] + (ReadBuffer.Span[1] << 8)), Acceleration.UnitType.MetersPerSecondSquared),
                    new Acceleration(ADXL345_MG2G_MULTIPLIER * (short)(ReadBuffer.Span[2] + (ReadBuffer.Span[3] << 8)), Acceleration.UnitType.MetersPerSecondSquared),
                    new Acceleration(ADXL345_MG2G_MULTIPLIER * (short)(ReadBuffer.Span[4] + (ReadBuffer.Span[5] << 8)), Acceleration.UnitType.MetersPerSecondSquared)
                    );

            });
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Acceleration3D> changeResult)
        {
            Acceleration3DUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        ///     Set the PowerControl register (see pages 25 and 26 of the data sheet)
        /// </summary>
        /// <param name="linkActivityAndInactivity">Link the activity and inactivity events.</param>
        /// <param name="autoSleep">Enable / disable auto sleep when the activity and inactivity are linked.</param>
        /// <param name="measuring">Enable or disable measurements (turn on or off).</param>
        /// <param name="sleep">Put the part to sleep (true) or run in normal more (false).</param>
        /// <param name="frequency">Frequency of measurements when the part is in sleep mode.</param>
        public void SetPowerState(bool linkActivityAndInactivity, bool autoSleep, bool measuring, bool sleep, Frequencies frequency)
        {
            byte data = 0;
            if (linkActivityAndInactivity)
            {
                data |= 0x20;
            }
            if (autoSleep)
            {
                data |= 0x10;
            }
            if (measuring)
            {
                data |= 0x08;
            }
            if (sleep)
            {
                data |= 0x40;
            }
            data |= (byte)frequency;

            WriteRegister(Register.POWER_CONTROL, data);
        }

        /// <summary>
        ///     Configure the data format (see pages 26 and 27 of the data sheet).
        /// </summary>
        /// <param name="selfTest">Put the device into self test mode when true.</param>
        /// <param name="spiMode">Use 3-wire SPI (true) or 4-wire SPI (false).</param>
        /// <param name="fullResolution">
        ///     Set to full resolution (true) or 10-bit mode using the range determined by the range
        ///     parameter (false).
        /// </param>
        /// <param name="justification">Left-justified when true, right justified with sign extension when false.</param>
        /// <param name="range">Set the range of the sensor to 2g, 4g, 8g or 16g</param>
        /// <remarks>
        ///     The range of the sensor is determined by the following table:
        ///         0:  +/- 2g
        ///         1:  +/- 4g
        ///         2:  +/- 8g
        ///         3:  +/ 16g
        /// </remarks>
        public void SetDataFormat(bool selfTest, bool spiMode, bool fullResolution, bool justification, GForceRanges range)
        {
            byte data = 0;
            if (selfTest)
            {
                data |= 0x80;
            }
            if (spiMode)
            {
                data |= 0x40;
            }
            if (fullResolution)
            {
                data |= 0x04;
            }
            if (justification)
            {
                data |= 0x02;
            }
            data |= (byte)range;

            WriteRegister(Register.DATA_FORMAT, data);
        }

        /// <summary>
        ///     Set the data rate and low power mode for the sensor.
        /// </summary>
        /// <param name="dataRate">Data rate for the sensor.</param>
        /// <param name="lowPower">
        ///     Setting this to true will enter low power mode (note measurement will encounter more noise in
        ///     this mode).
        /// </param>
        public void SetDataRate(byte dataRate, bool lowPower)
        {
            if (dataRate > 0xff)
            {
                throw new ArgumentOutOfRangeException(nameof(dataRate), "Data rate should be in the range 0-15 inclusive");
            }

            var data = dataRate;

            if (lowPower)
            {
                data |= 0x10;
            }

            WriteRegister(Register.DATA_RATE, data);
        }

        private void WriteRegister(Register register, byte value)
        {
            Peripheral.WriteRegister((byte)register, value);
        }

        private byte ReadRegister(Register register)
        {
            return Peripheral.ReadRegister((byte)register);
        }

        /// <summary>
        ///     Dump the registers to the debug output stream.
        /// </summary>
        public void DisplayRegisters()
        {
            byte[] registerData = new byte[29];
            Peripheral.ReadRegister((byte)Register.TAP_THRESHOLD, registerData);
            DebugInformation.DisplayRegisters((byte)Register.TAP_THRESHOLD, registerData);
        }
    }
}