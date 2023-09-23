using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Accelerometers
{
    /// <summary>
    /// Represents a LSM6dsox is a system-in-package (SiP) that combines a 3D linear acceleration sensor and a 3D gyroscope sensor
    /// </summary>
    public partial class Lsm6dsox :
        PollingSensorBase<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D)>,
        IGyroscope, IAccelerometer, II2cPeripheral
    {
        /// <summary>
        /// Event raised when acceleration changes
        /// </summary>
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = delegate { };

        /// <summary>
        /// Event raised when magnetic field changes
        /// </summary>
        public event EventHandler<IChangeResult<AngularVelocity3D>> AngularVelocity3DUpdated = delegate { };

        /// <summary>
        /// Current Acceleration 3D
        /// </summary>
        public Acceleration3D? Acceleration3D => Conditions.Acceleration3D;

        /// <summary>
        /// Current Magnetic Field 3D
        /// </summary>
        public AngularVelocity3D? AngularVelocity3D => Conditions.AngularVelocity3D;

        /// <inheritdoc/>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// I2C Communication bus used to communicate with the IMU
        /// </summary>
        readonly II2cCommunications i2cComms;

        // Local cached copy of the current scaling setting.
        private AccelFullScale currentAccelScale;
        private GyroFullScale currentGyroScale;

        /// <summary>
        /// Create a new Lsm6dsox instance
        /// </summary>
        /// <param name="i2cBus">The I2C bus connected to the sensor</param>
        /// <param name="address">The I2C address</param>
        public Lsm6dsox(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cComms = new I2cCommunications(i2cBus, (byte)address);

            Initialize();
        }

        /// <summary>
        /// Initializes the LSM6dsox sensor
        /// </summary>
        void Initialize()
        {
            i2cComms.WriteRegister(CTRL1_XL, 0x44); // 104 Hz, 16g scale
            i2cComms.WriteRegister(CTRL2_G, 0x4C);  // 104 Hz, 2000°/s scale
            currentAccelScale = AccelFullScale.G16;
            currentGyroScale = GyroFullScale.Dps2000;
        }

        /// <summary>
        /// Sets the full scale of the accelerometer
        /// </summary>
        /// <param name="fullScale">The desired full scale setting, specified by the <see cref="AccelFullScale"/> enum.</param>
        public void SetAccelerometerFullScale(AccelFullScale fullScale)
        {
            var scaleByte = i2cComms.ReadRegister(CTRL1_XL);
            scaleByte &= 0xF3; // Clear bits 3:2
            scaleByte |= (byte)fullScale;
            i2cComms.Write(scaleByte);
            currentAccelScale = fullScale;
        }

        /// <summary>
        /// Retrieves the current full scale setting of the accelerometer
        /// </summary>
        /// <returns>The current full scale setting, represented by the <see cref="AccelFullScale"/> enum.</returns>
        public AccelFullScale GetAccelerometerFullScale()
        {
            byte scaleByte = i2cComms.ReadRegister(CTRL1_XL);
            currentAccelScale = (AccelFullScale)(scaleByte & 0x0C);
            return currentAccelScale;
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D)> changeResult)
        {
            if (changeResult.New.AngularVelocity3D is { } mag)
            {
                AngularVelocity3DUpdated?.Invoke(this, new ChangeResult<AngularVelocity3D>(mag, changeResult.Old?.AngularVelocity3D));
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
        protected override Task<(Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D)> ReadSensor()
        {
            (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D) conditions;

            var accel = ReadAccelerometerRaw();
            var gyro = ReadGyroscopeRaw();

            conditions.Acceleration3D = GetAcceleration3D(accel.x, accel.y, accel.z);
            conditions.AngularVelocity3D = GetAngularVelocity3D(gyro.x, gyro.y, gyro.z);

            return Task.FromResult(conditions);
        }

        Acceleration3D GetAcceleration3D(short rawX, short rawY, short rawZ)
        {
            float lsbPerG = 0;
            switch (currentAccelScale)
            {
                case AccelFullScale.G2:
                    lsbPerG = 16384.0f; // 2^16 / (2 * 2)
                    break;
                case AccelFullScale.G4:
                    lsbPerG = 8192.0f; // 2^16 / (2 * 4)
                    break;
                case AccelFullScale.G8:
                    lsbPerG = 4096.0f; // 2^16 / (2 * 8)
                    break;
                case AccelFullScale.G16:
                    lsbPerG = 2048.0f; // 2^16 / (2 * 16)
                    break;
            }

            float x = rawX / lsbPerG;
            float y = rawY / lsbPerG;
            float z = rawZ / lsbPerG;

            return new Acceleration3D(x, y, z, Acceleration.UnitType.Gravity);
        }

        AngularVelocity3D GetAngularVelocity3D(short rawX, short rawY, short rawZ)
        {
            float lsbPerDps = 0;
            switch (currentGyroScale)
            {
                case GyroFullScale.Dps125:
                    lsbPerDps = 262.144f; // 2^16 / (2 * 125)
                    break;
                case GyroFullScale.Dps250:
                    lsbPerDps = 131.072f; // 2^16 / (2 * 250)
                    break;
                case GyroFullScale.Dps500:
                    lsbPerDps = 65.536f; // 2^16 / (2 * 500)
                    break;
                case GyroFullScale.Dps1000:
                    lsbPerDps = 32.768f; // 2^16 / (2 * 1000)
                    break;
                case GyroFullScale.Dps2000:
                    lsbPerDps = 16.384f; // 2^16 / (2 * 2000)
                    break;
            }

            var x = rawX / lsbPerDps;
            var y = rawY / lsbPerDps;
            var z = rawZ / lsbPerDps;

            return new AngularVelocity3D(x, y, z, AngularVelocity.UnitType.DegreesPerSecond);
        }

        /// <summary>
        /// Reads raw accelerometer data
        /// </summary>
        /// <returns>A tuple containing the X, Y, and Z values of the accelerometer.</returns>
        (short x, short y, short z) ReadAccelerometerRaw()
        {
            byte[] readBuffer = new byte[6];
            i2cComms.ReadRegister(OUTX_L_A, readBuffer);

            short x = BitConverter.ToInt16(new byte[] { readBuffer[0], readBuffer[1] }, 0);
            short y = BitConverter.ToInt16(new byte[] { readBuffer[2], readBuffer[3] }, 0);
            short z = BitConverter.ToInt16(new byte[] { readBuffer[4], readBuffer[5] }, 0);

            return (x, y, z);
        }

        /// <summary>
        /// Reads raw gyroscope data
        /// </summary>
        /// <returns>A tuple containing the X, Y, and Z values of the gyroscope.</returns>
        (short x, short y, short z) ReadGyroscopeRaw()
        {
            byte[] readBuffer = new byte[6];
            i2cComms.ReadRegister(OUTX_L_G, readBuffer);

            short x = BitConverter.ToInt16(new byte[] { readBuffer[0], readBuffer[1] }, 0);
            short y = BitConverter.ToInt16(new byte[] { readBuffer[2], readBuffer[3] }, 0);
            short z = BitConverter.ToInt16(new byte[] { readBuffer[4], readBuffer[5] }, 0);

            return (x, y, z);
        }

        /// <summary>
        /// Sets the output data rate for the accelerometer.
        /// </summary>
        /// <param name="dataRate">The desired output data rate setting.</param>
        public void SetAccelerometerOutputDataRate(OutputDataRate dataRate)
        {
            byte odrByte = i2cComms.ReadRegister(CTRL1_XL);
            odrByte &= 0x0F; // Clear bits 7:4
            odrByte |= (byte)dataRate;
            i2cComms.WriteRegister(CTRL1_XL, odrByte);
        }

        /// <summary>
        /// Retrieves the current output data rate setting for the accelerometer.
        /// </summary>
        /// <returns>The current output data rate setting.</returns>
        public OutputDataRate GetAccelerometerOutputDataRate()
        {
            byte odrByte = i2cComms.ReadRegister(CTRL1_XL);
            return (OutputDataRate)(odrByte & 0xF0);
        }

        /// <summary>
        /// Sets the output data rate for the gyroscope.
        /// </summary>
        /// <param name="dataRate">The desired output data rate setting.</param>
        public void SetGyroscopeOutputDataRate(OutputDataRate dataRate)
        {
            byte odrByte = i2cComms.ReadRegister(CTRL2_G);
            odrByte &= 0x0F; // Clear bits 7:4
            odrByte |= (byte)dataRate;
            i2cComms.WriteRegister(CTRL2_G, odrByte);
        }

        /// <summary>
        /// Retrieves the current output data rate setting for the gyroscope.
        /// </summary>
        /// <returns>The current output data rate setting.</returns>
        public OutputDataRate GetGyroscopeOutputDataRate()
        {
            byte odrByte = i2cComms.ReadRegister(CTRL2_G);
            return (OutputDataRate)(odrByte & 0xF0);
        }

        async Task<Acceleration3D> ISensor<Acceleration3D>.Read()
        => (await Read()).Acceleration3D.Value;

        async Task<AngularVelocity3D> ISensor<AngularVelocity3D>.Read()
        => (await Read()).AngularVelocity3D.Value;
    }
}