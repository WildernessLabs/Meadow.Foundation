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
        Acceleration3D? Acceleration3D, AngularAcceleration3D? AngularAcceleration3D,
        MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
        Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
        EulerAngles? EulerOrientation, Units.Temperature? Temperature)>,
        IAccelerometer, IAngularAccelerometer, ITemperatureSensor
    {
        //==== events
        public event EventHandler<IChangeResult<Acceleration3D>> Acceleration3DUpdated = delegate { };
        public event EventHandler<IChangeResult<AngularAcceleration3D>> AngularAcceleration3DUpdated = delegate { };
        public event EventHandler<IChangeResult<MagneticField3D>> MagneticField3DUpdated = delegate { };
        public event EventHandler<IChangeResult<Quaternion>> QuaternionOrientationUpdated = delegate { };
        public event EventHandler<IChangeResult<Acceleration3D>> LinearAccelerationUpdated = delegate { };
        public event EventHandler<IChangeResult<Acceleration3D>> GravityVectorUpdated = delegate { };
        public event EventHandler<IChangeResult<EulerAngles>> EulerOrientationUpdated = delegate { };
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        //==== internals

        /// <summary>
        ///     Sensor readings from the last time the BNO055 was polled.
        /// </summary>
        private byte[] sensorReadings;

        //==== properties
        public Acceleration3D? Acceleration3D => Conditions.Acceleration3D;
        public AngularAcceleration3D? AngularAcceleration3D => Conditions.AngularAcceleration3D;
        public MagneticField3D? MagneticField3D => Conditions.MagneticField3D;
        public Quaternion? QuaternionOrientation => Conditions.QuaternionOrientation;
        public Acceleration3D? LinearAcceleration => Conditions.LinearAcceleration;
        public Acceleration3D? GravityVector => Conditions.GravityVector;
        public EulerAngles? EulerOrientation => Conditions.EulerOrientation;
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        ///     Select the source of the Temperatute property.
        /// </summary>
        public Sensor TemperatureSource {
            get {
                return (Sensor)Peripheral.ReadRegister(Registers.TemperatureSource);
            }
            set {
                if ((value == Sensor.Accelerometer) || (value == Sensor.Gyroscope)) {
                    Peripheral.WriteRegister(Registers.TemperatureSource, (byte)value);
                } else {
                    throw new ArgumentException("Invlid sensor type, temperature can only be read from the Accelerometer or the Gyroscope.");
                }
            }
        }

        /// <summary>
        ///     Get or set the power mode for the sensor.
        /// </summary>
	    public byte PowerMode {
            get {
                return (Peripheral.ReadRegister(Registers.PowerMode));
            }
            set {
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
	    public byte OperatingMode {
            get {
                return (Peripheral.ReadRegister(Registers.OperatingMode));
            }
            set {
                if (value > OperatingModes.MAXIMUM_VALUE) {
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
	    private byte Page {
            get {
                return Peripheral.ReadRegister(Registers.PageID);
            }
            set {
                if ((value != 0) && (value != 1)) {
                    throw new ArgumentOutOfRangeException();
                }
                Peripheral.WriteRegister(Registers.PageID, value);
            }
        }

        /// <summary>
        ///     Check if sensor is currently working in Fusion mode.
        /// </summary>
	    public bool IsInFusionMode {
            get {
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
        public bool IsSystemCalibrated {
            get {
                return (((Peripheral.ReadRegister(Registers.CalibrationStatus) >> 6) & 0x03) != 0);
            }
        }

        /// <summary>
        ///     Get the accelerometer calibration status.
        /// </summary>
        public bool IsAccelerometerCalibrated {
            get {
                return (((Peripheral.ReadRegister(Registers.CalibrationStatus) >> 2) & 0x03) != 0);
            }
        }

        /// <summary>
        ///     Get the gyroscope calibration status.
        /// </summary>
        public bool IsGyroscopeCalibrated {
            get {
                return (((Peripheral.ReadRegister(Registers.CalibrationStatus) >> 4) & 0x03) != 0);
            }
        }

        /// <summary>
        ///     Get the magnetometer status.
        /// </summary>
        public bool IsMagnetometerCalibrated {
            get { return ((Peripheral.ReadRegister(Registers.CalibrationStatus) & 0x03) != 0); }
        }

        /// <summary>
        ///     Is the system fully calibrated?
        /// </summary>
        /// <remarks>
        ///     The sensor is fully calibrated if the system, accelerometer, gyroscope and megnetometer
        ///     are all calibrated.
        /// </remarks>
        public bool IsFullyCalibrated {
            get {
                return (IsAccelerometerCalibrated && IsGyroscopeCalibrated && IsSystemCalibrated &&
                        IsMagnetometerCalibrated);
            }
        }

        //==== ctors

        /// <summary>
        ///     Create a new BNO055 object using the default parameters for the component.
        /// </summary>
        /// <param name="address">Address of the BNO055 (default = 0x28).</param>
        /// <param name="i2cBus">I2C bus (default = 400 KHz).</param>
        public Bno055(II2cBus i2cBus, byte address = 0x28)
            : base(i2cBus, address, readBufferSize:256)
        {
            if (Peripheral.ReadRegister(Registers.ChipID) != 0xa0) {
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

        protected override Task<(Acceleration3D? Acceleration3D, AngularAcceleration3D? AngularAcceleration3D, MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation, Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector, EulerAngles? EulerOrientation, Units.Temperature? Temperature)> ReadSensor()
        {
            return Task.Run(() => {
                if(PowerMode != PowerModes.NORMAL) {
                    PowerMode = PowerModes.NORMAL;
                }

                if (OperatingMode != OperatingModes.NINE_DEGREES_OF_FREEDOM) {
                    OperatingMode = OperatingModes.NINE_DEGREES_OF_FREEDOM;
                }

                // The amazing Octple!
                (Acceleration3D? Acceleration3D, AngularAcceleration3D? AngularAcceleration3D,
                MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
                Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
                EulerAngles? EulerOrientation, Units.Temperature? Temperature) conditions;

                // make all the readings
                // 	This method reads ony the sensor motion / orientation registers.  When
                // 	accessing the data from a register it is necessary to subtract the
                // 	accress of the start of the sensor registers from the register required
                // 	in order to get the correct offset into the _sensorReadings array.
                sensorReadings = Peripheral.ReadRegisters(Registers.AccelerometerXLSB,
                                                        (ushort)(Registers.GravityVectorZMSB - Registers.AccelerometerXLSB));

                Console.WriteLine($"Registers.GravityVectorZMSB - Registers.AccelerometerXLSB: {Registers.GravityVectorZMSB - Registers.AccelerometerXLSB}");

                //---- Acceleration3D
                double accelDivisor = 100.0; //m/s2
                var accelData = GetReadings(Registers.AccelerometerXLSB - Registers.StartOfSensorData, accelDivisor);

                conditions.Acceleration3D = new Acceleration3D(accelData.X, accelData.Y, accelData.Z, Acceleration.UnitType.MetersPerSecondSquared);

                //---- AngularAcceleration3D
                double angularDivisor = 900.0; //radians
                var angularData = GetReadings(Registers.GyroscopeXLSB - Registers.StartOfSensorData, angularDivisor);

                conditions.AngularAcceleration3D = new AngularAcceleration3D(angularData.X, angularData.Y, angularData.Z, AngularAcceleration.UnitType.RadiansPerSecondSquared);

                //---- MagneticField3D
                var magnetometerData = GetReadings(Registers.MagnetometerXLSB - Registers.StartOfSensorData, 16.0);

                conditions.MagneticField3D = new MagneticField3D(magnetometerData.X, magnetometerData.Y, magnetometerData.Z, MagneticField.UnitType.Tesla);


                //---- Quarternion Orientation
                int quaternionData = Registers.QuaternionDataWLSB - Registers.StartOfSensorData;
                short w = (short)((sensorReadings[quaternionData + 1] << 8) | sensorReadings[quaternionData]);
                short x = (short)((sensorReadings[quaternionData + 3] << 8) | sensorReadings[quaternionData + 2]);
                short y = (short)((sensorReadings[quaternionData + 5] << 8) | sensorReadings[quaternionData + 4]);
                short z = (short)((sensorReadings[quaternionData + 5] << 8) | sensorReadings[quaternionData + 4]);
                double factor = 1.0 / (1 << 14);
                conditions.QuaternionOrientation = new Quaternion(w * factor, x * factor, y * factor, z * factor);

                //---- Linear Acceleration
                if (!IsInFusionMode) {
                    throw new InvalidOperationException("Linear accelration vectors are only available in fusion mode.");
                }
                double linearAccellDivisor = 100.0; //m/s2
                var linearAccelData = GetReadings(Registers.LinearAccelerationXLSB - Registers.StartOfSensorData, linearAccellDivisor);

                conditions.LinearAcceleration = new Acceleration3D(linearAccelData.X, linearAccelData.Y, linearAccelData.Z, Acceleration.UnitType.MetersPerSecondSquared);

                // TODO: this throws an errr
                //---- Gravity Vector
                //if (!IsInFusionMode) {
                //    throw new InvalidOperationException("Linear acceleration vectors are only available in fusion mode.");
                //}
                double gravityVectorDivisor = 100.0; //m/s2
                Console.WriteLine($"Registers.GravityVectorXLSB - Registers.StartOfSensorData: {Registers.GravityVectorXLSB - Registers.StartOfSensorData}");
                Console.WriteLine($"SensorReadings.Length: {sensorReadings.Length}");

                var gravityVectorData = GetReadings(Registers.GravityVectorXLSB - Registers.StartOfSensorData, gravityVectorDivisor);
                Console.WriteLine("Here.");
                //conditions.GravityVector = new Acceleration3D(gravityVectorData.X, gravityVectorData.Y, gravityVectorData.Z, Acceleration.UnitType.MetersPerSecondSquared);
                conditions.GravityVector = null;

                //---- euler
                double eulerDivisor = 900.0; //radians

                conditions.EulerOrientation = ConvertReadingToEulerAngles(Registers.EulerAngleXLSB - Registers.StartOfSensorData, eulerDivisor);

                //---- temperature
                conditions.Temperature = new Units.Temperature(Peripheral.ReadRegister(Registers.Temperature), Units.Temperature.UnitType.Celsius);

                return conditions;
            });
        }

        protected override void RaiseEventsAndNotify(IChangeResult<
            (Acceleration3D? Acceleration3D, AngularAcceleration3D? AngularAcceleration3D,
            MagneticField3D? MagneticField3D, Quaternion? QuaternionOrientation,
            Acceleration3D? LinearAcceleration, Acceleration3D? GravityVector,
            EulerAngles? EulerOrientation, Units.Temperature? Temperature)> changeResult)
        {
            if (changeResult.New.Acceleration3D is { } accel) {
                Acceleration3DUpdated?.Invoke(this, new ChangeResult<Acceleration3D>(accel, changeResult.Old?.Acceleration3D));
            }
            if (changeResult.New.AngularAcceleration3D is { } angular) {
                AngularAcceleration3DUpdated?.Invoke(this, new ChangeResult<AngularAcceleration3D>(angular, changeResult.Old?.AngularAcceleration3D));
            }
            if (changeResult.New.MagneticField3D is { } magnetic) {
                MagneticField3DUpdated?.Invoke(this, new ChangeResult<MagneticField3D>(magnetic, changeResult.Old?.MagneticField3D));
            }
            if (changeResult.New.QuaternionOrientation is { } quaternion) {
                QuaternionOrientationUpdated?.Invoke(this, new ChangeResult<Quaternion>(quaternion, changeResult.Old?.QuaternionOrientation));
            }
            if (changeResult.New.LinearAcceleration is { } linear) {
                LinearAccelerationUpdated?.Invoke(this, new ChangeResult<Acceleration3D>(linear, changeResult.Old?.LinearAcceleration));
            }
            if (changeResult.New.GravityVector is { } gravity) {
                GravityVectorUpdated?.Invoke(this, new ChangeResult<Acceleration3D>(gravity, changeResult.Old?.GravityVector));
            }
            if (changeResult.New.EulerOrientation is { } euler) {
                EulerOrientationUpdated?.Invoke(this, new ChangeResult<EulerAngles>(euler, changeResult.Old?.EulerOrientation));
            }
            if (changeResult.New.Temperature is { } temp) {
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
            var x = (short)((sensorReadings[start + 1] << 8) | sensorReadings[start]);
            var y = (short)((sensorReadings[start + 3] << 8) | sensorReadings[start + 2]);
            var z = (short)((sensorReadings[start + 5] << 8) | sensorReadings[start + 4]);

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
            var x = (short)((sensorReadings[start + 1] << 8) | sensorReadings[start]);
            var y = (short)((sensorReadings[start + 3] << 8) | sensorReadings[start + 2]);
            var z = (short)((sensorReadings[start + 5] << 8) | sensorReadings[start + 4]);
            return new EulerAngles(x / divisor, y / divisor, z / divisor);
        }

        /// <summary>
        ///     Read all of the registers and display their values on the Debug output.
        /// </summary>
	    public void DisplayRegisters()
        {
            Console.WriteLine("ReadRegisters");

            var registers = Peripheral.ReadRegisters(Registers.ChipID, 0x6a);
            DebugInformation.DisplayRegisters(0x00, registers);

            Console.WriteLine("ReadRegisters end");
        }
    }
}