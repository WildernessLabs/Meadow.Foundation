using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Temperature;

namespace Meadow.Foundation.Sensors.Barometric
{
    public class MPL115A2 : ITemperatureSensor, IBarometricPressure
    {
        #region Constants

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        #endregion Constants

        #region Structures

        /// <summary>
        ///     Device registers.
        /// </summary>
        private static class Registers
        {
            public static readonly byte PressureMSB = 0x00;
            public static readonly byte PressureLSB = 0x01;
            public static readonly byte TemperatureMSB = 0x02;
            public static readonly byte TemperatureLSB = 0x03;
            public static readonly byte A0MSB = 0x04;
            public static readonly byte A0LSB = 0x05;
            public static readonly byte B1MSB = 0x06;
            public static readonly byte B1LSB = 0x07;
            public static readonly byte B2MSB = 0x08;
            public static readonly byte B2LSB = 0x09;
            public static readonly byte C12MSB = 0x0a;
            public static readonly byte C12LSB = 0x0b;
            public static readonly byte StartConversion = 0x12;
        }

        /// <summary>
        ///     Structure holding the doubleing point equivalent of the compensation
        ///     coefficients for the sensor.
        /// </summary>
        private struct Coefficients
        {
            public double A0;
            public double B1;
            public double B2;
            public double C12;
        }

        #endregion Structures

        #region Properties

        /// <summary>
        ///     Temperature reading from last update.
        /// </summary>
        public float Temperature
        {
            get { return _temperature; }
            private set
            {
                _temperature = value;
                //
                //  Check to see if the change merits raising an event.
                //
                if ((_updateInterval > 0) && (Math.Abs(_lastNotifiedTemperature - value) >= TemperatureChangeNotificationThreshold))
                {
                    TemperatureChanged(this, new SensorFloatEventArgs(_lastNotifiedTemperature, value));
                    _lastNotifiedTemperature = value;
                }
            }
        }
        private float _temperature;
        private float _lastNotifiedTemperature = 0.0F;

        /// <summary>
        ///     Pressure reading from the sensor.
        /// </summary>
        /// <value>Current pressure reading from the sensor in Pascals (divide by 100 for hPa).</value>
        public float Pressure
        {
            get { return _pressure; }
            private set
            {
                _pressure = value;
                //
                //  Check to see if the change merits raising an event.
                //
                if ((_updateInterval > 0) && (Math.Abs(_lastNotifiedPressure - value) >= PressureChangeNotificationThreshold))
                {
                    PressureChanged(this, new SensorFloatEventArgs(_lastNotifiedPressure, value));
                    _lastNotifiedPressure = value;
                }
            }
        }
        private float _pressure;
        private float _lastNotifiedPressure = 0.0F;

        /// <summary>
        ///     Any changes in the temperature that are greater than the temperature
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float TemperatureChangeNotificationThreshold { get; set; } = 0.001F;

        /// <summary>
        ///     Any changes in the pressure that are greater than the pressure
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float PressureChangeNotificationThreshold { get; set; } = 0.001F;

        #endregion Properties

        #region Events and delegates

        /// <summary>
        ///     Event raised when the temperature change is greater than the 
        ///     TemperatureChangeNotificationThreshold value.
        /// </summary>
        public event SensorFloatEventHandler TemperatureChanged = delegate { };

        /// <summary>
        ///     Event raised when the change in pressure is greater than the
        ///     PresshureChangeNotificationThreshold value.
        /// </summary>
        public event SensorFloatEventHandler PressureChanged = delegate { };

        #endregion Events and delegates

        #region Member variables / fields

        /// <summary>
        ///     SI7021 is an I2C device.
        /// </summary>
        private readonly II2cPeripheral _mpl115a2;

        /// <summary>
        ///     doubleing point variants of the compensation coefficients from the sensor.
        /// </summary>
        private Coefficients _coefficients;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

        #endregion Member variables / fields

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent the user from calling this).
        /// </summary>
        private MPL115A2()
        {
        }

