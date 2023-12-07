using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// Provide access to the CCS811 C02 and VOC Air Quality Sensor
/// </summary>
public partial class Ccs811 :
    ByteCommsSensorBase<(Concentration? Co2, Concentration? Voc)>,
    ICo2Sensor, IVocSensor, II2cPeripheral
{
    private const int ReadBufferSize = 10;
    private const int WriteBufferSize = 8;

    // internal thread lock
    private byte[] _readingBuffer = new byte[8];

    private event EventHandler<IChangeResult<Concentration>> _co2Handlers;

    event EventHandler<IChangeResult<Concentration>> ISamplingSensor<Concentration>.Updated
    {
        add => _co2Handlers += value;
        remove => _co2Handlers -= value;
    }

    /// <summary>
    /// Event raised when the VOC concentration value changes
    /// </summary>
    public event EventHandler<ChangeResult<Concentration>> VocUpdated = default!;

    /// <summary>
    /// The measured CO2 concentration
    /// </summary>
    public Concentration? Co2 => Conditions.Co2;

    /// <summary>
    /// The measured VOC concentration
    /// </summary>
    public Concentration? Voc => Conditions.Voc;

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Create a new Ccs811 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">The I2C address</param>
    public Ccs811(II2cBus i2cBus, Addresses address = Addresses.Default)
        : this(i2cBus, (byte)address)
    { }

    /// <summary>
    /// Create a new Ccs811 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">The I2C address</param>
    public Ccs811(II2cBus i2cBus, byte address)
        : base(i2cBus, address, ReadBufferSize, WriteBufferSize)
    {
        switch (address)
        {
            case 0x5a:
            case 0x5b:
                // valid;
                break;
            default:
                throw new ArgumentOutOfRangeException("CCS811 device address must be either 0x5a or 0x5b");
        }

        Initialize();
    }

    /// <summary>
    /// Initialize the sensor
    /// </summary>
    /// <exception cref="Exception">Raised if HW_ID register returns an invalid id</exception>
    protected void Initialize()
    {
        Reset();

        Thread.Sleep(100);

        var id = BusComms?.ReadRegister((byte)Register.HW_ID);
        if (id != 0x81)
        {
            throw new Exception("Hardware is not identifying as a CCS811");
        }

        BusComms?.Write((byte)BootloaderCommand.APP_START);

        SetMeasurementMode(MeasurementMode.ConstantPower1s);
        var mode = BusComms?.ReadRegister((byte)Register.MEAS_MODE);
    }

    private void ShowDebugInfo()
    {
        var ver = BusComms?.ReadRegister((byte)Register.HW_VERSION);
        Resolver.Log.Info($"hardware version A = 0x{ver:x2}");

        var fwb = BusComms?.ReadRegister((byte)Register.FW_BOOT_VERSION);
        Resolver.Log.Info($"FWB version = 0x{fwb:x4}");

        var fwa = BusComms?.ReadRegister((byte)Register.FW_APP_VERSION);
        Resolver.Log.Info($"FWA version = 0x{fwa:x4}");

        // read status
        var status = BusComms?.ReadRegister((byte)Register.STATUS);
        Resolver.Log.Info($"status = 0x{status:x2}");
    }

    /// <summary>
    /// Get baseline value
    /// </summary>
    /// <returns>The baseline value</returns>
    public ushort GetBaseline()
    {
        return BusComms?.ReadRegister((byte)Register.BASELINE) ?? 0;
    }

    /// <summary>
    /// Set the baseline value
    /// </summary>
    /// <param name="value">The new baseline</param>
    public void SetBaseline(ushort value)
    {
        BusComms?.WriteRegister((byte)Register.BASELINE, (byte)value);
    }

    /// <summary>
    /// Get the current measurement mode
    /// </summary>
    /// <returns>The measurement mode</returns>
    public MeasurementMode GetMeasurementMode()
    {
        return (MeasurementMode)(BusComms?.ReadRegister((byte)Register.MEAS_MODE) ?? 0);
    }

    /// <summary>
    /// Set the Measurement mode
    /// </summary>
    /// <param name="mode">The new mode</param>
    public void SetMeasurementMode(MeasurementMode mode)
    {
        var m = (byte)mode;
        BusComms?.WriteRegister((byte)Register.MEAS_MODE, m);
    }

    private void Reset()
    {
        BusComms?.Write(new byte[] { (byte)Register.SW_RESET, 0x11, 0xE5, 0x72, 0x8A });
    }

    /// <summary>
    /// Reads data from the sensor
    /// </summary>
    /// <returns>The latest sensor reading</returns>
    protected override Task<(Concentration? Co2, Concentration? Voc)> ReadSensor()
    {
        // data is really in just the first 4, but this gets us status and raw data as well
        BusComms?.ReadRegister((byte)Register.ALG_RESULT_DATA, _readingBuffer);

        (Concentration? co2, Concentration? voc) state;
        state.co2 = new Concentration(_readingBuffer[0] << 8 | _readingBuffer[1], Concentration.UnitType.PartsPerMillion);
        state.voc = new Concentration(_readingBuffer[2] << 8 | _readingBuffer[3], Concentration.UnitType.PartsPerBillion);

        return Task.FromResult(state);
    }

    /// <summary>
    /// Raise events for subscribers and notify of value changes
    /// </summary>
    /// <param name="changeResult">The updated sensor data</param>
    protected override void RaiseEventsAndNotify(IChangeResult<(Concentration? Co2, Concentration? Voc)> changeResult)
    {
        if (changeResult.New.Co2 is { } co2)
        {
            _co2Handlers?.Invoke(this, new ChangeResult<Concentration>(co2, changeResult.Old?.Co2));
        }
        if (changeResult.New.Voc is { } voc)
        {
            VocUpdated?.Invoke(this, new ChangeResult<Concentration>(voc, changeResult.Old?.Voc));
        }

        base.RaiseEventsAndNotify(changeResult);
    }

    async Task<Concentration> ISensor<Concentration>.Read()
        => (await Read()).Voc!.Value;
}