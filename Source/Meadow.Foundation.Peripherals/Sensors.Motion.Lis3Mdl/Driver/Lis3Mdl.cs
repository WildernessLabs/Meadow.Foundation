using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Represents a Lis3mdl, a low-power, high-performance 3-axis magnetometer from STMicroelectronics
    /// with a selectable full range of ±4 to ±16 gauss and a 16-bit resolution
    /// </summary>
    public partial class Lis3mdl
        : PollingSensorBase<MagneticField3D>, IMagnetometer, II2cPeripheral
    {
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
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        // Local copies of sensor configuration
        private FullScale currentFullScale = FullScale.PlusMinus4Gauss;
        private OutputDataRate currentDataRate = OutputDataRate.Odr10Hz;

        /// <summary>
        /// Create a new instance of an Lis3mdl 3D magnetometer sensor.
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the sensor</param>
        /// <param name="address">The I2C address</param>
        /// <param name="fullScale">default <see cref="FullScale"/> enumeration value to use during initialization.</param>
        public Lis3mdl(II2cBus i2cBus, byte address = (byte)Addresses.Default, FullScale fullScale = FullScale.PlusMinus4Gauss, OutputDataRate outputDataRate = OutputDataRate.Odr10Hz)
        {
            i2cComms = new I2cCommunications(i2cBus, address);
            currentFullScale = fullScale;
            currentDataRate = outputDataRate;
            Initialize();
        }

        /// <summary>
        /// Initializes the Lis3mdl sensor
        /// </summary>
        void Initialize()
        {
            // Configure the device
            i2cComms.WriteRegister(CTRL_REG1, (byte)currentDataRate); // Temperature sensor: Off, Output Data Rate: 10Hz
            i2cComms.WriteRegister(CTRL_REG2, (byte)currentFullScale); // Full Scale: as configured, other values default
            i2cComms.WriteRegister(CTRL_REG3, 0x00); // Normal Power, Continuous conversion mode
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
            MagneticField3D conditions;
            var mag = ReadMagnetometerRaw();

            conditions = GetMagneticField3D(mag.x, mag.y, mag.z);

            return Task.FromResult(conditions);
        }

        MagneticField3D GetMagneticField3D(short rawX, short rawY, short rawZ)
        {
            // Get the appropriate scale factor
            var scaling = currentFullScale switch
            {
                FullScale.PlusMinus4Gauss => 6842.0,
                FullScale.PlusMinus8Gauss => 3421.0,
                FullScale.PlusMinus12Gauss => 2281.0,
                FullScale.PlusMinus16Gauss => 1711.0,
                _ => throw new NotImplementedException(),
            };

            var x = rawX / scaling;
            var y = rawY / scaling;
            var z = rawZ / scaling;

            return new MagneticField3D(x, y, z, MagneticField.UnitType.Gauss);
        }

        /// <summary>
        /// Reads raw magnetometer data
        /// </summary>
        /// <returns>A tuple containing the X, Y, and Z values of the magnetometer.</returns>
        (short x, short y, short z) ReadMagnetometerRaw()
        {
            Span<byte> rawData = stackalloc byte[6];
            i2cComms.ReadRegister(OUT_X_L, rawData);

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
            byte registerValue = i2cComms.ReadRegister(CTRL_REG2);
            currentFullScale = (FullScale)(registerValue & 0x60);
            return currentFullScale;
        }

        /// <summary>
        /// Sets the full scale range of the magnetometer.
        /// </summary>
        /// <param name="scale">The desired full scale range as a <see cref="FullScale"/> enum.</param>
        public void SetFullScale(FullScale scale)
        {
            byte registerValue = i2cComms.ReadRegister(CTRL_REG2);
            registerValue &= 0x9F; // Clear bits 6:5
            registerValue |= (byte)scale;
            i2cComms.WriteRegister(CTRL_REG2, registerValue);
        }

        /// <summary>
        /// Gets the output data rate (ODR) of the magnetometer.
        /// </summary>
        /// <returns>The output data rate as a <see cref="OutputDataRate"/> enum.</returns>
        public OutputDataRate GetOutputDataRate()
        {
            byte registerValue = i2cComms.ReadRegister(CTRL_REG1);
            return (OutputDataRate)(registerValue & 0x1C);
        }

        /// <summary>
        /// Sets the output data rate (ODR) of the magnetometer.
        /// </summary>
        /// <param name="odr">The desired output data rate as a <see cref="OutputDataRate"/> enum.</param>
        public void SetOutputDataRate(OutputDataRate odr)
        {
            byte registerValue = i2cComms.ReadRegister(CTRL_REG1);
            registerValue &= 0xE3; // Clear bits 4:2
            registerValue |= (byte)odr;
            i2cComms.WriteRegister(CTRL_REG1, registerValue);
        }

        /// <summary>
        /// Gets the operating mode of the magnetometer.
        /// </summary>
        /// <returns>The operating mode as a <see cref="OperatingMode"/> enum.</returns>
        public OperatingMode GetOperatingMode()
        {
            byte registerValue = i2cComms.ReadRegister(CTRL_REG3);
            return (OperatingMode)(registerValue & 0x03);
        }

        /// <summary>
        /// Sets the operating mode of the magnetometer.
        /// </summary>
        /// <param name="mode">The desired operating mode as a <see cref="OperatingMode"/> enum.</param>
        public void SetOperatingMode(OperatingMode mode)
        {
            byte registerValue = i2cComms.ReadRegister(CTRL_REG3);
            registerValue &= 0xFC; // Clear bits 0 and 1
            registerValue |= (byte)mode;
            i2cComms.WriteRegister(CTRL_REG3, registerValue);
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
            byte registerValue = i2cComms.ReadRegister(CTRL_REG1);
            if (enable)
            {
                registerValue |= 0x02; // Set bit 1
            }
            else
            {
                registerValue &= 0xFD; // Clear bit 1
            }
            i2cComms.WriteRegister(CTRL_REG1, registerValue);
        }

        /// <summary>
        /// Gets the status of the Block Data Update (BDU) feature.
        /// </summary>
        /// <returns>true if BDU is enabled, false otherwise.</returns>
        public bool GetBlockDataUpdate()
        {
            byte registerValue = i2cComms.ReadRegister(CTRL_REG5);
            return (registerValue & 0x40) == 0x40;
        }

        /// <summary>
        /// Sets the status of the Block Data Update (BDU) feature.
        /// </summary>
        /// <param name="enable">true to enable BDU, false to disable it.</param>
        public void SetBlockDataUpdate(bool enable)
        {
            byte registerValue = i2cComms.ReadRegister(CTRL_REG5);
            if (enable)
            {
                registerValue |= 0x40; // Set bit 6
            }
            else
            {
                registerValue &= 0xBF; // Clear bit 6
            }
            i2cComms.WriteRegister(CTRL_REG5, registerValue);
        }

        /// <summary>
        /// Gets the status of the temperature sensor feature.
        /// </summary>
        /// <returns>true if temperature sensor is enabled, false otherwise.</returns>
        public bool GetTemperatureSensorEnable()
        {
            byte registerValue = i2cComms.ReadRegister(CTRL_REG1);
            return (registerValue & 0x80) == 0x80;
        }

        /// <summary>
        /// Sets the status of the temperature sensor feature.
        /// </summary>
        /// <param name="enable">true to enable the temperature sensor, false to disable it.</param>
        public void SetTemperatureSensorEnable(bool enable)
        {
            byte registerValue = i2cComms.ReadRegister(CTRL_REG1);
            if (enable)
            {
                registerValue |= 0x80; // Set bit 7
            }
            else
            {
                registerValue &= 0x7F; // Clear bit 7
            }
            i2cComms.WriteRegister(CTRL_REG1, registerValue);
        }

        async Task<MagneticField3D> ISensor<MagneticField3D>.Read() => await Read();
    }
}