        /// <summary>
        ///     Create a new MPL115A2 temperature and humidity sensor object.
        /// </summary>
        /// <param name="address">Sensor address (default to 0x60).</param>
        /// <param name="i2cBus">I2CBus (default to 100 KHz).</param>
        /// <param name="updateInterval">Number of milliseconds between samples (0 indicates polling to be used)</param>
        /// <param name="temperatureChangeNotificationThreshold">Changes in temperature greater than this value will trigger an event when updatePeriod > 0.</param>
        /// <param name="pressureChangedNotificationThreshold">Changes in pressure greater than this value will trigger an event when updatePeriod > 0.</param>
        public MPL115A2(II2cBus i2cBus, byte address = 0x60, ushort updateInterval = MinimumPollingPeriod,
            float temperatureChangeNotificationThreshold = 0.001F, float pressureChangedNotificationThreshold = 10.0F)
        {
            if (temperatureChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(temperatureChangeNotificationThreshold), "Temperature threshold should be >= 0");
            }
            if (pressureChangedNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pressureChangedNotificationThreshold), "Pressure threshold should be >= 0");
            }
            if ((updateInterval != 0) && (updateInterval < MinimumPollingPeriod))
            {
                throw new ArgumentOutOfRangeException(nameof(updateInterval), "Update period should be 0 or >= than " + MinimumPollingPeriod);
            }

            TemperatureChangeNotificationThreshold = temperatureChangeNotificationThreshold;
            PressureChangeNotificationThreshold = pressureChangedNotificationThreshold;
            _updateInterval = updateInterval;

            var device = new I2cPeripheral(i2cBus, address);
            _mpl115a2 = device;
            //
            //  Update the compensation data from the sensor.  The location and format of the
            //  compensation data can be found on pages 5 and 6 of the datasheet.
            //
            var data = _mpl115a2.ReadRegisters(Registers.A0MSB, 8);
            var a0 = (short) (ushort) ((data[0] << 8) | data[1]);
            var b1 = (short) (ushort) ((data[2] << 8) | data[3]);
            var b2 = (short) (ushort) ((data[4] << 8) | data[5]);
            var c12 = (short) (ushort) (((data[6] << 8) | data[7]) >> 2);
            //
            //  Convert the raw compensation coefficients from the sensor into the
            //  doubleing point equivalents to speed up the calculations when readings
            //  are made.
            //
            //  Datasheet, section 3.1
            //  a0 is signed with 12 integer bits followed by 3 fractional bits so divide by 2^3 (8)
            //
            _coefficients.A0 = (double) a0 / 8;
            //
            //  b1 is 2 integer bits followed by 7 fractional bits.  The lower bits are all 0
            //  so the format is:
            //      sign i1 I0 F12...F0
            //
            //  So we need to divide by 2^13 (8192)
            //
            _coefficients.B1 = (double) b1 / 8192;
            //
            //  b2 is signed integer (1 bit) followed by 14 fractional bits so divide by 2^14 (16384).
            //
            _coefficients.B2 = (double) b2 / 16384;
            //
            //  c12 is signed with no integer bits but padded with 9 zeroes:
            //      sign 0.000 000 000 f12...f0
            //
            //  So we need to divide by 2^22 (4,194,304) - 13 doubleing point bits 
            //  plus 9 leading zeroes.
            //
            _coefficients.C12 = (double) c12 / 4194304;
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
        ///     Update the temperature and pressure from the sensor and set the Pressure property.
        /// </summary>
        public void Update()
        {
            //
            //  Tell the sensor to take a temperature and pressure reading, wait for
            //  3ms (see section 2.2 of the datasheet) and then read the ADC values.
            //
            _mpl115a2.WriteBytes(new byte[] { Registers.StartConversion, 0x00 });
            Thread.Sleep(5);
            var data = _mpl115a2.ReadRegisters(Registers.PressureMSB, 4);
            //
            //  Extract the sensor data, note that this is a 10-bit reading so move
            //  the data right 6 bits (see section 3.1 of the datasheet).
            //
            var pressure = (ushort) (((data[0] << 8) + data[1]) >> 6);
            var temperature = (ushort) (((data[2] << 8) + data[3]) >> 6);
            Temperature = (float) ((temperature - 498.0) / -5.35) + 25;
            //
            //  Now use the calculations in section 3.2 to determine the
            //  current pressure reading.
            //
            const double PRESSURE_CONSTANT = 65.0 / 1023.0;
            var compensatedPressure = _coefficients.A0 + ((_coefficients.B1 + (_coefficients.C12 * temperature))
                                                          * pressure) + (_coefficients.B2 * temperature);
            Pressure = (float) (PRESSURE_CONSTANT * compensatedPressure) + 50;
        }

        #endregion Methods
    }
}