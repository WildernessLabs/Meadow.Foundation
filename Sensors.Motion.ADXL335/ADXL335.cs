using Meadow.Foundation.Spatial;
using Meadow.Hardware;
using System;
using System.Threading;

namespace Meadow.Foundation.Sensors.Motion
{
    /// <summary>
    ///     Provide an interface for the ADXL335 triple axis accelerometer.
    /// </summary>
    public class ADXL335
    {
        #region Constants

        /// <summary>
        ///     Minimum value that can be used for the update interval when the
        ///     sensor is being configured to generate interrupts.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;
        
        #endregion Constants
        
        #region Member variables / fields

        /// <summary>
        ///     Analog input channel connected to the x axis.
        /// </summary>
        private readonly AnalogInputPort _x;

        /// <summary>
        ///     Analog input channel connected to the x axis.
        /// </summary>
        private readonly AnalogInputPort _y;

        /// <summary>
        ///     Analog input channel connected to the x axis.
        /// </summary>
        private readonly AnalogInputPort _z;

        /// <summary>
        ///     Voltage that represents 0g.  This is the supply voltage / 2.
        /// </summary>
        private double _zeroGVoltage;

        /// <summary>
        ///     How often should this sensor be read?
        /// </summary>
        private readonly ushort _updateInterval;

        /// <summary>
        ///     Last X acceleration reading from the sensor.
        /// </summary>
        private double _lastX = 0;

        /// <summary>
        ///     Last Y reading from the sensor.
        /// </summary>
        private double _lastY = 0;
        
        /// <summary>
        ///     Last Z reading from the sensor.
        /// </summary>
        private double _lastZ = 0;

        #endregion Member variables / fields

        #region Properties

        /// <summary>
        ///     Acceleration along the X-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public double X { get; private set; }

        /// <summary>
        ///     Acceleration along the Y-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public double Y { get; private set; }

        /// <summary>
        ///     Acceleration along the Z-axis.
        /// </summary>
        /// <remarks>
        ///     This property will only contain valid data after a call to Read or after
        ///     an interrupt has been generated.
        /// </remarks>
        public double Z { get; private set; }

        /// <summary>
        ///     Volts per G for the X axis.
        /// </summary>
        public double XVoltsPerG { get; set; }

        /// <summary>
        ///     Volts per G for the X axis.
        /// </summary>
        public double YVoltsPerG { get; set; }

        /// <summary>
        ///     Volts per G for the X axis.
        /// </summary>
        public double ZVoltsPerG { get; set; }

        /// <summary>
        ///     Power supply voltage applied to the sensor.  This will be set (in the constructor)
        ///     to 3.3V by default.
        /// </summary>
        private double _supplyVoltage;

        public double SupplyVoltage
        {
            get { return _supplyVoltage; }
            set
            {
                _supplyVoltage = value;
                _zeroGVoltage = value / 2;
            }
        }
        
        /// <summary>
        ///     Acceleration as read from the 
        /// </summary>
        private Vector Acceleration { get; set; }

        /// <summary>
        ///     Any changes in the acceleration that are greater than the acceleration
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public double AccelerationChangeNotificationThreshold { get; set; } = 0.1F;

        #endregion Properties
        
        #region Events and delegates

        /// <summary>
        ///     Event to be raised when the acceleration is greater than
        ///     +/- AccelerationChangeNotificationThreshold.
        /// </summary>
        public event SensorVectorEventHandler AccelerationChanged = delegate { };
        
        #endregion Events and delegates


        #region Constructors

        /// <summary>
        ///     Make the default constructor private so that the developer cannot access it.
        /// </summary>
        private ADXL335()
        {
        }

        /// <summary>
        ///     Create a new ADXL335 sensor object.
        /// </summary>
        /// <param name="x">Analog pin connected to the X axis output from the ADXL335 sensor.</param>
        /// <param name="y">Analog pin connected to the Y axis output from the ADXL335 sensor.</param>
        /// <param name="z">Analog pin connected to the Z axis output from the ADXL335 sensor.</param>
        /// <param name="updateInterval">Update interval for the sensor, set to 0 to put the sensor in polling mode.</param>
        /// <<param name="accelerationChangeNotificationThreshold">Acceleration change threshold.</param>
        public ADXL335(Cpu.AnalogChannel x, Cpu.AnalogChannel y, Cpu.AnalogChannel z, ushort updateInterval = 100,
                       double accelerationChangeNotificationThreshold = 0.1F)
        {
            if ((updateInterval != 0) && (updateInterval < MinimumPollingPeriod))
            {
                throw new ArgumentOutOfRangeException(nameof(updateInterval),
                    "Update interval should be 0 or greater than " + MinimumPollingPeriod);    
            }

            _x = new AnalogInputPort(x);
            _y = new AnalogInputPort(y);
            _z = new AnalogInputPort(z);
            //
            //  Now set the default calibration data.
            //
            XVoltsPerG = 0.325;
            YVoltsPerG = 0.325;
            ZVoltsPerG = 0.550;
            SupplyVoltage = 3.3;

            if (updateInterval > 0)
            {
                StartUpdating();
            }
            else
            {
                Update();
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        ///     Start the update process.
        /// </summary>
        private void StartUpdating()
        {
            Thread t = new Thread(() => {
                while (true)
                {
                    Update();
                    Thread.Sleep(_updateInterval);
                }
            });
            t.Start();
        }

        /// <summary>
        ///     Read the sensor output and convert the sensor readings into acceleration values.
        /// </summary>
        public void Update()
        {
            X = ((_x.Value * SupplyVoltage) - _zeroGVoltage) / XVoltsPerG;
            Y = ((_y.Value * SupplyVoltage) - _zeroGVoltage) / YVoltsPerG;
            Z = ((_z.Value * SupplyVoltage) - _zeroGVoltage) / ZVoltsPerG;
            if ((_updateInterval != 0) && 
                ((Math.Abs(X - _lastX) > AccelerationChangeNotificationThreshold) ||
                 (Math.Abs(Y - _lastY) > AccelerationChangeNotificationThreshold) ||
                 (Math.Abs(Z - _lastZ) > AccelerationChangeNotificationThreshold)))
            {
                Vector lastNotifiedReading = new Vector(_lastX, _lastY, _lastZ);
                Vector currentReading = new Vector(X, Y, Z);
                _lastX = X;
                _lastY = Y;
                _lastZ = Z;
                AccelerationChanged(this, new SensorVectorEventArgs(lastNotifiedReading, currentReading));
            }
        }

        /// <summary>
        ///     Get the raw analog input values from the sensor.
        /// </summary>
        /// <returns>Vector object containing the raw sensor data from the analog pins.</returns>
        public Vector GetRawSensorData()
        {
            return new Vector(_x.Value, _y.Value, _z.Value);
        }

        #endregion Methods
    }
}