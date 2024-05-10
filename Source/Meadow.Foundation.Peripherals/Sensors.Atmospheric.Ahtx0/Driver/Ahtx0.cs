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
public partial class Ahtx0 :
    ByteCommsSensorBase<(RelativeHumidity? Humidity, Units.Temperature? Temperature)>,
    ITemperatureSensor, IHumiditySensor, II2cPeripheral
{
    private bool isInitialized = false;

    private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers = default!;
    private event EventHandler<IChangeResult<RelativeHumidity>> _humidityHandlers = default!;

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
        : base(i2cBus, address, readBufferSize: 6)
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
            _humidityHandlers?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
        }
        if (changeResult.New.Temperature is { } temperature)
        {
            _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temperature, changeResult.Old?.Temperature));
        }
        base.RaiseEventsAndNotify(changeResult);
    }

    private async Task InitializeIfRequired()
    {
        if (isInitialized) { return; }

        BusComms.Write((byte)Commands.SOFT_RESET);
        await Task.Delay(20);

        BusComms.Write((byte)Commands.INITIALIZE);
        //BusComms.Write(new byte[] { (byte)Commands.INITIALIZE, 0x08, 0x00 });

        isInitialized = true;
    }

    /// <summary>
    /// Reads the humidity and temperature.
    /// </summary>
    protected override async Task<(Units.RelativeHumidity?, Units.Temperature?)> ReadSensor()
    {
        (Units.RelativeHumidity? Humidity, Units.Temperature? Temperature) conditions;

        await InitializeIfRequired();

        BusComms.Write((byte)Commands.TRIGGER_MEAS);
        //BusComms.Write(new byte[] { (byte)Commands.TRIGGER_MEAS, 0x33, 0x00 });

        do
        {
            BusComms.Read(ReadBuffer.Span[0..1]);
        } while ((ReadBuffer.Span[0] & 0x80) != 0);

        BusComms.Read(ReadBuffer.Span);

        var humidity = ReadBuffer.Span[1] << 8 | ReadBuffer.Span[2] << 4 | ReadBuffer.Span[3] >> 4;
        conditions.Humidity = new RelativeHumidity(humidity * 100 / 0x100000);

        var temp = ReadBuffer.Span[3] & 0x0f << 8 | ReadBuffer.Span[4] << 4 | ReadBuffer.Span[5];
        conditions.Temperature = new Units.Temperature((temp * 200 / 0x100000) - 50);

        return conditions;
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
        => (await Read()).Temperature!.Value;

    async Task<Units.RelativeHumidity> ISensor<Units.RelativeHumidity>.Read()
        => (await Read()).Humidity!.Value;
}