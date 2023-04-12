using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Accelerometers
{
    /// <summary>
    /// Represents a LSM303AGR is a system-in-package (SiP) that combines a A 3D linear acceleration sensor and a A 3D magnetic sensor
    /// </summary>
    public partial class Lsm303agr :
        ByteCommsSensorBase<(Acceleration3D? Acceleration3D, MagneticField3D? MagneticField3D)>
    {
        /// <summary>
        /// Event raised when acceleration changes
        /// </summary>
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = delegate { };

        /// <summary>
        /// Event raised when magentic field changes
        /// </summary>
        public event EventHandler<IChangeResult<MagneticField3D>> MagneticField3DUpdated = delegate { };

        /// <summary>
        /// Current Acceleration 3D
        /// </summary>
        public Acceleration3D? Acceleration3D => Conditions.Acceleration3D;

        /// <summary>
        /// Current Magentic Field 3D
        /// </summary>
        public MagneticField3D? MagneticField3D => Conditions.MagneticField3D;

        readonly II2cPeripheral i2cPeripheralAccel;
        readonly II2cPeripheral i2cPeripheralMag;

        /// <summary>
        /// Create a new Lsm303agr instance
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the sensor</param>
        public Lsm303agr(II2cBus i2cBus)
        {
            i2cPeripheralAccel = new I2cPeripheral(i2cBus, (byte)Addresses.AddressAccel_0x19);
            i2cPeripheralMag = new I2cPeripheral(i2cBus, (byte)Addresses.AddressMag_0x69);

            Initialize();
        }

        /// <summary>
        /// Initializes the LSM303AGR sensor
        /// </summary>
        void Initialize()
        {
            i2cPeripheralAccel.WriteRegister(ACC_CTRL_REG1_A, 0x57);
            i2cPeripheralMag.WriteRegister(ACC_CTRL_REG1_A, 0x60);
        }

        /// <summary>
        /// Sets the sensitivity of the accelerometer.
        /// </summary>
        /// <param name="sensitivity">The desired sensitivity setting, specified by the AccSensitivity enum.</param>
        public void SetAccelerometerSensitivity(AccSensitivity sensitivity)
        {
            byte[] writeBuffer = new byte[] { ACC_CTRL_REG4_A, (byte)sensitivity };
            i2cPeripheralAccel.Write(writeBuffer);
        }

        /// <summary>
        /// Sets the sensitivity of the magnetometer.
        /// </summary>
        /// <param name="sensitivity">The desired sensitivity setting, specified by the MagSensitivity enum.</param>
        public void SetMagnetometerSensitivity(MagSensitivity sensitivity)
        {
            byte[] writeBuffer = new byte[] { MAG_CTRL_REG2_M, (byte)sensitivity };
            i2cPeripheralMag.Write(writeBuffer);
        }

        /// <summary>
        /// Retrieves the current sensitivity setting of the accelerometer.
        /// </summary>
        /// <returns>The current sensitivity setting, represented by the AccSensitivity enum.</returns>
        public AccSensitivity GetAccelerometerSensitivity()
        {
            byte[] readBuffer = new byte[1];
            i2cPeripheralAccel.ReadRegister(ACC_CTRL_REG4_A, readBuffer);
            byte sensitivity = (byte)(readBuffer[0] & 0x30);
            return (AccSensitivity)sensitivity;
        }

        /// <summary>
        /// Retrieves the current sensitivity setting of the magnetometer.
        /// </summary>
        /// <returns>The current sensitivity setting, represented by the MagSensitivity enum.</returns>
        public MagSensitivity GetMagnetometerSensitivity()
        {
            byte[] readBuffer = new byte[1];
            i2cPeripheralMag.ReadRegister(MAG_CTRL_REG2_M, readBuffer);
            byte sensitivity = (byte)(readBuffer[0] & 0x60);
            return (MagSensitivity)sensitivity;
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Acceleration3D? Acceleration3D, MagneticField3D? MagneticField3D)> changeResult)
        {
            if (changeResult.New.MagneticField3D is { } mag)
            {
                MagneticField3DUpdated?.Invoke(this, new ChangeResult<MagneticField3D>(mag, changeResult.Old?.MagneticField3D));
            }
            if (changeResult.New.Acceleration3D is { } accel)
            {
                Acceleration3DUpdated?.Invoke(this, new ChangeResult<Acceleration3D>(accel, changeResult.Old?.Acceleration3D));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<(Acceleration3D? Acceleration3D, MagneticField3D? MagneticField3D)> ReadSensor()
        {
            return Task.Run(() =>
            {
                (Acceleration3D? Acceleration3D, MagneticField3D? MagneticField3D) conditions;

                var accel = ReadAccelerometerRaw();
                var mag = ReadMagnetometerRaw();

                conditions.Acceleration3D = GetAcceleration3D(accel.x, accel.y, accel.z, GetAccelerometerSensitivity());
                conditions.MagneticField3D = GetMagneticField3D(mag.x, mag.y, mag.z, GetMagnetometerSensitivity());

                return conditions;
            });
        }

        Acceleration3D GetAcceleration3D(short rawX, short rawY, short rawZ, AccSensitivity sensitivity)
        {
            float lsbPerG = 0;
            switch (sensitivity)
            {
                case AccSensitivity.G2:
                    lsbPerG = 16384.0f; // 2^16 / (2 * 2)
                    break;
                case AccSensitivity.G4:
                    lsbPerG = 8192.0f; // 2^16 / (2 * 4)
                    break;
                case AccSensitivity.G8:
                    lsbPerG = 4096.0f; // 2^16 / (2 * 8)
                    break;
                case AccSensitivity.G16:
                    lsbPerG = 2048.0f; // 2^16 / (2 * 16)
                    break;
            }

            // Convert raw values to units of g
            float x = rawX / lsbPerG;
            float y = rawY / lsbPerG;
            float z = rawZ / lsbPerG;

            return new Acceleration3D(x, y, z, Acceleration.UnitType.Gravity);
        }

        MagneticField3D GetMagneticField3D(short rawX, short rawY, short rawZ, MagSensitivity sensitivity)
        {
            float lsbPerGauss = 0;
            switch (sensitivity)
            {
                case MagSensitivity.Gauss50:
                    lsbPerGauss = 6842.0f; // 2^15 / 50
                    break;
                case MagSensitivity.Gauss100:
                    lsbPerGauss = 3421.0f; // 2^15 / 100
                    break;
                case MagSensitivity.Gauss200:
                    lsbPerGauss = 1711.0f; // 2^15 / 200
                    break;
                case MagSensitivity.Gauss400:
                    lsbPerGauss = 855.0f; // 2^15 / 400
                    break;
            }

            // Convert raw values to units of gauss
            float x = rawX / lsbPerGauss;
            float y = rawY / lsbPerGauss;
            float z = rawZ / lsbPerGauss;

            return new MagneticField3D(x, y, z, MagneticField.UnitType.Gauss);
        }

        /// <summary>
        /// Reads raw accelerometer data.
        /// </summary>
        /// <returns>A tuple containing the X, Y, and Z values of the accelerometer.</returns>
        (short x, short y, short z) ReadAccelerometerRaw()
        {
            byte[] readBuffer = new byte[6];
            i2cPeripheralAccel.ReadRegister(ACC_OUT_X_L_A | 0x80, readBuffer);

            short x = BitConverter.ToInt16(new byte[] { readBuffer[0], readBuffer[1] }, 0);
            short y = BitConverter.ToInt16(new byte[] { readBuffer[2], readBuffer[3] }, 0);
            short z = BitConverter.ToInt16(new byte[] { readBuffer[4], readBuffer[5] }, 0);

            return (x, y, z);
        }

        /// <summary>
        /// Reads raw magnetometer data.
        /// </summary>
        /// <returns>A tuple containing the X, Y, and Z values of the magnetometer.</returns>
        (short x, short y, short z) ReadMagnetometerRaw()
        {
            byte[] readBuffer = new byte[6];
            i2cPeripheralMag.ReadRegister(MAG_OUTX_L_REG_M | 0x80, readBuffer);

            short x = BitConverter.ToInt16(new byte[] { readBuffer[0], readBuffer[1] }, 0);
            short y = BitConverter.ToInt16(new byte[] { readBuffer[2], readBuffer[3] }, 0);
            short z = BitConverter.ToInt16(new byte[] { readBuffer[4], readBuffer[5] }, 0);

            return (x, y, z);
        }


    }
}