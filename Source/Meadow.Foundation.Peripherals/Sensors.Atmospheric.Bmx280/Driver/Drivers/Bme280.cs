using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using static Meadow.Foundation.Sensors.Atmospheric.Bmx280;

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
    private readonly Configuration configuration;

    /// <summary>
    /// The temperature from the last reading
    /// </summary>
    public Units.Temperature? Temperature => Conditions.Temperature;

    /// <summary>
    /// The pressure from the last reading
    /// </summary>
    public Pressure? Pressure => Conditions.Pressure;

    /// <summary>
    /// The realtive humidity from the last reading
    /// </summary>
    public RelativeHumidity? Humidity => Conditions.Humidity;

    /// <summary>
    /// The default SPI bus speed for the device
    /// </summary>
    public Frequency DefaultSpiBusSpeed => new(10000, Frequency.UnitType.Kilohertz);

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

    private readonly IDigitalOutputPort? chipSelectPort;

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
        Bmx280.ReadCompensationData(bme280Comms, readBuffer, compensationData);

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
        // TODO: set an update flag on the oversample properties and set
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

        return await Bmx280.ReadSensor(bme280Comms, readBuffer, compensationData);
    }

    /// <summary>
    /// Update the configuration for the BME280.
    /// </summary>
    /// <remarks>
    /// This method uses the data in the configuration properties in order to set up the
    /// BME280. Ensure that the following are set correctly before calling this method:
    /// - Standby
    /// - Filter
    /// - HumidityOverSampling
    /// - TemperatureOverSampling
    /// - PressureOverSampling
    /// - Mode
    /// </remarks>
    private void UpdateConfiguration(Configuration configuration)
    {
        //  Put to sleep to allow the configuration to be changed.
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
    /// Get the chip ID
    /// </summary>
    /// <returns>The ID as a byte</returns>
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