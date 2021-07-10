using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using PU = Meadow.Units.Pressure.UnitType;
using TU = Meadow.Units.Temperature.UnitType;
using HU = Meadow.Units.RelativeHumidity.UnitType;

namespace Meadow.Foundation.Sensors.Atmospheric
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


    /// <summary>
    /// BME280 Temperature, Pressure and Humidity Sensor.
    /// </summary>
    /// <remarks>
    /// This class implements the functionality necessary to read the temperature, pressure and humidity
    /// from the Bosch BME280 sensor.
    /// </remarks>
    public partial class Bme280 :
        SamplingSensorBase<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)>,
        ITemperatureSensor, IHumiditySensor, IBarometricPressureSensor
    {
        //==== events
        /// <summary>
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };
        public event EventHandler<IChangeResult<Pressure>> PressureUpdated = delegate { };
        public event EventHandler<IChangeResult<RelativeHumidity>> HumidityUpdated = delegate { };

        //==== internals
        protected Memory<byte> readBuffer = new byte[32];
        protected Memory<byte> writeBuffer = new byte[32];

        //==== properties
        public Oversample TemperatureSampleCount { get; set; } = Oversample.OversampleX8;
        public Oversample PressureSampleCount { get; set; } = Oversample.OversampleX8;
        public Oversample HumiditySampleCount { get; set; } = Oversample.OversampleX8;

        /// <summary>
        ///     Communication bus used to read and write to the BME280 sensor.
        /// </summary>
        /// <remarks>
        ///     The BME has both I2C and SPI interfaces. The ICommunicationBus allows the
        ///     selection to be made in the constructor.
        /// </remarks>
        private readonly Bme280Comms bme280Comms;

        /// <summary>
        ///     Compensation data from the sensor.
        /// </summary>
        protected CompensationData compensationData;

        ///// <summary>
        /////     Update interval in milliseconds
        ///// </summary>
        //private ushort _updateInterval = 100;

        protected Configuration configuration;

        /// <summary>
        /// The temperature, in degrees celsius (°C), from the last reading.
        /// </summary>
        public Units.Temperature? Temperature => Conditions.Temperature;

        /// <summary>
        /// The pressure, in hectopascals (hPa), from the last reading. 1 hPa
        /// is equal to one millibar, or 1/10th of a kilopascal (kPa)/centibar.
        /// </summary>
        public Pressure? Pressure => Conditions.Pressure;

        /// <summary>
        /// The humidity, in percent relative humidity, from the last reading..
        /// </summary>
        public RelativeHumidity? Humidity => Conditions.Humidity;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Meadow.Foundation.Sensors.Barometric.BME280" /> class.
        /// </summary>
        /// <param name="i2c">I2C Bus to use for communicating with the sensor</param>
        /// <param name="busAddress">I2C address of the sensor (default = 0x77).</param>
        public Bme280(II2cBus i2c, I2cAddress busAddress = I2cAddress.Adddress0x77)
        {
            bme280Comms = new Bme280I2C(i2c, (byte)busAddress);
            configuration = new Configuration(); // here to avoid the warning
            Init();
        }

        public Bme280(ISpiBus spi, IDigitalOutputPort chipSelect)
        {
            bme280Comms = new Bme280Spi(spi, chipSelect);
            configuration = new Configuration(); // here to avoid the warning
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        protected void Init()
        {
            // these are basically calibrations burned into the chip.
            ReadCompensationData();

            // set to sleep until we're ready to start sampling 
            configuration.Mode = Modes.Sleep;
            configuration.Filter = FilterCoefficient.Off;
            UpdateConfiguration(configuration);
        }

        protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)> changeResult)
        {
            if (changeResult.New.Temperature is { } temp) {
                TemperatureUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
            }
            if (changeResult.New.Humidity is { } humidity) {
                HumidityUpdated?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
            }
            if (changeResult.New.Pressure is { } pressure) {
                PressureUpdated?.Invoke(this, new ChangeResult<Units.Pressure>(pressure, changeResult.Old?.Pressure));
            }
            base.RaiseEventsAndNotify(changeResult);
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
        protected override async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)> ReadSensor()
        {
            return await Task.Run(() => {

                //TODO: set an update flag on the oversample properties and set
                // these once, unless the update flag has been set.

                // update configuration
                configuration.TemperatureOverSampling = TemperatureSampleCount;
                configuration.PressureOversampling = PressureSampleCount;
                configuration.HumidityOverSampling = HumiditySampleCount;
                // TODO: do we need this?
                //configuration.Mode = Modes.Forced;
                configuration.Filter = FilterCoefficient.Off;
                UpdateConfiguration(configuration);


                (Units.Temperature Temperature, RelativeHumidity Humidity, Pressure Pressure) conditions;

                // readily read the readings from the reading register into the read buffer
                bme280Comms.ReadRegisters(0xf7, readBuffer.Span[0..8]);
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
                var adcTemperature = (readBuffer.Span[3] << 12) | (readBuffer.Span[4] << 4) | ((readBuffer.Span[5] >> 4) & 0x0f);
                var tvar1 = (((adcTemperature >> 3) - (compensationData.T1 << 1)) * compensationData.T2) >> 11;
                var tvar2 = (((((adcTemperature >> 4) - compensationData.T1) *
                               ((adcTemperature >> 4) - compensationData.T1)) >> 12) * compensationData.T3) >> 14;
                var tfine = tvar1 + tvar2;
                //
                conditions.Temperature = new Units.Temperature((float)(((tfine * 5) + 128) >> 8) / 100, TU.Celsius);
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
                var pvar2 = pvar1 * pvar1 * compensationData.P6;
                pvar2 += (pvar1 * compensationData.P5) << 17;
                pvar2 += (long)compensationData.P4 << 35;
                pvar1 = ((pvar1 * pvar1 * compensationData.P8) >> 8) + ((pvar1 * compensationData.P2) << 12);
                pvar1 = ((((long)1 << 47) + pvar1) * compensationData.P1) >> 33;
                if (pvar1 == 0) {
                    conditions.Pressure = new Pressure(0, PU.Pascal);
                } else {
                    var adcPressure = (readBuffer.Span[0] << 12) | (readBuffer.Span[1] << 4) | ((readBuffer.Span[2] >> 4) & 0x0f);
                    long pressure = 1048576 - adcPressure;
                    pressure = (((pressure << 31) - pvar2) * 3125) / pvar1;
                    pvar1 = (compensationData.P9 * (pressure >> 13) * (pressure >> 13)) >> 25;
                    pvar2 = (compensationData.P8 * pressure) >> 19;
                    pressure = ((pressure + pvar1 + pvar2) >> 8) + ((long)compensationData.P7 << 4);
                    //
                    conditions.Pressure = new Pressure((double)pressure / 256, PU.Pascal);
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
                var adcHumidity = (readBuffer.Span[6] << 8) | readBuffer.Span[7];
                var v_x1_u32r = tfine - 76800;

                v_x1_u32r = ((((adcHumidity << 14) - (compensationData.H4 << 20) - (compensationData.H5 * v_x1_u32r)) +
                              16384) >> 15) *
                            ((((((((v_x1_u32r * compensationData.H6) >> 10) *
                                  (((v_x1_u32r * compensationData.H3) >> 11) + 32768)) >> 10) + 2097152) *
                               compensationData.H2) + 8192) >> 14);
                v_x1_u32r = v_x1_u32r - (((((v_x1_u32r >> 15) * (v_x1_u32r >> 15)) >> 7) * compensationData.H1) >> 4);

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
                conditions.Humidity = new RelativeHumidity((v_x1_u32r >> 12) / 1024, HU.Percent);

                return conditions;
            });
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
            bme280Comms.WriteRegister(Bme280Comms.Register.Measurement, 0x00);

            var data = (byte)((((byte)configuration.Standby << 5) & 0xe0) | (((byte)configuration.Filter << 2) & 0x1c));
            bme280Comms.WriteRegister(Bme280Comms.Register.Configuration, data);
            data = (byte)((byte)configuration.HumidityOverSampling & 0x07);
            bme280Comms.WriteRegister(Bme280Comms.Register.Humidity, data);
            data = (byte)((((byte)configuration.TemperatureOverSampling << 5) & 0xe0) |
                           (((byte)configuration.PressureOversampling << 2) & 0x1c) |
                           ((byte)configuration.Mode & 0x03));
            bme280Comms.WriteRegister(Bme280Comms.Register.Measurement, data);
        }

        /// <summary>
        ///     Reset the sensor.
        /// </summary>
        /// <remarks>
        ///     Perform a full power-on-reset of the sensor and reset the configuration of the sensor.
        /// </remarks>
        public void Reset()
        {
            bme280Comms.WriteRegister(Bme280Comms.Register.Reset, 0xb6);
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
            // read the temperature and pressure data into the internal read buffer
            bme280Comms.ReadRegisters(0x88, readBuffer.Span[0..24]);

            // Temperature
            compensationData.T1 = (ushort)(readBuffer.Span[0] + (readBuffer.Span[1] << 8));
            compensationData.T2 = (short)(readBuffer.Span[2] + (readBuffer.Span[3] << 8));
            compensationData.T3 = (short)(readBuffer.Span[4] + (readBuffer.Span[5] << 8));
            // Pressure
            compensationData.P1 = (ushort)(readBuffer.Span[6] + (readBuffer.Span[7] << 8));
            compensationData.P2 = (short)(readBuffer.Span[8] + (readBuffer.Span[9] << 8));
            compensationData.P3 = (short)(readBuffer.Span[10] + (readBuffer.Span[11] << 8));
            compensationData.P4 = (short)(readBuffer.Span[12] + (readBuffer.Span[13] << 8));
            compensationData.P5 = (short)(readBuffer.Span[14] + (readBuffer.Span[15] << 8));
            compensationData.P6 = (short)(readBuffer.Span[16] + (readBuffer.Span[17] << 8));
            compensationData.P7 = (short)(readBuffer.Span[18] + (readBuffer.Span[19] << 8));
            compensationData.P8 = (short)(readBuffer.Span[20] + (readBuffer.Span[21] << 8));
            compensationData.P9 = (short)(readBuffer.Span[22] + (readBuffer.Span[23] << 8));

            // read the humidity data. have to read twice because they're in different,
            // non-sequential registers

            // first one
            bme280Comms.ReadRegisters(0xa1, readBuffer.Span[0..1]);
            compensationData.H1 = readBuffer.Span[0];
            // 2-6
            bme280Comms.ReadRegisters(0xe1, readBuffer.Span[0..7]);
            compensationData.H2 = (short)(readBuffer.Span[0] + (readBuffer.Span[1] << 8));
            compensationData.H3 = readBuffer.Span[2];
            compensationData.H4 = (short)((readBuffer.Span[3] << 4) + (readBuffer.Span[4] & 0xf));
            compensationData.H5 = (short)(((readBuffer.Span[4] & 0xf) >> 4) + (readBuffer.Span[5] << 4));
            compensationData.H6 = (sbyte)readBuffer.Span[6];
        }

        public byte GetChipID()
        {
            bme280Comms.ReadRegisters((byte)Bme280Comms.Register.ChipID, readBuffer.Span[0..1]);
            return readBuffer.Span[0];
        }
    }
}