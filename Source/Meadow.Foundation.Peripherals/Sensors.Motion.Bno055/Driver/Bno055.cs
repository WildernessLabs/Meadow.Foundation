using Meadow.Foundation.Helpers;
using Meadow.Foundation.Spatial;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    /// Provide methods / properties to allow an application to control a BNO055 
    /// 9-axis absolute orientation sensor.
    /// </summary>
    /// <remarks>
    /// By default the sensor will start with the following configuration:
    /// 
    /// Range           Range       Bandwidth
    /// Accelerometer   4G          62.5 Hz
    /// Magnetometer    N/A         10 Hz
    /// Gyroscope       2000 dps    32 Hz
    /// </remarks>
    public partial class Bno055 : ByteCommsSensorBase<(
        Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D,
        MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
        Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
        EulerAngles? EulerOrientation, Units.Temperature? Temperature)>,
        IAccelerometer, IGyroscope, ITemperatureSensor, II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Raised when the magnetic field value changes
        /// </summary>
        public event EventHandler<IChangeResult<MagneticField3D>> MagneticField3DUpdated = default!;

        /// <summary>
        /// Raised when the quaternion orientation value changes
        /// </summary>
        public event EventHandler<IChangeResult<Quaternion>> QuaternionOrientationUpdated = default!;

        /// <summary>
        /// Raised when the linear acceleration value changes
        /// </summary>
        public event EventHandler<IChangeResult<Acceleration3D>> LinearAccelerationUpdated = default!;

        /// <summary>
        /// Raised when the gravity vector acceleration value changes
        /// </summary>
        public event EventHandler<IChangeResult<Acceleration3D>> GravityVectorUpdated = default!;

        /// <summary>
        /// Raised when the euler orientation value changes
        /// </summary>
        public event EventHandler<IChangeResult<EulerAngles>> EulerOrientationUpdated = default!;

        private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers;
        private event EventHandler<IChangeResult<AngularVelocity3D>> _velocityHandlers;
        private event EventHandler<IChangeResult<Acceleration3D>> _accelerationHandlers;

        event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
        {
            add => _temperatureHandlers += value;
            remove => _temperatureHandlers -= value;
        }

        event EventHandler<IChangeResult<AngularVelocity3D>> ISamplingSensor<AngularVelocity3D>.Updated
        {
            add => _velocityHandlers += value;
            remove => _velocityHandlers -= value;
        }

        event EventHandler<IChangeResult<Acceleration3D>> ISamplingSensor<Acceleration3D>.Updated
        {
            add => _accelerationHandlers += value;
            remove => _accelerationHandlers -= value;
        }

        /// <summary>
        /// Current Acceleration
        /// </summary>
        public Acceleration3D? Acceleration3D => Conditions.Acceleration3D;

        /// <summary>
        /// Current Angular Velocity
        /// </summary>
        public AngularVelocity3D? AngularVelocity3D => Conditions.AngularVelocity3D;

        /// <summary>
        /// Current Magnetic Field
        /// </summary>
        public MagneticField3D? MagneticField3D => Conditions.MagneticField3D;

        /// <summary>
        /// Current Quaternion Orientation
        /// </summary>
        public Quaternion? QuaternionOrientation => Conditions.QuaternionOrientation;

        /// <summary>
        /// Current Linear Acceleration
        /// </summary>
        public Acceleration3D? LinearAcceleration => Conditions.LinearAcceleration;

        /// <summary>
        /// Current Gravity Vector
        /// </summary>
        public Acceleration3D? GravityVector => Conditions.GravityVector;

        /// <summary>
        /// Current Euler Orientation
        /// </summary>
        public EulerAngles? EulerOrientation => Conditions.EulerOrientation;

        /// <summary>
        /// Current Temperature value
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// Select the source of the Temperature property
        /// </summary>
        public Sensor TemperatureSource
        {
            get
            {
                return (Sensor)BusComms.ReadRegister(Registers.TemperatureSource);
            }
            set
            {
                if ((value == Sensor.Accelerometer) || (value == Sensor.Gyroscope))
                {
                    BusComms.WriteRegister(Registers.TemperatureSource, (byte)value);
                }
                else
                {
                    throw new ArgumentException("Invalid sensor type, temperature can only be read from the Accelerometer or the Gyroscope.");
                }
            }
        }

        /// <summary>
        /// Get or set the power mode for the sensor.
        /// </summary>
	    public byte PowerMode
        {
            get => BusComms.ReadRegister(Registers.PowerMode);
            set
            {
                BusComms.WriteRegister(Registers.PowerMode, value);
                Thread.Sleep(15);
            }
        }

        /// <summary>
        /// Get / set the current operating mode for the sensor.
        /// </summary>
        /// <remarks>
        /// Mode change takes 7-19 ms.
        /// </remarks>
	    public byte OperatingMode
        {
            get => BusComms.ReadRegister(Registers.OperatingMode);
            set
            {
                if (value > OperatingModes.MAXIMUM_VALUE)
                {
                    throw new ArgumentOutOfRangeException();
                }
                BusComms.WriteRegister(Registers.OperatingMode, value);
                Thread.Sleep(20);
            }
        }

        /// <summary>
        /// Get / set the register page.  Page 1 contains a number of configuration registers.
        /// Page 0 contains the sensor information.
        /// </summary>
        /// <remarks>
        /// Most of the operating in this class are on the sensor data.  It is therefore
        /// crucial that the sensor is left accessing Page 0.  Methods / properties that
        /// require access to the registers in Page 1 should change to Page 1, complete
        /// the work and then return the system back to Page 0.
        /// </remarks>
	    private byte Page
        {
            get => BusComms.ReadRegister(Registers.PageID);
            set
            {
                if ((value != 0) && (value != 1))
                {
                    throw new ArgumentOutOfRangeException();
                }
                BusComms.WriteRegister(Registers.PageID, value);
            }
        }

        /// <summary>
        /// Check if sensor is currently working in Fusion mode.
        /// </summary>
	    public bool IsInFusionMode => ((OperatingMode == OperatingModes.COMPASS) ||
                        (OperatingMode == OperatingModes.MAGNET_FOR_GYROSCOPE) ||
                        (OperatingMode == OperatingModes.NINE_DEGREES_OF_FREEDOM) ||
                        (OperatingMode == OperatingModes.INERTIAL_MEASUREMENT_UNIT) ||
                        (OperatingMode == OperatingModes.NINE_DEGREES_OF_FREEDOM));

        /// <summary>
        /// Get the system calibration status.
        /// </summary>
        public bool IsSystemCalibrated => ((BusComms.ReadRegister(Registers.CalibrationStatus) >> 6) & 0x03) != 0;

        /// <summary>
        /// Get the accelerometer calibration status.
        /// </summary>
        public bool IsAccelerometerCalibrated => ((BusComms.ReadRegister(Registers.CalibrationStatus) >> 2) & 0x03) != 0;

        /// <summary>
        /// Get the gyroscope calibration status.
        /// </summary>
        public bool IsGyroscopeCalibrated => ((BusComms.ReadRegister(Registers.CalibrationStatus) >> 4) & 0x03) != 0;

        /// <summary>
        /// Get the magnetometer status.
        /// </summary>
        public bool IsMagnetometerCalibrated => (BusComms.ReadRegister(Registers.CalibrationStatus) & 0x03) != 0;

        /// <summary>
        /// Is the system fully calibrated?
        /// </summary>
        /// <remarks>
        /// The sensor is fully calibrated if the system, accelerometer, gyroscope and magnetometer
        /// are all calibrated.
        /// </remarks>
        public bool IsFullyCalibrated => IsAccelerometerCalibrated && IsGyroscopeCalibrated && IsSystemCalibrated &&
                        IsMagnetometerCalibrated;

        /// <summary>
        /// Create a new BNO055 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the BNO055 (default = 0x28).</param>
        /// <param name="i2cBus">I2C bus (default = 400 KHz).</param>
        public Bno055(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        {
        }

        /// <summary>
        /// Create a new BNO055 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the BNO055 (default = 0x28).</param>
        /// <param name="i2cBus">I2C bus (default = 400 KHz).</param>
        public Bno055(II2cBus i2cBus, byte address)
            : base(i2cBus, address, readBufferSize: 256)
        {
            var id = BusComms.ReadRegister(Registers.ChipID);

            if (id != 0xa0)
            {
                throw new Exception($"Sensor ID should be 0xa0 (it returned 0x{id:x2}).");
            }
        }

        /// <summary>
        /// Start updating
        /// </summary>
        /// <param name="updateInterval">The time between updates</param>
        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            PowerMode = PowerModes.NORMAL;
            OperatingMode = OperatingModes.NINE_DEGREES_OF_FREEDOM;
            base.StartUpdating(updateInterval);
        }

        /// <summary>
        /// Stop reading data 
        /// </summary>
        public override void StopUpdating()
        {
            PowerMode = PowerModes.SUSPENDED;
            base.StopUpdating();
        }

        /// <summary>
        /// Reads data from the sensor
        /// </summary>
        /// <returns>The latest sensor reading</returns>
        protected override Task<
            (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D,
            MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
            Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
            EulerAngles? EulerOrientation, Units.Temperature? Temperature)> ReadSensor()
        {
            if (PowerMode != PowerModes.NORMAL)
            {
                PowerMode = PowerModes.NORMAL;
            }

            if (OperatingMode != OperatingModes.NINE_DEGREES_OF_FREEDOM)
            {
                OperatingMode = OperatingModes.NINE_DEGREES_OF_FREEDOM;
            }

            // The amazing Octuple!
            (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D,
            MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
            Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
            EulerAngles? EulerOrientation, Units.Temperature? Temperature) conditions;

            // make all the readings
            // 	This method reads only the sensor motion / orientation registers.  When
            // 	accessing the data from a register it is necessary to subtract the
            // 	access of the start of the sensor registers from the register required
            // 	in order to get the correct offset into the _sensorReadings array.

            int length = Registers.GravityVectorZMSB + 1 - Registers.AccelerometerXLSB;
            BusComms.ReadRegister(Registers.AccelerometerXLSB, ReadBuffer.Span[0..length]);

            // for debugging, you can look at the raw data:
            //DebugInformation.DisplayRegisters(0x00, ReadBuffer.Span[0..length].ToArray());

            //---- Acceleration3D
            double accelDivisor = 100.0; //m/s2
            var accelData = GetReadings(Registers.AccelerometerXLSB - Registers.StartOfSensorData, accelDivisor);
            conditions.Acceleration3D = new Acceleration3D(accelData.X, accelData.Y, accelData.Z, Acceleration.UnitType.MetersPerSecondSquared);

            //---- AngularAcceleration3D
            double angularDivisor = 900.0; //radians
            var angularData = GetReadings(Registers.GyroscopeXLSB - Registers.StartOfSensorData, angularDivisor);
            conditions.AngularVelocity3D = new AngularVelocity3D(angularData.X, angularData.Y, angularData.Z, AngularVelocity.UnitType.RadiansPerSecond);

            //---- MagneticField3D
            var magnetometerData = GetReadings(Registers.MagnetometerXLSB - Registers.StartOfSensorData, 16.0);
            conditions.MagneticField3D = new MagneticField3D(magnetometerData.X, magnetometerData.Y, magnetometerData.Z, MagneticField.UnitType.Tesla);

            //---- Quaternion Orientation
            int quaternionData = Registers.QuaternionDataWLSB - Registers.StartOfSensorData;
            short w = (short)((ReadBuffer.Span[quaternionData + 1] << 8) | ReadBuffer.Span[quaternionData]);
            short x = (short)((ReadBuffer.Span[quaternionData + 3] << 8) | ReadBuffer.Span[quaternionData + 2]);
            short y = (short)((ReadBuffer.Span[quaternionData + 5] << 8) | ReadBuffer.Span[quaternionData + 4]);
            short z = (short)((ReadBuffer.Span[quaternionData + 5] << 8) | ReadBuffer.Span[quaternionData + 4]);
            double factor = 1.0 / (1 << 14);
            conditions.QuaternionOrientation = new Quaternion(w * factor, x * factor, y * factor, z * factor);

            //---- Linear Acceleration
            double linearAccellDivisor = 100.0; //m/s2
            var linearAccelData = GetReadings(Registers.LinearAccelerationXLSB - Registers.StartOfSensorData, linearAccellDivisor);
            conditions.LinearAcceleration = new Acceleration3D(linearAccelData.X, linearAccelData.Y, linearAccelData.Z, Acceleration.UnitType.MetersPerSecondSquared);

            //---- Gravity Vector
            double gravityVectorDivisor = 100.0; //m/s2
            var gravityVectorData = GetReadings(Registers.GravityVectorXLSB - Registers.StartOfSensorData, gravityVectorDivisor);
            conditions.GravityVector = new Acceleration3D(gravityVectorData.X, gravityVectorData.Y, gravityVectorData.Z, Acceleration.UnitType.MetersPerSecondSquared);

            //---- euler
            double eulerDivisor = 900.0; //radians
            conditions.EulerOrientation = ConvertReadingToEulerAngles(Registers.EulerAngleXLSB - Registers.StartOfSensorData, eulerDivisor);

            //---- temperature
            conditions.Temperature = new Units.Temperature(BusComms.ReadRegister(Registers.Temperature), Units.Temperature.UnitType.Celsius);

            return Task.FromResult(conditions);
        }

        /// <summary>
        /// Raise events for subscribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<
            (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D,
            MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
            Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
            EulerAngles? EulerOrientation, Units.Temperature? Temperature)> changeResult)
        {
            if (changeResult.New.Acceleration3D is { } accel)
            {
                _accelerationHandlers?.Invoke(this, new ChangeResult<Acceleration3D>(accel, changeResult.Old?.Acceleration3D));
            }
            if (changeResult.New.AngularVelocity3D is { } angular)
            {
                _velocityHandlers?.Invoke(this, new ChangeResult<AngularVelocity3D>(angular, changeResult.Old?.AngularVelocity3D));
            }
            if (changeResult.New.MagneticField3D is { } magnetic)
            {
                MagneticField3DUpdated?.Invoke(this, new ChangeResult<MagneticField3D>(magnetic, changeResult.Old?.MagneticField3D));
            }
            if (changeResult.New.QuaternionOrientation is { } quaternion)
            {
                QuaternionOrientationUpdated?.Invoke(this, new ChangeResult<Quaternion>(quaternion, changeResult.Old?.QuaternionOrientation));
            }
            if (changeResult.New.LinearAcceleration is { } linear)
            {
                LinearAccelerationUpdated?.Invoke(this, new ChangeResult<Acceleration3D>(linear, changeResult.Old?.LinearAcceleration));
            }
            if (changeResult.New.GravityVector is { } gravity)
            {
                GravityVectorUpdated?.Invoke(this, new ChangeResult<Acceleration3D>(gravity, changeResult.Old?.GravityVector));
            }
            if (changeResult.New.EulerOrientation is { } euler)
            {
                EulerOrientationUpdated?.Invoke(this, new ChangeResult<EulerAngles>(euler, changeResult.Old?.EulerOrientation));
            }
            if (changeResult.New.Temperature is { } temp)
            {
                _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        /// Convert a section of the sensor data into a tuple
        /// </summary>
        /// <param name="start">Start of the data in the sensorReadings member variable</param>
        /// <param name="divisor">Divisor</param>
        protected (double X, double Y, double Z) GetReadings(int start, double divisor)
        {
            var x = (short)((ReadBuffer.Span[start + 1] << 8) | ReadBuffer.Span[start]);
            var y = (short)((ReadBuffer.Span[start + 3] << 8) | ReadBuffer.Span[start + 2]);
            var z = (short)((ReadBuffer.Span[start + 5] << 8) | ReadBuffer.Span[start + 4]);

            return (x / divisor, y / divisor, z / divisor);
        }

        /// <summary>
        /// Convert the sensor readings into an orientation in Euler angles
        /// </summary>
        /// <param name="start">First of the sensor readings to convert</param>
        /// <param name="divisor">Divisor to apply to the sensor data</param>
        /// <returns>EulerAngles object containing the orientation information</returns>
        protected EulerAngles ConvertReadingToEulerAngles(int start, double divisor)
        {
            var x = (short)((ReadBuffer.Span[start + 1] << 8) | ReadBuffer.Span[start]);
            var y = (short)((ReadBuffer.Span[start + 3] << 8) | ReadBuffer.Span[start + 2]);
            var z = (short)((ReadBuffer.Span[start + 5] << 8) | ReadBuffer.Span[start + 4]);
            return new EulerAngles(new Angle(x / divisor, Angle.UnitType.Radians), new Angle(y / divisor, Angle.UnitType.Radians), new Angle(z / divisor, Angle.UnitType.Radians));
        }

        /// <summary>
        /// Read all of the registers and display their values on the Debug output.
        /// </summary>
	    public void DisplayRegisters()
        {
            Resolver.Log.Info("== REGISTERS ========================================================================");

            int length = 0x6A;
            byte[] buffer = new byte[length];

            BusComms.ReadRegister(Registers.ChipID, buffer);
            DebugInformation.DisplayRegisters(0x00, buffer);

            Resolver.Log.Info("== /REGISTERS =======================================================================");
        }

        async Task<AngularVelocity3D> ISensor<AngularVelocity3D>.Read()
            => (await Read()).AngularVelocity3D!.Value;

        async Task<Acceleration3D> ISensor<Acceleration3D>.Read()
            => (await Read()).Acceleration3D!.Value;

        async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
            => (await Read()).Temperature!.Value;
    }
}