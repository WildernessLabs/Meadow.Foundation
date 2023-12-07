using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// Valid I2C addresses for the sensor
/// </summary>
public enum Addresses : byte
{
    /// <summary>
    /// Bus address 0x40
    /// </summary>
    Address_0x40 = 0x40,
    /// <summary>
    /// Default bus address
    /// </summary>
    Default = Address_0x40
}

/// <summary>
/// Abstract HTDx1D base class for HTU21D and HTU31D
/// </summary>
public abstract class Htux1dBase :
    ByteCommsSensorBase<(Units.Temperature? Temperature, RelativeHumidity? Humidity)>,
    ITemperatureSensor, IHumiditySensor, II2cPeripheral
{
    private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers;
    private event EventHandler<IChangeResult<RelativeHumidity>> _humidityHandlers;

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

    /// <summary>
    /// Default I2C bus speed
    /// </summary>
    public int DEFAULT_SPEED => 400;

    /// <summary>
    /// The temperature, from the last reading
    /// </summary>
    public Units.Temperature? Temperature => Conditions.Temperature;

    /// <summary>
    /// The humidity, in percent relative humidity, from the last reading
    /// </summary>
    public RelativeHumidity? Humidity => Conditions.Humidity;

    /// <summary>
    /// Serial number of the device.
    /// </summary>
    public UInt32 SerialNumber { get; protected set; }

    /// <summary>
    /// Abstract HTDx1D ctor for HTU21D and HTU31D
    /// </summary>
    /// <param name="i2cBus"></param>
    /// <param name="address"></param>
    /// <param name="updateInterval"></param>
    public Htux1dBase(II2cBus i2cBus, byte address = (byte)Addresses.Default, TimeSpan? updateInterval = null)
        : base(i2cBus, address, (int)(updateInterval == null ? 1000 : updateInterval.Value.TotalMilliseconds))
    {
    }

    /// <summary>
    /// Inheritance-safe way to raise events and notify observers
    /// </summary>
    /// <param name="changeResult">New temperature and humidity values</param>
    protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> changeResult)
    {
        if (changeResult.New.Temperature is { } temp)
        {
            _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
        }
        if (changeResult.New.Humidity is { } humidity)
        {
            _humidityHandlers?.Invoke(this, new ChangeResult<Units.RelativeHumidity>(humidity, changeResult.Old?.Humidity));
        }
        base.RaiseEventsAndNotify(changeResult);
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
        => (await Read()).Temperature!.Value;

    async Task<RelativeHumidity> ISensor<RelativeHumidity>.Read()
        => (await Read()).Humidity!.Value;
}