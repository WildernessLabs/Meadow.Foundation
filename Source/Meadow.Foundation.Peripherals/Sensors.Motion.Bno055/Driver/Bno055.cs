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
    // Sample Reading:
    // Accel: [X:0.00,Y:-1.15,Z:10.09 (m/s^2)]
    // Gyro: [X:0.00,Y:-0.06,Z:0.06 (degrees/s)]
    // Compass: [X:19.38,Y:-36.75,Z:-118.25 (Tesla)]
    // Gravity: [X:0.00, Y:-1.12, Z:9.74 (meters/s^2)]
    // Quaternion orientation: [X:-0.06, Y:0.00, Z:0.00]
    // Euler orientation: [heading: 0.00, Roll: 0.00, Pitch: 0.12]
    // Linear Accel: [X:0.00, Y:-0.03, Z:0.35 (meters/s^2)]
    // Temp: 33.00C

    //TODO: the sensor works great as is right now, but there's some room for
    // improvement. Currently, we basically turn it on full bore and get all
    // the readings.
    // However, there's an opportunity here to allow users to selectively turn
    // on the various features, and then in the `ReadSensor()` method we can
    // check before we do the reading to see what the user has turned on.
    // if the feature is turned on, we can read and parse the registers and then
    // set the `Conditions.[X] = reading`, otherwise set them to `null`, since
    // all the conditions are nullable. this would provide folks with an
    // opportunity to use the sensor in a low or lower-power configuration.

    /// <summary>
    ///     Provide methods / properties to allow an application to control a BNO055 
    ///     9-axis absolute orientation sensor.
    /// </summary>
    /// <remarks>
    ///     By defult the sensor will start with the following configuration:
    /// 
    ///     Range           Range       Bandwidth
    ///     Accelerometer   4G          62.5 Hz
    ///     Magnetometer    N/A         10 Hz
    ///     Gyroscope       2000 dps    32 Hz
    /// </remarks>
    public partial class Bno055 : ByteCommsSensorBase<(
        Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D,
        MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
        Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
        EulerAngles? EulerOrientation, Units.Temperature? Temperature)>,
        IAccelerometer, IGyroscope, ITemperatureSensor
    {
        //==== events
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = delegate { };
        public event EventHandler<IChangeResult<AngularVelocity3D>> AngularVelocity3DUpdated = delegate { };
        public event EventHandler<IChangeResult<MagneticField3D>> MagneticField3DUpdated = delegate { };
        public event EventHandler<IChangeResult<Quaternion>> QuaternionOrientationUpdated = delegate { };
        public event EventHandler<IChangeResult<Acceleration3D>> LinearAccelerationUpdated = delegate { };
        public event EventHandler<IChangeResult<Acceleration3D>> GravityVectorUpdated = delegate { };
        public event EventHandler<IChangeResult<EulerAngles>> EulerOrientationUpdated = delegate { };
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        //==== internals

        //==== properties
        public Acceleration3D? Acceleration3D => Conditions.Acceleration3D;
        public AngularVelocity3D? AngularVelocity3D => Conditions.AngularVelocity3D;
        public MagneticField3D? MagneticField3D => Conditions.MagneticField3D;
        public Quaternion? QuaternionOrientation => Conditions.QuaternionOrientation;
        public Acceleration3D? LinearAcceleration => Conditions.LinearAcceleration;
        public Acceleration3D? GravityVector => Conditions.GravityVector;
        public EulerAngles? EulerOrientation => Conditions.EulerOrientation;
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        ///     Select the source of the Temperatute property.
        /// </summary>
        public Sensor TemperatureSource
        {
            get
            {
                return (Sensor)Peripheral.ReadRegister(Registers.TemperatureSource);
            }
            set
            {
                if ((value == Sensor.Accelerometer) || (value == Sensor.Gyroscope))
                {
                    Peripheral.WriteRegister(Registers.TemperatureSource, (byte)value);
                }
                else
                {
                    throw new ArgumentException("Invlid sensor type, temperature can only be read from the Accelerometer or the Gyroscope.");
                }
            }
        }

        /// <summary>
        ///     Get or set the power mode for the sensor.
        /// </summary>
	    public byte PowerMode
        {
            get
            {
                return (Peripheral.ReadRegister(Registers.PowerMode));
            }
            set
            {
                Peripheral.WriteRegister(Registers.PowerMode, value);
                Thread.Sleep(15);
            }
        }

        /// <summary>
        ///     Get / set the current operating mode for the sensor.
        /// </summary>
        /// <remarks>
        ///     Mode change takes 7-19 ms.
        /// </remarks>
	    public byte OperatingMode
        {
            get
            {
                return (Peripheral.ReadRegister(Registers.OperatingMode));
            }
            set
            {
                if (value > OperatingModes.MAXIMUM_VALUE)
                {
                    throw new ArgumentOutOfRangeException();
                }
                Peripheral.WriteRegister(Registers.OperatingMode, value);
                Thread.Sleep(20);
            }
        }

        /// <summary>
        ///     Get / set the register page.  Page 1 contains a number of configuration registers.
        ///     Page 0 contains the sensor information.
        /// </summary>
        /// <remarks>
        ///     Most of the operating in this class are on the sensor data.  It is therefore
        ///     crucial that the sensor is left accessing Page 0.  Methods / properties that
        ///     require access to the registers in Page 1 should change to Page 1, complete
        ///     the work and then return the system back to Page 0.
        /// </remarks>
	    private byte Page
        {
            get
            {
                return Peripheral.ReadRegister(Registers.PageID);
            }
            set
            {
                if ((value != 0) && (value != 1))
                {
                    throw new ArgumentOutOfRangeException();
                }
                Peripheral.WriteRegister(Registers.PageID, value);
            }
        }

        /// <summary>
        ///     Check if sensor is currently working in Fusion mode.
        /// </summary>
	    public bool IsInFusionMode
        {
            get
            {
                return ((OperatingMode == OperatingModes.COMPASS) ||
                        (OperatingMode == OperatingModes.MAGNET_FOR_GYROSCOPE) ||
                        (OperatingMode == OperatingModes.NINE_DEGREES_OF_FREEDOM) ||
                        (OperatingMode == OperatingModes.INERTIAL_MEASUREMENT_UNIT) ||
                        (OperatingMode == OperatingModes.NINE_DEGREES_OF_FREEDOM));
            }
        }

        /// <summary>
        ///     Get the system calibration status.
        /// </summary>
        public bool IsSystemCalibrated
        {
            get
            {
                return (((Peripheral.ReadRegister(Registers.CalibrationStatus) >> 6) & 0x03) != 0);
            }
        }

        /// <summary>
        ///     Get the accelerometer calibration status.
        /// </summary>
        public bool IsAccelerometerCalibrated
        {
            get
            {
                return (((Peripheral.ReadRegister(Registers.CalibrationStatus) >> 2) & 0x03) != 0);
            }
        }

        /// <summary>
        ///     Get the gyroscope calibration status.
        /// </summary>
        public bool IsGyroscopeCalibrated
        {
            get
            {
                return (((Peripheral.ReadRegister(Registers.CalibrationStatus) >> 4) & 0x03) != 0);
            }
        }

        /// <summary>
        ///     Get the magnetometer status.
        /// </summary>
        public bool IsMagnetometerCalibrated
        {
            get { return ((Peripheral.ReadRegister(Registers.CalibrationStatus) & 0x03) != 0); }
        }

        /// <summary>
        ///     Is the system fully calibrated?
        /// </summary>
        /// <remarks>
        ///     The sensor is fully calibrated if the system, accelerometer, gyroscope and megnetometer
        ///     are all calibrated.
        /// </remarks>
        public bool IsFullyCalibrated
        {
            get
            {
                return (IsAccelerometerCalibrated && IsGyroscopeCalibrated && IsSystemCalibrated &&
                        IsMagnetometerCalibrated);
            }
        }

        /// <summary>
        ///     Create a new BNO055 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the BNO055 (default = 0x28).</param>
        /// <param name="i2cBus">I2C bus (default = 400 KHz).</param>
        public Bno055(II2cBus i2cBus, Addresses address = Addresses.Default)
            : this(i2cBus, (byte)address)
        {

        }

        /// <summary>
        ///     Create a new BNO055 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the BNO055 (default = 0x28).</param>
        /// <param name="i2cBus">I2C bus (default = 400 KHz).</param>
        public Bno055(II2cBus i2cBus, byte address)
            : base(i2cBus, address, readBufferSize: 256)
        {
            if (Peripheral.ReadRegister(Registers.ChipID) != 0xa0)
            {
                throw new Exception("Sensor ID should be 0xa0.");
            }

        }

        public override void StartUpdating(TimeSpan? updateInterval = null)
        {
            // set up to run
            PowerMode = PowerModes.NORMAL;
            OperatingMode = OperatingModes.NINE_DEGREES_OF_FREEDOM;
            base.StartUpdating(updateInterval);
        }

        public override void StopUpdating()
        {
            PowerMode = PowerModes.SUSPENDED;
            base.StopUpdating();
        }

        protected override Task<
            (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D,
            MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
            Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
            EulerAngles? EulerOrientation, Units.Temperature? Temperature)> ReadSensor()
        {
            return Task.Run(() =>
            {
                if (PowerMode != PowerModes.NORMAL)
                {
                    PowerMode = PowerModes.NORMAL;
                }

                if (OperatingMode != OperatingModes.NINE_DEGREES_OF_FREEDOM)
                {
                    OperatingMode = OperatingModes.NINE_DEGREES_OF_FREEDOM;
                }

                // The amazing Octple!
                (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D,
                MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
                Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
                EulerAngles? EulerOrientation, Units.Temperature? Temperature) conditions;

                // make all the readings
                // 	This method reads ony the sensor motion / orientation registers.  When
                // 	accessing the data from a register it is necessary to subtract the
                // 	access of the start of the sensor registers from the register required
                // 	in order to get the correct offset into the _sensorReadings array.

                int length = Registers.GravityVectorZMSB + 1 - Registers.AccelerometerXLSB;
                Peripheral.ReadRegister(Registers.AccelerometerXLSB, ReadBuffer.Span[0..length]);

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

                //---- Quarternion Orientation
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
                conditions.Temperature = new Units.Temperature(Peripheral.ReadRegister(Registers.Temperature), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }

        protected override void RaiseEventsAndNotify(IChangeResult<
            (Acceleration3D? Acceleration3D, AngularVelocity3D? AngularVelocity3D,
            MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
            Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
            EulerAngles? EulerOrientation, Units.Temperature? Temperature)> changeResult)
        {
            if (changeResult.New.Acceleration3D is { } accel)
            {
                Acceleration3DUpdated?.Invoke(this, new ChangeResult<Acceleration3D>(accel, changeResult.Old?.Acceleration3D));
            }
            if (changeResult.New.AngularVelocity3D is { } angular)
            {
                AngularVelocity3DUpdated?.Invoke(this, new ChangeResult<AngularVelocity3D>(angular, changeResult.Old?.AngularVelocity3D));
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
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            base.RaiseEventsAndNotify(changeResult);
        }

        /// <summary>
        ///     Convert a section of the sensor data into a tuple.
        /// </summary>
        /// <param name="start">Start of the data in the _sensorReadings member variable.</param>
        protected (double X, double Y, double Z) GetReadings(int start, double divisor)
        {
            var x = (short)((ReadBuffer.Span[start + 1] << 8) | ReadBuffer.Span[start]);
            var y = (short)((ReadBuffer.Span[start + 3] << 8) | ReadBuffer.Span[start + 2]);
            var z = (short)((ReadBuffer.Span[start + 5] << 8) | ReadBuffer.Span[start + 4]);

            return (x / divisor, y / divisor, z / divisor);
        }

        /// <summary>
        ///     Convert the sensor readings into an orientation in Euler angles.
        /// </summary>
        /// <param name="start">First of the sensor readings to convert.</param>
        /// <param name="divisor">Divisor to apply to the sensor data.</param>
        /// <returns>EulerAngles object containing the orientation informaiton.</returns>
        protected EulerAngles ConvertReadingToEulerAngles(int start, double divisor)
        {
            var x = (short)((ReadBuffer.Span[start + 1] << 8) | ReadBuffer.Span[start]);
            var y = (short)((ReadBuffer.Span[start + 3] << 8) | ReadBuffer.Span[start + 2]);
            var z = (short)((ReadBuffer.Span[start + 5] << 8) | ReadBuffer.Span[start + 4]);
            return new EulerAngles(new Angle(x / divisor, Angle.UnitType.Radians), new Angle(y / divisor, Angle.UnitType.Radians), new Angle(z / divisor, Angle.UnitType.Radians));
        }

        /// <summary>
        ///     Read all of the registers and display their values on the Debug output.
        /// </summary>
	    public void DisplayRegisters()
        {
            Console.WriteLine("== REGISTERS ========================================================================");

            int length = 0x6A;
            byte[] buffer = new byte[length];

            Peripheral.ReadRegister(Registers.ChipID, buffer);
            DebugInformation.DisplayRegisters(0x00, buffer);

            Console.WriteLine("== /REGISTERS =======================================================================");
        }
    }
}