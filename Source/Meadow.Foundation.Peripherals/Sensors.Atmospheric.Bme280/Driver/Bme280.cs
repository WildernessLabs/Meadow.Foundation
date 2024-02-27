using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using HU = Meadow.Units.RelativeHumidity.UnitType;
using PU = Meadow.Units.Pressure.UnitType;
using TU = Meadow.Units.Temperature.UnitType;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// BME280 Temperature, Pressure and Humidity Sensor
/// </summary>
/// <remarks>
/// This class implements the functionality necessary to read the temperature, pressure and humidity
/// from the Bosch BME280 sensor
/// </remarks>
public partial class Bme280 :
    PollingSensorBase<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)>,
    ITemperatureSensor, IHumiditySensor, IBarometricPressureSensor, ISpiPeripheral, II2cPeripheral, IDisposable
{
    private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers = default!;
    private event EventHandler<IChangeResult<RelativeHumidity>> _humidityHandlers = default!;
    private event EventHandler<IChangeResult<Pressure>> _pressureHandlers = default!;

    event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
    {
        add => _temperatureHandlers += value;
        remove => _temperatureHandlers -= value;
    }

    event EventHandler<IChangeResult<RelativeHumidity>> ISamplingSensor<RelativeHumidity>.Updated
    {
        add => _humidityHandlers += value;
        remove => _humidityHandlers -= value;
    }

    event EventHandler<IChangeResult<Pressure>> ISamplingSensor<Pressure>.Updated
    {
        add => _pressureHandlers += value;
        remove => _pressureHandlers -= value;
    }

    /// <summary>
    /// Is the object disposed
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Did we create the port(s) used by the peripheral
    /// </summary>
    private readonly bool createdPort = false;

    /// <summary>
    /// The read buffer
    /// </summary>
    protected Memory<byte> readBuffer = new byte[32];

    /// <summary>
    /// The write buffer
    /// </summary>
    protected Memory<byte> writeBuffer = new byte[32];

    /// <summary>
    /// Temperature oversample count
    /// </summary>
    public Oversample TemperatureSampleCount { get; set; } = Oversample.OversampleX8;

    /// <summary>
    /// Pressure oversample count
    /// </summary>
    public Oversample PressureSampleCount { get; set; } = Oversample.OversampleX8;

    /// <summary>
    /// Humidity oversample count
    /// </summary>
    public Oversample HumiditySampleCount { get; set; } = Oversample.OversampleX8;

    /// <summary>
    /// Communication bus used to read and write to the BME280 sensor
    /// </summary>
    private readonly IByteCommunications bme280Comms;

    /// <summary>
    /// Compensation data from the sensor
    /// </summary>
    private CompensationData compensationData;

    /// <summary>
    /// Sensor configuration
    /// </summary>
    protected Configuration configuration;

    /// <summary>
    /// The temperature from the last reading
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
    /// The default SPI bus speed for the device
    /// </summary>
    public Frequency DefaultSpiBusSpeed => new Frequency(10000, Frequency.UnitType.Kilohertz);

    /// <summary>
    /// The SPI bus speed for the device
    /// </summary>
    public Frequency SpiBusSpeed
    {
        get => ((ISpiCommunications)bme280Comms).BusSpeed;
        set => ((ISpiCommunications)bme280Comms).BusSpeed = value;
    }

    /// <summary>
    /// The default SPI bus mode for the device
    /// </summary>
    public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;

    /// <summary>
    /// The SPI bus mode for the device
    /// </summary>
    public SpiClockConfiguration.Mode SpiBusMode
    {
        get => ((ISpiCommunications)bme280Comms).BusMode;
        set => ((ISpiCommunications)bme280Comms).BusMode = value;
    }

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    private IDigitalOutputPort? chipSelectPort;

    /// <summary>
    /// Initializes a new instance of the BME280 class
    /// </summary>
    /// <param name="i2cBus">I2C Bus to use for communicating with the sensor</param>
    /// <param name="address">I2C address of the sensor (default = 0x76)</param>
    public Bme280(II2cBus i2cBus, byte address = (byte)Addresses.Default)
    {
        bme280Comms = new I2cCommunications(i2cBus, address);
        configuration = new Configuration(); // here to avoid the warning
        Initialize();
    }

    /// <summary>
    /// Initializes a new instance of the BME280 class
    /// </summary>
    /// <param name="spiBus">The SPI bus connected to the BME280</param>
    /// <param name="chipSelectPin">The chip select pin</param>
    public Bme280(ISpiBus spiBus, IPin chipSelectPin) :
        this(spiBus, chipSelectPin.CreateDigitalOutputPort())
    {
        createdPort = true;
    }

    /// <summary>
    /// Initializes a new instance of the BME280 class
    /// </summary>
    /// <param name="spiBus">The SPI bus connected to the BME280</param>
    /// <param name="chipSelectPort">The port for the chip select pin</param>
    public Bme280(ISpiBus spiBus, IDigitalOutputPort chipSelectPort)
    {
        bme280Comms = new SpiCommunications(spiBus, this.chipSelectPort = chipSelectPort, DefaultSpiBusSpeed, DefaultSpiBusMode);
        configuration = new Configuration(); // here to avoid the warning
        Initialize();
    }

    /// <summary>
    /// Initialize the sensor
    /// </summary>
    protected void Initialize()
    {
        ReadCompensationData();

        configuration.Mode = Modes.Sleep;
        configuration.Filter = FilterCoefficient.Off;
        UpdateConfiguration(configuration);
    }

    /// <summary>
    /// Raise events for subscribers and notify of value changes
    /// </summary>
    /// <param name="changeResult">The updated sensor data</param>
    protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)> changeResult)
    {
        if (changeResult.New.Temperature is { } temp)
        {
            _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
        }
        if (changeResult.New.Humidity is { } humidity)
        {
            _humidityHandlers?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
        }
        if (changeResult.New.Pressure is { } pressure)
        {
            _pressureHandlers?.Invoke(this, new ChangeResult<Units.Pressure>(pressure, changeResult.Old?.Pressure));
        }
        base.RaiseEventsAndNotify(changeResult);
    }

    /// <summary>
    /// Update the sensor information from the BME280.
    /// </summary>
    /// <remarks>
    /// Reads the raw temperature, pressure and humidity data from the BME280 and applies
    /// the compensation data to get the actual readings.  These are made available through the
    /// Temperature, Pressure and Humidity properties.
    /// All three readings are taken at once to ensure that the three readings are consistent.
    /// Register locations and formulas taken from the Bosch BME280 datasheet revision 1.1, May 2015.
    /// Register locations - section 5.3 Memory Map
    /// Formulas - section 4.2.3 Compensation Formulas
    /// The integer formulas have been used to try and keep the calculations per formant.
    /// </remarks>
    protected override async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity, Pressure? Pressure)> ReadSensor()
    {
        //TODO: set an update flag on the oversample properties and set
        // these once, unless the update flag has been set.
        configuration.TemperatureOverSampling = TemperatureSampleCount;
        configuration.PressureOversampling = PressureSampleCount;
        configuration.HumidityOverSampling = HumiditySampleCount;

        //if we're not in normal mode, set up the BME280 for a one-time read
        if (configuration.Mode != Modes.Normal)
        {
            configuration.Mode = Modes.Forced;
            configuration.Filter = FilterCoefficient.Off;
            UpdateConfiguration(configuration);
            await Task.Delay(100); //give the BME280 time to read new values
        }

        (Units.Temperature Temperature, RelativeHumidity Humidity, Pressure Pressure) conditions;

        bme280Comms.ReadRegister(0xf7, readBuffer.Span[0..8]);

        var adcTemperature = (readBuffer.Span[3] << 12) | (readBuffer.Span[4] << 4) | ((readBuffer.Span[5] >> 4) & 0x0f);
        var tvar1 = (((adcTemperature >> 3) - (compensationData.T1 << 1)) * compensationData.T2) >> 11;
        var tvar2 = (((((adcTemperature >> 4) - compensationData.T1) *
                       ((adcTemperature >> 4) - compensationData.T1)) >> 12) * compensationData.T3) >> 14;
        var tfine = tvar1 + tvar2;

        conditions.Temperature = new Units.Temperature((float)(((tfine * 5) + 128) >> 8) / 100, TU.Celsius);

        long pvar1 = tfine - 128000;
        var pvar2 = pvar1 * pvar1 * compensationData.P6;
        pvar2 += (pvar1 * compensationData.P5) << 17;
        pvar2 += (long)compensationData.P4 << 35;
        pvar1 = ((pvar1 * pvar1 * compensationData.P8) >> 8) + ((pvar1 * compensationData.P2) << 12);
        pvar1 = ((((long)1 << 47) + pvar1) * compensationData.P1) >> 33;
        if (pvar1 == 0)
        {
            conditions.Pressure = new Pressure(0, PU.Pascal);
        }
        else
        {
            var adcPressure = (readBuffer.Span[0] << 12) | (readBuffer.Span[1] << 4) | ((readBuffer.Span[2] >> 4) & 0x0f);
            long pressure = 1048576 - adcPressure;
            pressure = (((pressure << 31) - pvar2) * 3125) / pvar1;
            pvar1 = (compensationData.P9 * (pressure >> 13) * (pressure >> 13)) >> 25;
            pvar2 = (compensationData.P8 * pressure) >> 19;
            pressure = ((pressure + pvar1 + pvar2) >> 8) + ((long)compensationData.P7 << 4);
            conditions.Pressure = new Pressure((double)pressure / 256, PU.Pascal);
        }

        var adcHumidity = (readBuffer.Span[6] << 8) | readBuffer.Span[7];
        var v_x1_u32r = tfine - 76800;

        v_x1_u32r = ((((adcHumidity << 14) - (compensationData.H4 << 20) - (compensationData.H5 * v_x1_u32r)) +
                      16384) >> 15) *
                    ((((((((v_x1_u32r * compensationData.H6) >> 10) *
                          (((v_x1_u32r * compensationData.H3) >> 11) + 32768)) >> 10) + 2097152) *
                       compensationData.H2) + 8192) >> 14);
        v_x1_u32r = v_x1_u32r - (((((v_x1_u32r >> 15) * (v_x1_u32r >> 15)) >> 7) * compensationData.H1) >> 4);

        v_x1_u32r = v_x1_u32r < 0 ? 0 : v_x1_u32r;
        v_x1_u32r = v_x1_u32r > 419430400 ? 419430400 : v_x1_u32r;

        conditions.Humidity = new RelativeHumidity((v_x1_u32r >> 12) / 1024, HU.Percent);

        return conditions;
    }
    /// <summary>
    /// Update the configuration for the BME280.
    /// </summary>
    /// <remarks>
    /// This method uses the data in the configuration properties in order to set up the
    /// BME280.  Ensure that the following are set correctly before calling this method:
    /// - Standby
    /// - Filter
    /// - HumidityOverSampling
    /// - TemperatureOverSampling
    /// - PressureOverSampling
    /// - Mode
    /// </remarks>
    protected void UpdateConfiguration(Configuration configuration)
    {
        //
        //  Put to sleep to allow the configuration to be changed.
        //
        bme280Comms.WriteRegister((byte)Register.Measurement, 0x00);

        var data = (byte)((((byte)configuration.Standby << 5) & 0xe0) | (((byte)configuration.Filter << 2) & 0x1c));
        bme280Comms.WriteRegister((byte)Register.Configuration, data);
        data = (byte)((byte)configuration.HumidityOverSampling & 0x07);
        bme280Comms.WriteRegister((byte)Register.Humidity, data);
        data = (byte)((((byte)configuration.TemperatureOverSampling << 5) & 0xe0) |
                       (((byte)configuration.PressureOversampling << 2) & 0x1c) |
                       ((byte)configuration.Mode & 0x03));
        bme280Comms.WriteRegister((byte)Register.Measurement, data);
    }

    /// <summary>
    /// Reset the sensor.
    /// </summary>
    /// <remarks>
    /// Perform a full power-on-reset of the sensor and reset the configuration of the sensor.
    /// </remarks>
    public void Reset()
    {
        bme280Comms.WriteRegister((byte)Register.Reset, 0xb6);
        UpdateConfiguration(configuration);
    }

    /// <summary>
    /// Reads the compensation data.
    /// </summary>
    /// <remarks>
    /// The compensation data is written to the chip at the time of manufacture and cannot be changed.
    /// This information is used to convert the readings from the sensor into actual temperature,
    /// pressure and humidity readings.
    /// From the data sheet, the register addresses and length are:
    /// Temperature and pressure: start address 0x88, end address 0x9F (length = 24)
    /// Humidity 1: 0xa1, length = 1
    /// Humidity 2 and 3: start address 0xe1, end address 0xe7, (length = 8)
    /// </remarks>
    protected void ReadCompensationData()
    {
        // read the temperature and pressure data into the internal read buffer
        bme280Comms.ReadRegister(0x88, readBuffer.Span[0..24]);

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
        bme280Comms.ReadRegister(0xa1, readBuffer.Span[0..1]);
        compensationData.H1 = readBuffer.Span[0];
        // 2-6
        bme280Comms.ReadRegister(0xe1, readBuffer.Span[0..7]);
        compensationData.H2 = (short)(readBuffer.Span[0] + (readBuffer.Span[1] << 8));
        compensationData.H3 = readBuffer.Span[2];
        compensationData.H4 = (short)((readBuffer.Span[3] << 4) + (readBuffer.Span[4] & 0xf));
        compensationData.H5 = (short)(((readBuffer.Span[4] & 0xf) >> 4) + (readBuffer.Span[5] << 4));
        compensationData.H6 = (sbyte)readBuffer.Span[6];
    }

    /// <summary>
    /// Get the chip ID
    /// </summary>
    /// <returns></returns>
    public byte GetChipID()
    {
        bme280Comms.ReadRegister((byte)Register.ChipID, readBuffer.Span[0..1]);
        return readBuffer.Span[0];
    }

    /// <summary>
    /// Start updating 
    /// </summary>
    /// <param name="updateInterval">The update inverval</param>
    public override void StartUpdating(TimeSpan? updateInterval = null)
    {
        configuration.Mode = Modes.Normal;
        UpdateConfiguration(configuration);

        base.StartUpdating(updateInterval);
    }

    ///<inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of the object
    /// </summary>
    /// <param name="disposing">Is disposing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing && createdPort)
            {
                chipSelectPort?.Dispose();
            }

            IsDisposed = true;
        }
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
        => (await Read()).Temperature!.Value;

    async Task<RelativeHumidity> ISensor<RelativeHumidity>.Read()
        => (await Read()).Humidity!.Value;

    async Task<Pressure> ISensor<Pressure>.Read()
        => (await Read()).Pressure!.Value;
}