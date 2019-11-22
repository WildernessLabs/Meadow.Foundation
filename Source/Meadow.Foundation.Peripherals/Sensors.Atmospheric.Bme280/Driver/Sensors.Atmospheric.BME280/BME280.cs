using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Peripherals.Sensors.Temperature;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    /// <summary>
    /// BME280 Temperature, Pressure and Humidity Sensor.
    /// </summary>
    /// <remarks>
    /// This class implements the functionality necessary to read the temperature, pressure and humidity
    /// from the Bosch BME280 sensor.
    /// </remarks>
    public class BME280 :
        FilterableObservableBase<AtmosphericConditionChangeResult, AtmosphericConditions>,
        IAtmosphericSensor, ITemperatureSensor, IHumiditySensor, IBarometricPressureSensor
    {
        #region Constants

        ///// <summary>
        /////     Minimum value that should be used for the polling frequency.
        ///// </summary>
        //public const ushort MinimumPollingPeriod = 100;

        #endregion Constants

        #region Enums

        public enum ChipType : byte
        {
            BMP = 0x58,
            BME = 0x60
        }

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
        public enum Modes : byte
        {
            /// <summary>
            /// no operation, all registers accessible, lowest power, selected after startup
            /// </summary>
            Sleep = 0,
            /// <summary>
            /// perform one measurement, store results and return to sleep mode
            /// </summary>
            Forced = 1,
            /// <summary>
            /// perpetual cycling of measurements and inactive periods.
            /// </summary>
            Normal = 3
        }

        /// <summary>
        ///     Valid values for the inactive duration in normal mode.
        /// </summary>
        public enum StandbyDuration : byte
        {
            /// <summary>
            /// 0.5 milliseconds
            /// </summary>
            MsHalf = 0,
            /// <summary>
            /// 62.5 milliseconds
            /// </summary>
            Ms62Half,
            /// <summary>
            /// 125 milliseconds
            /// </summary>
            Ms125,
            /// <summary>
            /// 250 milliseconds
            /// </summary>
            Ms250,
            /// <summary>
            /// 500 milliseconds
            /// </summary>
            Ms500,
            /// <summary>
            /// 1000 milliseconds
            /// </summary>
            Ms1000,
            /// <summary>
            /// 10 milliseconds
            /// </summary>
            Ms10,
            /// <summary>
            /// 20 milliseconds
            /// </summary>
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

        public enum I2cAddress : byte
        {
            Adddress0x76 = 0x76,
            Adddress0x77 = 0x77
        }

        #endregion Enums

        #region Classes / structures

        /// <summary>
        ///     Compensation data.
        /// </summary>
        protected struct CompensationData
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

        #endregion Internal Structures

        #region Member Variables / fields

        /// <summary>
        ///     Communication bus used to read and write to the BME280 sensor.
        /// </summary>
        /// <remarks>
        ///     The BME has both I2C and SPI interfaces. The ICommunicationBus allows the
        ///     selection to be made in the constructor.
        /// </remarks>
        private readonly BME280Comms _bme280;

        /// <summary>
        ///     Compensation data from the sensor.
        /// </summary>
        protected CompensationData _compensationData;

        ///// <summary>
        /////     Update interval in milliseconds
        ///// </summary>
        //private ushort _updateInterval = 100;

        protected Configuration configuration;

        // internal thread lock
        private object _lock = new object();
        private CancellationTokenSource SamplingTokenSource;

        #endregion Member Variables

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the analog input port is currently
        /// sampling the ADC. Call StartSampling() to spin up the sampling process.
        /// </summary>
        /// <value><c>true</c> if sampling; otherwise, <c>false</c>.</value>
        public bool IsSampling { get; protected set; } = false;

        /// <summary>
        /// The AtmosphericConditions from the last reading.
        /// </summary>
        public AtmosphericConditions Conditions { get; protected set; } = new AtmosphericConditions();

        /// <summary>
        /// The temperature, in degrees celsius (ºC), from the last reading.
        /// </summary>
        public float Temperature => Conditions.Temperature;

        /// <summary>
        /// The pressure, in hectopascals (hPa), from the last reading. 1 hPa
        /// is equal to one millibar, or 1/10th of a kilopascal (kPa)/centibar.
        /// </summary>
        public float Pressure => Conditions.Pressure;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public float Humidity => Conditions.Humidity;

        #endregion Properties

        #region Events and delegates

        /// <summary>
        /// </summary>
        public event EventHandler<AtmosphericConditionChangeResult> Updated = delegate { };

        #endregion Events and delegates

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Meadow.Foundation.Sensors.Barometric.BME280" /> class.
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
        /// <param name="i2c">I2C Bus to use for communicating with the sensor</param>
        /// <param name="busAddress">I2C address of the sensor (default = 0x77).</param>
        public BME280(II2cBus i2c, I2cAddress busAddress = I2cAddress.Adddress0x77)
        {
            _bme280 = new BME280I2C(i2c, (byte)busAddress);
            Init();
        }

        public BME280(ISpiBus spi, IDigitalOutputPort chipSelect)
        {
            _bme280 = new BME280SPI(spi, chipSelect);
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void Init()
        {
            // these are basically calibrations burned into the chip.
            ReadCompensationData();

            //
            //  Update the configuration information and start sampling.
            //
            configuration = new Configuration();
            configuration.Mode = Modes.Sleep;
            configuration.Filter = FilterCoefficient.Off;
            UpdateConfiguration(configuration);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Convenience method to get the current sensor readings. For frequent reads, use
        /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
        /// </summary>
        /// <param name="temperatureSampleCount">The number of sample readings to take. 
        /// Must be greater than 0. These samples are automatically averaged.</param>
        public async Task<AtmosphericConditions> Read(
            Oversample temperatureSampleCount = Oversample.OversampleX8,
            Oversample pressureSampleCount = Oversample.OversampleX8,
            Oversample humiditySampleCount = Oversample.OversampleX8)
        {
            // update confiruation for a one-off read
            configuration.TemperatureOverSampling = temperatureSampleCount;
            configuration.PressureOversampling = pressureSampleCount;
            configuration.HumidityOverSampling = humiditySampleCount;
            configuration.Mode = Modes.Forced;
            configuration.Filter = FilterCoefficient.Off;
            UpdateConfiguration(configuration);

            this.Conditions = await Read();

            return Conditions;
        }

        public void StartUpdating(
            Oversample temperatureSampleCount = Oversample.OversampleX8,
            Oversample pressureSampleCount = Oversample.OversampleX8,
            Oversample humiditySampleCount = Oversample.OversampleX1,
            int standbyDuration = 1000)
        {
            // TODO: for standby durations of 1,000ms and less, the sensor
            // will actually handle the reading loop. you put it into `Normal`
            // mode and set it to one of the known `StandbyDuration`s.
            //
            // So perhaps this should be an option. From a method signature
            // standpoint, i think that we would add an overload that took
            // a known `StandbyDuration` instead of an int.
            //
            // With that said, however, as far as I can tell, the sensor won't
            // send an interrupt when a new reading is taken, so i'm not sure
            // how we would synchronize with it, since the time that each read
            // takes is determined by the samples, filter, etc. -b
            //
            // TODO: for longer standby durations, we should put the sensor into
            // Modes.Sleep to save power. Need to figure out what the stanby
            // duration threshold is for that. i'm guessing 5 seconds might be a
            // good value.
            

            // thread safety
            lock (_lock) {
                if (IsSampling) return;

                // state muh-cheen
                IsSampling = true;

                SamplingTokenSource = new CancellationTokenSource();
                CancellationToken ct = SamplingTokenSource.Token;

                AtmosphericConditions oldConditions;
                AtmosphericConditionChangeResult result;
                Task.Factory.StartNew(async () => {
                    while (true) {
                        if (ct.IsCancellationRequested) {
                            // do task clean up here
                            _observers.ForEach(x => x.OnCompleted());
                            break;
                        }
                        // capture history
                        oldConditions = Conditions;

                        // read
                        await Read(temperatureSampleCount, pressureSampleCount, humiditySampleCount);

                        // build a new result with the old and new conditions
                        result = new AtmosphericConditionChangeResult(oldConditions, Conditions);

                        // let everyone know
                        RaiseChangedAndNotify(result);

                        // sleep for the appropriate interval
                        await Task.Delay(standbyDuration);
                    }
                }, SamplingTokenSource.Token);
            }
        }

        protected void RaiseChangedAndNotify(AtmosphericConditionChangeResult changeResult)
        {
            Updated?.Invoke(this, changeResult);
            base.NotifyObservers(changeResult);
        }

        /// <summary>
        /// Stops sampling the temperature.
        /// </summary>
        public void StopUpdating()
        {
            lock (_lock) {
                if (!IsSampling) return;

                SamplingTokenSource?.Cancel();

                // state muh-cheen
                IsSampling = false;
            }
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
        protected void UpdateConfiguration(Configuration configuration)
        {
            //
            //  Put to sleep to allow the configuration to be changed.
            //
            _bme280.WriteRegister(BME280Comms.Register.Measurement, 0x00);

            var data = (byte) ((((byte) configuration.Standby << 5) & 0xe0) | (((byte)configuration.Filter << 2) & 0x1c));
            _bme280.WriteRegister(BME280Comms.Register.Configuration, data);
            data = (byte) ((byte)configuration.HumidityOverSampling & 0x07);
            _bme280.WriteRegister(BME280Comms.Register.Humidity, data);
            data = (byte) ((( (byte)configuration.TemperatureOverSampling << 5) & 0xe0) |
                           (( (byte)configuration.PressureOversampling << 2) & 0x1c) |
                           ( (byte)configuration.Mode & 0x03));
            _bme280.WriteRegister(BME280Comms.Register.Measurement, data);
        }

        /// <summary>
        ///     Reset the sensor.
        /// </summary>
        /// <remarks>
        ///     Perform a full power-on-reset of the sensor and reset the configuration of the sensor.
        /// </remarks>
        public void Reset()
        {
            _bme280.WriteRegister(BME280Comms.Register.Reset, 0xb6);
            UpdateConfiguration(configuration);
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
        protected void ReadCompensationData()
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
        /// Update the sensor information from the BME280.
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
        protected async Task<AtmosphericConditions> Read()
        {
            return await Task.Run(() => {
                AtmosphericConditions conditions = new AtmosphericConditions();

                var readings = _bme280.ReadRegisters(0xf7, 8);
                Console.WriteLine($"  Readings: {BitConverter.ToString(readings)}");
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
                conditions.Temperature = (float)(((tfine * 5) + 128) >> 8) / 100;
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
                pvar2 += (long)_compensationData.P4 << 35;
                pvar1 = ((pvar1 * pvar1 * _compensationData.P8) >> 8) + ((pvar1 * _compensationData.P2) << 12);
                pvar1 = ((((long)1 << 47) + pvar1) * _compensationData.P1) >> 33;
                if (pvar1 == 0) {
                    conditions.Pressure = 0;
                } else {
                    var adcPressure = (readings[0] << 12) | (readings[1] << 4) | ((readings[2] >> 4) & 0x0f);
                    long pressure = 1048576 - adcPressure;
                    pressure = (((pressure << 31) - pvar2) * 3125) / pvar1;
                    pvar1 = (_compensationData.P9 * (pressure >> 13) * (pressure >> 13)) >> 25;
                    pvar2 = (_compensationData.P8 * pressure) >> 19;
                    pressure = ((pressure + pvar1 + pvar2) >> 8) + ((long)_compensationData.P7 << 4);
                    //
                    conditions.Pressure = (float)pressure / 256;
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
                conditions.Humidity = (float)(v_x1_u32r >> 12) / 1024;

                return conditions;
            });
        }

        public byte GetChipID()
        {
            return _bme280.ReadRegisters((byte)BME280Comms.Register.ChipID, 1).First();
        }

        #endregion Methods


        public class Configuration
        {
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
        }

    }
}