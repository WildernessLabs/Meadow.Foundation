using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric;


/// <summary>
/// Ahtx0 Temperature sensor object
/// </summary>    
public abstract partial class Ahtx0 :
    ByteCommsSensorBase<(RelativeHumidity? Humidity, Units.Temperature? Temperature)>,
    ISamplingTemperatureSensor, IHumiditySensor, II2cPeripheral
{
    private bool isInitialized = false;

    private event EventHandler<IChangeResult<Units.Temperature>> temperatureHandlers = default!;
    private event EventHandler<IChangeResult<RelativeHumidity>> humidityHandlers = default!;

    event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
    {
        add => temperatureHandlers += value;
        remove => temperatureHandlers -= value;
    }

    event EventHandler<IChangeResult<RelativeHumidity>> ISamplingSensor<RelativeHumidity>.Updated
    {
        add => humidityHandlers += value;
        remove => humidityHandlers -= value;
    }

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <inheritdoc/>
    public Units.Temperature? Temperature { get; protected set; }

    /// <inheritdoc/>
    public Units.RelativeHumidity? Humidity { get; protected set; }

    /// <summary>
    /// Create a new Ahtx0 object using the default configuration for the sensor
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">I2C address of the sensor</param>
    public Ahtx0(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        : base(i2cBus, address, readBufferSize: 7)
    {
    }

    /// <summary>
    /// Raise all change events for subscribers
    /// </summary>
    /// <param name="changeResult">humidity and temperature</param>
    protected override void RaiseEventsAndNotify(IChangeResult<(Units.RelativeHumidity? Humidity, Units.Temperature? Temperature)> changeResult)
    {
        if (changeResult.New.Humidity is { } humidity)
        {
            humidityHandlers?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
        }
        if (changeResult.New.Temperature is { } temperature)
        {
            temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temperature, changeResult.Old?.Temperature));
        }
        base.RaiseEventsAndNotify(changeResult);
    }

    private async Task InitializeIfRequired()
    {
        if (isInitialized) { return; }

        BusComms.Write((byte)Commands.SOFT_RESET);
        await Task.Delay(20);

        while (IsBusy())
        {
            await Task.Delay(10);
        }

        BusComms.Write(new byte[] { (byte)Commands.INITIALIZE, 0x08, 0x00 });

        while (IsBusy())
        {
            await Task.Delay(10);
        }

        BusComms.Read(ReadBuffer.Span[0..1]);

        isInitialized = true;
    }

    private bool IsBusy()
    {
        BusComms.Read(ReadBuffer.Span[0..1]);
        return (ReadBuffer.Span[0] & 0x80) == 0x80;
    }

    /// <summary>
    /// Reads the humidity and temperature.
    /// </summary>
    private readonly byte[] triggerCommand = { (byte)Commands.TRIGGER_MEAS, 0x33, 0x00 };

    protected override async Task<(Units.RelativeHumidity?, Units.Temperature?)> ReadSensor()
    {
        await InitializeIfRequired();

        BusComms.Write(triggerCommand);
        await Task.Delay(80);

        while (IsBusy())
        {
            await Task.Delay(10);
        }

        BusComms.Read(ReadBuffer.Span);

        return ParseConditions(ReadBuffer.Span);
    }

    private static (Units.RelativeHumidity?, Units.Temperature?) ParseConditions(ReadOnlySpan<byte> data)
    {
        var humidity = (data[1] << 12) | (data[2] << 4) | (data[3] >> 4);
        var relativeHumidity = new RelativeHumidity((humidity / (double)0x100000) * 100d, RelativeHumidity.UnitType.Percent);

        var temp = ((data[3] & 0x0f) << 16) | (data[4] << 8) | data[5];
        var temperature = new Units.Temperature((temp / (double)0x100000) * 200d - 50d, Units.Temperature.UnitType.Celsius);

        return (relativeHumidity, temperature);
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
        => (await Read()).Temperature!.Value;

    async Task<Units.RelativeHumidity> ISensor<Units.RelativeHumidity>.Read()
        => (await Read()).Humidity!.Value;
}