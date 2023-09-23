using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Accelerometers
{
    /// <summary>
    /// Represents a Lis3MDL is a low-power, high-performance 3-axis magnetometer from STMicroelectronics
    /// with a default full range of ±4 gauss and a 16-bit resolution
    /// </summary>
    public partial class Lis3Mdl : PollingSensorBase<MagneticField3D>, IMagnetometer, II2cPeripheral
    {
        // TODO: Should this also implement ITemperatureSensor and PolingSensorBase<(MagneticField3D, Units.Temperature)>? 
        //       It's only a secondary/low resolution feature of the sensor.

        /// <summary>
        /// Event raised when magnetic field changes
        /// </summary>
        public event EventHandler<IChangeResult<MagneticField3D>> MagneticField3DUpdated = delegate { };

        /// <summary>
        /// Current Magnetic Field 3D
        /// </summary>
        public MagneticField3D? MagneticField3D => Conditions;

        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Current Temperature Sensor reading
        /// </summary>
        public Units.Temperature? Temperature { get; }

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        // Local cached copy of the current scaling.
        private FullScale currentScale;

        /// <summary>
        /// Create a new Lis3Mdl instance
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the sensor</param>
        /// <param name="address">The I2C address</param>
        public Lis3Mdl(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cComms = new I2cCommunications(i2cBus, address);

            Initialize();
        }

        /// <summary>
        /// Initializes the LIS3MDL sensor
        /// </summary>
        void Initialize()
        {
            // Configure the device
            i2cComms.WriteRegister(CTRL_REG1, 0x10); // Temperature sensor: ON, Low Power Mode, ODR: 10Hz
            i2cComms.WriteRegister(CTRL_REG2, 0x00); // Full-scale: ±4 Gauss
            i2cComms.WriteRegister(CTRL_REG3, 0x00); // Continuous mode

            currentScale = FullScale.PlusMinus4Gauss;
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<MagneticField3D> changeResult)
        {
            if (changeResult.New is { } mag)
            {
                MagneticField3DUpdated?.Invoke(this, new ChangeResult<MagneticField3D>(mag, changeResult.Old));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<MagneticField3D> ReadSensor()
        {
            var (x, y, z) = ReadMagnetometerRaw();

            var scaling = GetScaleFactor(currentScale);

            var conditions = new MagneticField3D(x / scaling, y / scaling, z / scaling, MagneticField.UnitType.Gauss);

            return Task.FromResult(conditions);
        }

        /// <summary>
        /// Reads raw magnetometer data
        /// </summary>
        /// <returns>A tuple containing the X, Y, and Z values of the magnetometer.</returns>
        (short x, short y, short z) ReadMagnetometerRaw()
        {
            Span<byte> rawData = stackalloc byte[6];
            i2cComms.ReadRegister(OUTX_L_REG, rawData);

            short x = BitConverter.ToInt16(rawData.Slice(0, 2));
            short y = BitConverter.ToInt16(rawData.Slice(2, 2));
            short z = BitConverter.ToInt16(rawData.Slice(4, 2));

            return (x, y, z);
        }

        /// <summary>
        /// Gets the full scale range of the magnetometer.
        /// </summary>
        /// <returns>The full scale range as a <see cref="FullScale"/> enum.</returns>
        public FullScale GetFullScale()
        {
            byte scaleByte = i2cComms.ReadRegister(CTRL_REG2);
            currentScale = (FullScale)(scaleByte & 0x60);
            return currentScale;
        }

        /// <summary>
        /// Sets the full scale range of the magnetometer.
        /// </summary>
        /// <param name="scale">The desired full scale range as a <see cref="FullScale"/> enum.</param>
        public void SetFullScale(FullScale scale)
        {
            byte scaleByte = i2cComms.ReadRegister(CTRL_REG2);
            scaleByte &= 0x9F; // Clear bits 6:5
            scaleByte |= (byte)scale;
            i2cComms.WriteRegister(CTRL_REG2, scaleByte);
        }

        /// <summary>
        /// Gets the output data rate (ODR) of the magnetometer.
        /// </summary>
        /// <returns>The output data rate as a <see cref="OutputDataRate"/> enum.</returns>
        public OutputDataRate GetOutputDataRate()
        {
            byte odrByte = i2cComms.ReadRegister(CTRL_REG1);
            return (OutputDataRate)(odrByte & 0x1C);
        }

        /// <summary>
        /// Sets the output data rate (ODR) of the magnetometer.
        /// </summary>
        /// <param name="odr">The desired output data rate as a <see cref="OutputDataRate"/> enum.</param>
        public void SetOutputDataRate(OutputDataRate odr)
        {
            byte odrByte = i2cComms.ReadRegister(CTRL_REG1);
            odrByte &= 0xE3; // Clear bits 4:2
            odrByte |= (byte)odr;
            i2cComms.WriteRegister(CTRL_REG1, odrByte);
        }

        /// <summary>
        /// Gets the operating mode of the magnetometer.
        /// </summary>
        /// <returns>The operating mode as a <see cref="OperatingMode"/> enum.</returns>
        public OperatingMode GetOperatingMode()
        {
            byte modeByte = i2cComms.ReadRegister(CTRL_REG3);
            return (OperatingMode)(modeByte & 0x03);
        }

        /// <summary>
        /// Sets the operating mode of the magnetometer.
        /// </summary>
        /// <param name="mode">The desired operating mode as a <see cref="OperatingMode"/> enum.</param>
        public void SetOperatingMode(OperatingMode mode)
        {
            byte modeByte = i2cComms.ReadRegister(CTRL_REG3);
            modeByte &= 0xFC; // Clear bits 0 and 1
            modeByte |= (byte)mode;
            i2cComms.WriteRegister(CTRL_REG3, modeByte);
        }
        /// <summary>
        /// Gets the status of the Fast Read feature.
        /// </summary>
        /// <returns>true if Fast Read is enabled, false otherwise.</returns>
        public bool GetFastRead()
        {
            byte fastReadByte = i2cComms.ReadRegister(CTRL_REG1);
            return (fastReadByte & 0x02) == 0x02;
        }

        /// <summary>
        /// Sets the status of the Fast Read feature.
        /// </summary>
        /// <param name="enable">true to enable Fast Read, false to disable it.</param>
        public void SetFastRead(bool enable)
        {
            byte fastReadByte = i2cComms.ReadRegister(CTRL_REG1);
            if (enable)
            {
                fastReadByte |= 0x02; // Set bit 1
            }
            else
            {
                fastReadByte &= 0xFD; // Clear bit 1
            }
            i2cComms.WriteRegister(CTRL_REG1, fastReadByte);
        }

        /// <summary>
        /// Gets the status of the Block Data Update (BDU) feature.
        /// </summary>
        /// <returns>true if BDU is enabled, false otherwise.</returns>
        public bool GetBlockDataUpdate()
        {
            byte bduByte = i2cComms.ReadRegister(CTRL_REG5);
            return (bduByte & 0x40) == 0x40;
        }

        /// <summary>
        /// Sets the status of the Block Data Update (BDU) feature.
        /// </summary>
        /// <param name="enable">true to enable BDU, false to disable it.</param>
        public void SetBlockDataUpdate(bool enable)
        {
            byte bduByte = i2cComms.ReadRegister(CTRL_REG5);
            if (enable)
            {
                bduByte |= 0x40; // Set bit 6
            }
            else
            {
                bduByte &= 0x40; // Clear bit 6
            }
            i2cComms.WriteRegister(CTRL_REG5, bduByte);
        }

        /// <summary>
        /// Get the appropriate scaling value for a given scale range
        /// </summary>
        /// <param name="scale">a <see cref="FullScale"/> enumeration value</param>
        /// <returns>the typical scale factor to apply to the raw data in mGauss/LSB</returns>
        /// <exception cref="ArgumentOutOfRangeException">if <paramref name="scale"/> is not a valid <see cref="FullScale"/></exception>
        private static double GetScaleFactor(FullScale scale)
        {
            switch (scale)
            {
                case FullScale.PlusMinus4Gauss:
                    return 6842;
                case FullScale.PlusMinus8Gauss:
                    return 3421;
                case FullScale.PlusMinus12Gauss:
                    return 2281;
                case FullScale.PlusMinus16Gauss:
                    return 1711;
                default:
                    throw new ArgumentOutOfRangeException(nameof(scale));
            }
        }

        /// <summary>
        /// Gets the status of the temperature sensor feature.
        /// </summary>
        /// <returns>true if temperature sensor is enabled, false otherwise.</returns>
        public bool GetTemperatureSensorEnable()
        {
            byte tempSenseByte = i2cComms.ReadRegister(CTRL_REG1);
            return (tempSenseByte & 0x80) == 0x80;
        }

        /// <summary>
        /// Sets the status of the temperature sensor feature.
        /// </summary>
        /// <param name="enable">true to enable the temperature sensor, false to disable it.</param>
        public void SetTemperatureSensorEnable(bool enable)
        {
            byte tempSenseByte = i2cComms.ReadRegister(CTRL_REG1);
            if (enable)
            {
                tempSenseByte |= 0x80; // Set bit 7
            }
            else
            {
                tempSenseByte &= 0x7F; // Clear bit 7
            }
            i2cComms.WriteRegister(CTRL_REG1, tempSenseByte);
        }

        /// <summary>
        /// Reads raw temperature data
        /// </summary>
        /// <returns>Raw temperature reported by the magnetometer.</returns>
        short ReadTemperatureRaw()
        {
            Span<byte> rawData = stackalloc byte[2];
            i2cComms.ReadRegister(TEMP_L_REG, rawData);

            short t = BitConverter.ToInt16(rawData);

            return t;
        }

        /// <summary>
        /// Reads temperature data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected Task<Units.Temperature> ReadTemperature()
        {
            var raw = ReadTemperatureRaw();

            var temperature = new Units.Temperature(raw / 8.0, Units.Temperature.UnitType.Celsius);

            return Task.FromResult(temperature);
        }

    }
}