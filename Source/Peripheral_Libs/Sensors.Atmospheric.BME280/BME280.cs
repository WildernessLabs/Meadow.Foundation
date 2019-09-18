using System;
using System.Threading;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// BME280 Temperature, Pressure and Humidity Sensor.
    /// </summary>
    /// <remarks>
    /// This class implements the functionality necessary to read the temperature, pressure and humidity
    /// from the Bosch BME280 sensor.
    /// </remarks>
    public class BME280 : ITemperatureSensor, IHumiditySensor, IBarometricPressure
    {
        #region Constants

        /// <summary>
        ///     Minimum value that should be used for the polling frequency.
        /// </summary>
        public const ushort MinimumPollingPeriod = 100;

        #endregion Constants

        #region Enums

        /// <summary>
        ///     Valid oversampling values.
        /// </summary>
        /// <remarks>
        ///     000 - Data output set to 0x8000
        ///     001 - Oversampling x1
        ///     010 - Oversampling x2
        ///     011 - Oversampling x4
        ///     100 - Oversampling x8
        ///     101, 110, 111 - Oversampling x16
        /// </remarks>
        public enum Oversample : byte
        {
            Skip = 0,
            OversampleX1,
            OversampleX2,
            OversampleX4,
            OversampleX8,
            OversampleX16
        }

        /// <summary>
        ///     Valid values for the operating mode of the sensor.
        /// </summary>
        /// <remarks>
        ///     00 - Sleep mode
        ///     01 and 10 - Forced mode
        ///     11 - Normal mode
        /// </remarks>
        public enum Modes : byte
        {
            Sleep = 0,
            Forced = 1,
            Normal = 3
        }

        /// <summary>
        ///     Valid values for the inactive duration in normal mode.
        /// </summary>
        /// <remarks>
        ///     000 - 0.5 milliseconds
        ///     001 - 62.5 milliseconds
        ///     010 - 125 milliseconds
        ///     011 - 250 milliseconds
        ///     100 - 500 milliseconds
        ///     101 - 1000 milliseconds
        ///     110 - 10 milliseconds
        ///     111 - 20 milliseconds
        ///     See section 3.4 of the datasheet.
        /// </remarks>
        public enum StandbyDuration : byte
        {
            MsHalf = 0,
            Ms62Half,
            Ms125,
            Ms250,
            Ms500,
            Ms1000,
            Ms10,
            Ms20
        }

        /// <summary>
        ///     Valid filter co-efficient values.
        /// </summary>
        public enum FilterCoefficient : byte
        {
            Off = 0,
            Two,
            Four,
            Eight,
            Sixteen
        }

        #endregion Enums

        #region Classes / structures

        /// <summary>
        ///     Compensation data.
        /// </summary>
        private struct CompensationData
        {
            public ushort T1;
            public short T2;
            public short T3;
            public ushort P1;
            public short P2;
            public short P3;
            public short P4;
            public short P5;
            public short P6;
            public short P7;
            public short P8;
            public short P9;
            public byte H1;
            public short H2;
            public byte H3;
            public short H4;
            public short H5;
            public sbyte H6;
        }

        /// <summary>
        ///     Registers used to control the BME280.
        /// </summary>
        private static class Registers
        {
            public const byte Humidity = 0xf2;
            public const byte Status = 0xf3;
            public const byte Measurement = 0xf4;
            public const byte Configuration = 0xf5;
            public const byte Reset = 0xe0;
        }

        #endregion Internal Structures

        #region Member Variables / fields

        /// <summary>
        ///     Communication bus used to read and write to the BME280 sensor.
        /// </summary>
        /// <remarks>
        ///     The BME has both I2C and SPI interfaces. The ICommunicationBus allows the
        ///     selection to be made in the constructor.
        /// </remarks>
        private readonly II2cPeripheral _bme280;

        /// <summary>
        ///     Compensation data from the sensor.
        /// </summary>
        private CompensationData _compensationData;

        /// <summary>
        ///     Update interval in milliseconds
        /// </summary>
        private readonly ushort _updateInterval = 100;

        #endregion Member Variables

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
        ///     Humidity reading from the last update.
        /// </summary>
        public float Humidity
        {
            get { return _humidity; }
            private set
            {
                _humidity = value;
                //
                //  Check to see if the change merits raising an event.
                //
                if ((_updateInterval > 0) && (Math.Abs(_lastNotifiedHumidity - value) >= HumidityChangeNotificationThreshold))
                {
                    HumidityChanged(this, new SensorFloatEventArgs(_lastNotifiedHumidity, value));
                    _lastNotifiedHumidity = value;
                }
            }
        }
        private float _humidity;
        private float _lastNotifiedHumidity = 0.0F;

        /// <summary>
        ///     Temperature over sampling configuration.
        /// </summary>
        public Oversample TemperatureOverSampling { get; set; }

        /// <summary>
        ///     Pressure over sampling configuration.
        /// </summary>
        public Oversample PressureOversampling { get; set; }

        /// <summary>
        ///     Humidity over sampling configuration.
        /// </summary>
        public Oversample HumidityOverSampling { get; set; }

        /// <summary>
        ///     Set the operating mode for the sensor.
        /// </summary>
        public Modes Mode { get; set; }

        /// <summary>
        ///     Set the standby period for the sensor.
        /// </summary>
        public StandbyDuration Standby { get; set; }

        /// <summary>
        ///     Determine the time constant for the IIR filter.
        /// </summary>
        /// <remarks>
        ///     See section 3.44 of the datasheet for more informaiton.
        /// </remarks>
        public FilterCoefficient Filter { get; set; }

        /// <summary>
        ///     Any changes in the temperature that are greater than the temperature
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float TemperatureChangeNotificationThreshold { get; set; } = 0.001F;

        /// <summary>
        ///     Any changes in the humidity that are greater than the humidity
        ///     threshold will cause an event to be raised when the instance is
        ///     set to update automatically.
        /// </summary>
        public float HumidityChangeNotificationThreshold { get; set; } = 0.001F;

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
        ///     Event raised when the humidity change is greater than the
        ///     HumidityChangeNotificationThreshold value.
        /// </summary>
        public event SensorFloatEventHandler HumidityChanged = delegate { };

        /// <summary>
        ///     Event raised when the change in pressure is greater than the
        ///     PressureChangeNotificationThreshold value.
        /// </summary>
        public event SensorFloatEventHandler PressureChanged = delegate { };

        #endregion Events and delegates

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Meadow.Foundation.Sensors.Barometric.BME280" /> class.
        /// </summary>
        /// <remarks>
        ///     This constructor is private to force the use of the constructor which defines the
        ///     communication parameters for the sensor.
        /// </remarks>
        private BME280()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Meadow.Foundation.Sensors.Barometric.BME280" /> class.
        /// </summary>
        /// <param name="address">I2C address of the sensor (default = 0x77).</param>
        /// <param name="updateInterval">Number of milliseconds between samples (0 indicates polling to be used)</param>
        /// <param name="humidityChangeNotificationThreshold">Changes in humidity greater than this value will trigger an event when updatePeriod > 0.</param>
        /// <param name="temperatureChangeNotificationThreshold">Changes in temperature greater than this value will trigger an event when updatePeriod > 0.</param>
        /// <param name="pressureChangedNotificationThreshold">Changes in pressure greater than this value will trigger an event when updatePeriod > 0.</param>
        public BME280(II2cBus i2cBus, byte address = 0x77, ushort speed = 100, ushort updateInterval = MinimumPollingPeriod,
                      float humidityChangeNotificationThreshold = 0.001F,
                      float temperatureChangeNotificationThreshold = 0.001F,
                      float pressureChangedNotificationThreshold = 10.0F)
        {
            if ((address != 0x76) && (address != 0x77))
            {
                throw new ArgumentOutOfRangeException(nameof(address), "Address should be 0x76 or 0x77");
            }
            if (humidityChangeNotificationThreshold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(humidityChangeNotificationThreshold), "Humidity threshold should be >= 0");
            }
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

            _bme280 = new I2cPeripheral(i2cBus, address);
            TemperatureChangeNotificationThreshold = temperatureChangeNotificationThreshold;
            HumidityChangeNotificationThreshold = humidityChangeNotificationThreshold;
            PressureChangeNotificationThreshold = pressureChangedNotificationThreshold;
            _updateInterval = updateInterval;
            ReadCompensationData();
            //
            //  Update the configuration information and start sampling.
            //
            TemperatureOverSampling = Oversample.OversampleX1;
            PressureOversampling = Oversample.OversampleX1;
            HumidityOverSampling = Oversample.OversampleX1;
            Mode = Modes.Normal;
            Filter = FilterCoefficient.Off;
            Standby = StandbyDuration.MsHalf;
            UpdateConfiguration();
            if (updateInterval > 0)
            {
                StartUpdating();
            }
            else
            {
                Update();
            }
        }

        event SensorFloatEventHandler ITemperatureSensor.TemperatureChanged
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
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
        ///     Update the configuration for the BME280.
        /// </summary>
        /// <remarks>
        ///     This method uses the data in the configuration properties in order to set up the
        ///     BME280.  Ensure that the following are set correctly before calling this method:
        ///     - Standby
        ///     - Filter
        ///     - HumidityOverSampling
        ///     - TemperatureOverSampling
        ///     - PressureOverSampling
        ///     - Mode
        /// </remarks>
        public void UpdateConfiguration()
        {
            //
            //  Put to sleep to allow the configuration to be changed.
            //
            _bme280.WriteRegister(Registers.Measurement, 0x00);

            var data = (byte) ((((byte) Standby << 5) & 0xe0) | (((byte) Filter << 2) & 0x1c));
            _bme280.WriteRegister(Registers.Configuration, data);
            data = (byte) ((byte) HumidityOverSampling & 0x07);
            _bme280.WriteRegister(Registers.Humidity, data);
            data = (byte) ((((byte) TemperatureOverSampling << 5) & 0xe0) |
                           (((byte) PressureOversampling << 2) & 0x1c) |
                           ((byte) Mode & 0x03));
            _bme280.WriteRegister(Registers.Measurement, data);
        }

        /// <summary>
        ///     Reset the sensor.
        /// </summary>
        /// <remarks>
        ///     Perform a full power-on-reset of the sensor and reset the configuration of the sensor.
        /// </remarks>
        public void Reset()
        {
            _bme280.WriteRegister(Registers.Reset, 0xb6);
            UpdateConfiguration();
        }

        /// <summary>
        ///     Reads the compensation data.
        /// </summary>
        /// <remarks>
        ///     The compensation data is written to the chip at the time of manufacture and cannot be changed.
        ///     This information is used to convert the readings from the sensor into actual temperature,
        ///     pressure and humidity readings.
        ///     From the data sheet, the register addresses and length are:
        ///     Temperature and pressure: start address 0x88, end address 0x9F (length = 24)
        ///     Humidity 1: 0xa1, length = 1
        ///     Humidity 2 and 3: start address 0xe1, end address 0xe7, (length = 8)
        /// </remarks>
        private void ReadCompensationData()
        {
            var temperatureAndPressureData = _bme280.ReadRegisters(0x88, 24);
            var humidityData1 = _bme280.ReadRegisters(0xa1, 1);
            var humidityData2To6 = _bme280.ReadRegisters(0xe1, 7);

            _compensationData.T1 = (ushort) (temperatureAndPressureData[0] + (temperatureAndPressureData[1] << 8));
            _compensationData.T2 = (short) (temperatureAndPressureData[2] + (temperatureAndPressureData[3] << 8));
            _compensationData.T3 = (short) (temperatureAndPressureData[4] + (temperatureAndPressureData[5] << 8));
            //
            _compensationData.P1 = (ushort) (temperatureAndPressureData[6] + (temperatureAndPressureData[7] << 8));
            _compensationData.P2 = (short) (temperatureAndPressureData[8] + (temperatureAndPressureData[9] << 8));
            _compensationData.P3 = (short) (temperatureAndPressureData[10] + (temperatureAndPressureData[11] << 8));
            _compensationData.P4 = (short) (temperatureAndPressureData[12] + (temperatureAndPressureData[13] << 8));
            _compensationData.P5 = (short) (temperatureAndPressureData[14] + (temperatureAndPressureData[15] << 8));
            _compensationData.P6 = (short) (temperatureAndPressureData[16] + (temperatureAndPressureData[17] << 8));
            _compensationData.P7 = (short) (temperatureAndPressureData[18] + (temperatureAndPressureData[19] << 8));
            _compensationData.P8 = (short) (temperatureAndPressureData[20] + (temperatureAndPressureData[21] << 8));
            _compensationData.P9 = (short) (temperatureAndPressureData[22] + (temperatureAndPressureData[23] << 8));
            //
            _compensationData.H1 = humidityData1[0];
            _compensationData.H2 = (short) (humidityData2To6[0] + (humidityData2To6[1] << 8));
            _compensationData.H3 = humidityData2To6[2];
            _compensationData.H4 = (short) ((humidityData2To6[3] << 4) + (humidityData2To6[4] & 0xf));
            _compensationData.H5 = (short) (((humidityData2To6[4] & 0xf) >> 4) + (humidityData2To6[5] << 4));
            _compensationData.H6 = (sbyte) humidityData2To6[6];
        }

        /// <summary>
        ///     Update the sensor information from the BME280.
        /// </summary>
        /// <remarks>
        ///     Reads the raw temperature, pressure and humidity data from the BME280 and applies
        ///     the compensation data to get the actual readings.  These are made available through the
        ///     Temperature, Pressure and Humidity properties.
        ///     All three readings are taken at once to ensure that the three readings are consistent.
        ///     Register locations and formulas taken from the Bosch BME280 datasheet revision 1.1, May 2015.
        ///     Register locations - section 5.3 Memory Map
        ///     Formulas - section 4.2.3 Compensation Formulas
        ///     The integer formulas have been used to try and keep the calculations performant.
        /// </remarks>
        public void Update()
        {
            var readings = _bme280.ReadRegisters(0xf7, 8);
            //
            //  Temperature calculation from section 4.2.3 of the datasheet.
            //
            // Returns temperature in DegC, resolution is 0.01 DegC. Output value of “5123” equals 51.23 DegC.
            // t_fine carries fine temperature as global value:
            //
            // BME280_S32_t t_fine;
            // BME280_S32_t BME280_compensate_T_int32(BME280_S32_t adc_T)
            // {
            //     BME280_S32_t var1, var2, T;
            //     var1 = ((((adc_T>>3) - ((BME280_S32_t)dig_T1<<1))) * ((BME280_S32_t)dig_T2)) >> 11;
            //     var2 = (((((adc_T>>4) - ((BME280_S32_t)dig_T1)) * ((adc_T>>4) - ((BME280_S32_t)dig_T1))) >> 12) *
            //     ((BME280_S32_t)dig_T3)) >> 14;
            //     t_fine = var1 + var2;
            //     T = (t_fine * 5 + 128) >> 8;
            //     return T;
            // }
            //
            var adcTemperature = (readings[3] << 12) | (readings[4] << 4) | ((readings[5] >> 4) & 0x0f);
            var tvar1 = (((adcTemperature >> 3) - (_compensationData.T1 << 1)) * _compensationData.T2) >> 11;
            var tvar2 = (((((adcTemperature >> 4) - _compensationData.T1) *
                           ((adcTemperature >> 4) - _compensationData.T1)) >> 12) * _compensationData.T3) >> 14;
            var tfine = tvar1 + tvar2;
            //
            Temperature = (float) (((tfine * 5) + 128) >> 8) / 100;
            //
            // Pressure calculation from section 4.2.3 of the datasheet.
            //
            // Returns pressure in Pa as unsigned 32 bit integer in Q24.8 format (24 integer bits and 8 fractional bits).
            // Output value of “24674867” represents 24674867/256 = 96386.2 Pa = 963.862 hPa
            //
            // BME280_U32_t BME280_compensate_P_int64(BME280_S32_t adc_P)
            // {
            //     BME280_S64_t var1, var2, p;
            //     var1 = ((BME280_S64_t)t_fine) - 128000;
            //     var2 = var1 * var1 * (BME280_S64_t)dig_P6;
            //     var2 = var2 + ((var1*(BME280_S64_t)dig_P5)<<17);
            //     var2 = var2 + (((BME280_S64_t)dig_P4)<<35);
            //     var1 = ((var1 * var1 * (BME280_S64_t)dig_P3)>>8) + ((var1 * (BME280_S64_t)dig_P2)<<12);
            //     var1 = (((((BME280_S64_t)1)<<47)+var1))*((BME280_S64_t)dig_P1)>>33;
            //     if (var1 == 0)
            //     {
            //         return 0; // avoid exception caused by division by zero
            //     }
            //     p = 1048576-adc_P;
            //     p = (((p<<31)-var2)*3125)/var1;
            //     var1 = (((BME280_S64_t)dig_P9) * (p>>13) * (p>>13)) >> 25;
            //     var2 = (((BME280_S64_t)dig_P8) * p) >> 19;
            //     p = ((p + var1 + var2) >> 8) + (((BME280_S64_t)dig_P7)<<4);
            //     return (BME280_U32_t)p;
            // }
            //
            long pvar1 = tfine - 128000;
            var pvar2 = pvar1 * pvar1 * _compensationData.P6;
            pvar2 += (pvar1 * _compensationData.P5) << 17;
            pvar2 += (long) _compensationData.P4 << 35;
            pvar1 = ((pvar1 * pvar1 * _compensationData.P8) >> 8) + ((pvar1 * _compensationData.P2) << 12);
            pvar1 = ((((long) 1 << 47) + pvar1) * _compensationData.P1) >> 33;
            if (pvar1 == 0)
            {
                Pressure = 0;
            }
            else
            {
                var adcPressure = (readings[0] << 12) | (readings[1] << 4) | ((readings[2] >> 4) & 0x0f);
                long pressure = 1048576 - adcPressure;
                pressure = (((pressure << 31) - pvar2) * 3125) / pvar1;
                pvar1 = (_compensationData.P9 * (pressure >> 13) * (pressure >> 13)) >> 25;
                pvar2 = (_compensationData.P8 * pressure) >> 19;
                pressure = ((pressure + pvar1 + pvar2) >> 8) + ((long) _compensationData.P7 << 4);
                //
                Pressure = (float) pressure / 256;
            }
            //
            // Humidity calculations from section 4.2.3 of the datasheet.
            //
            // Returns humidity in %RH as unsigned 32 bit integer in Q22.10 format (22 integer and 10 fractional bits).
            // Output value of “47445” represents 47445/1024 = 46.333 %RH
            //
            // BME280_U32_t bme280_compensate_H_int32(BME280_S32_t adc_H)
            // {
            //     BME280_S32_t v_x1_u32r;
            //     v_x1_u32r = (t_fine - ((BME280_S32_t)76800));
            //     v_x1_u32r = (((((adc_H << 14) - (((BME280_S32_t)dig_H4) << 20) - (((BME280_S32_t)dig_H5) * v_x1_u32r)) +
            //         ((BME280_S32_t)16384)) >> 15) * (((((((v_x1_u32r * ((BME280_S32_t)dig_H6)) >> 10) * (((v_x1_u32r *
            //         ((BME280_S32_t)dig_H3)) >> 11) + ((BME280_S32_t)32768))) >> 10) + ((BME280_S32_t)2097152)) *
            //         ((BME280_S32_t)dig_H2) + 8192) >> 14));
            //     v_x1_u32r = (v_x1_u32r - (((((v_x1_u32r >> 15) * (v_x1_u32r >> 15)) >> 7) * ((BME280_S32_t)dig_H1)) >> 4));
            //     v_x1_u32r = (v_x1_u32r < 0 ? 0 : v_x1_u32r);
            //     v_x1_u32r = (v_x1_u32r > 419430400 ? 419430400 : v_x1_u32r);
            //     return (BME280_U32_t)(v_x1_u32r>>12);
            // }
            //
            var adcHumidity = (readings[6] << 8) | readings[7];
            var v_x1_u32r = tfine - 76800;

            v_x1_u32r = ((((adcHumidity << 14) - (_compensationData.H4 << 20) - (_compensationData.H5 * v_x1_u32r)) +
                          16384) >> 15) *
                        ((((((((v_x1_u32r * _compensationData.H6) >> 10) *
                              (((v_x1_u32r * _compensationData.H3) >> 11) + 32768)) >> 10) + 2097152) *
                           _compensationData.H2) + 8192) >> 14);
            v_x1_u32r = v_x1_u32r - (((((v_x1_u32r >> 15) * (v_x1_u32r >> 15)) >> 7) * _compensationData.H1) >> 4);

            //v_x1_u32r = (((((adcHumidity << 14) - (((int) _compensationData.H4) << 20) - (((int) _compensationData.H5) * v_x1_u32r)) +
            //            ((int) 16384)) >> 15) * (((((((v_x1_u32r * ((int) _compensationData.H6)) >> 10) * (((v_x1_u32r *
            //            ((int) _compensationData.H3)) >> 11) + ((int) 32768))) >> 10) + ((int) 2097152)) *
            //            ((int) _compensationData.H2) + 8192) >> 14));
            //v_x1_u32r = (v_x1_u32r - (((((v_x1_u32r >> 15) * (v_x1_u32r >> 15)) >> 7) * ((int) _compensationData.H1)) >> 4));
            //
            //  Makes sure the humidity reading is in the range [0..100].
            //
            v_x1_u32r = v_x1_u32r < 0 ? 0 : v_x1_u32r;
            v_x1_u32r = v_x1_u32r > 419430400 ? 419430400 : v_x1_u32r;
            //
            Humidity = (float) (v_x1_u32r >> 12) / 1024;
        }

        #endregion Methods
    }
}