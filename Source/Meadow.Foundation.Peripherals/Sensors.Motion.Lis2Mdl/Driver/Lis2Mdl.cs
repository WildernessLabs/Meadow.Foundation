using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Accelerometers
{
    /// <summary>
    /// Represents a LIS2MDL is a low-power, high-performance 3-axis magnetometer from STMicroelectronics
    /// with a fixed full range of ±50 gauss and a 16-bit resolution
    /// </summary>
    public partial class Lis2Mdl : PollingSensorBase<MagneticField3D>, IMagnetometer
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
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        /// <summary>
        /// Create a new Lis2Mdl instance
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the sensor</param>
        /// <param name="address">The I2C address</param>
        public Lis2Mdl(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cComms = new I2cCommunications(i2cBus, address);

            Initialize();
        }

        /// <summary>
        /// Initializes the LSM303AGR sensor
        /// </summary>
        void Initialize()
        {
            // Configure the device
            i2cComms.WriteRegister(CTRL_REG1, 0x10); // Temperature compensation: ON, ODR: 10Hz, Mode: Continuous
            i2cComms.WriteRegister(CTRL_REG2, 0x00); // Full-scale: ±50 Gauss
            i2cComms.WriteRegister(CTRL_REG3, 0x00); // Continuous mode
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
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

            var conditions = new MagneticField3D(x / 1500.0, y / 1500.0, z / 1500.0, MagneticField.UnitType.Gauss);

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
        /// Gets the output data rate (ODR) of the magnetometer.
        /// </summary>
        /// <returns>The output data rate as a <see cref="OutputDataRate"/> enum.</returns>
        public OutputDataRate GetOutputDataRate()
        {
            byte odrByte = i2cComms.ReadRegister(CTRL_REG1);
            return (OutputDataRate)(odrByte & 0x0C);
        }

        /// <summary>
        /// Sets the output data rate (ODR) of the magnetometer.
        /// </summary>
        /// <param name="odr">The desired output data rate as a <see cref="OutputDataRate"/> enum.</param>
        public void SetOutputDataRate(OutputDataRate odr)
        {
            byte odrByte = i2cComms.ReadRegister(CTRL_REG1);
            odrByte &= 0xF3; // Clear bits 2 and 3
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
        /// Gets the status of the temperature compensation feature.
        /// </summary>
        /// <returns>true if temperature compensation is enabled, false otherwise.</returns>
        public bool GetTemperatureCompensation()
        {
            byte tempCompByte = i2cComms.ReadRegister(CTRL_REG1);
            return (tempCompByte & 0x80) == 0x80;
        }

        /// <summary>
        /// Sets the status of the temperature compensation feature.
        /// </summary>
        /// <param name="enable">true to enable temperature compensation, false to disable it.</param>
        public void SetTemperatureCompensation(bool enable)
        {
            byte tempCompByte = i2cComms.ReadRegister(CTRL_REG1);
            if (enable)
            {
                tempCompByte |= 0x80; // Set bit 7
            }
            else
            {
                tempCompByte &= 0x7F; // Clear bit 7
            }
            i2cComms.WriteRegister(CTRL_REG1, tempCompByte);
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
            byte bduByte = i2cComms.ReadRegister(CTRL_REG1);
            return (bduByte & 0x01) == 0x01;
        }

        /// <summary>
        /// Sets the status of the Block Data Update (BDU) feature.
        /// </summary>
        /// <param name="enable">true to enable BDU, false to disable it.</param>
        public void SetBlockDataUpdate(bool enable)
        {
            byte bduByte = i2cComms.ReadRegister(CTRL_REG1);
            if (enable)
            {
                bduByte |= 0x01; // Set bit 0
            }
            else
            {
                bduByte &= 0xFE; // Clear bit 0
            }
            i2cComms.WriteRegister(CTRL_REG1, bduByte);
        }
    }
}