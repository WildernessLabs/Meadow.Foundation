using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// Represents an HIH6130 Temperature and Humidity sensor
/// </summary>
public partial class Hih6130 :
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
    /// The temperature, from the last reading
    /// </summary>
    public Units.Temperature? Temperature => Conditions.Temperature;

    /// <summary>
    /// The humidity, in percent relative humidity, from the last reading
    /// </summary>
    public RelativeHumidity? Humidity => Conditions.Humidity;

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Create a new HIH6130 object using the default parameters for the component.
    /// </summary>
    /// <param name="address">Address of the HIH6130 (default = 0x27).</param>
    /// <param name="i2cBus">I2C bus (default = 100 KHz).</param>
    public Hih6130(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        : base(i2cBus, address, readBufferSize: 4, writeBufferSize: 4)
    {
    }

    /// <summary>
    /// Inheritance-safe way to raise events and notify observers.
    /// </summary>
    /// <param name="changeResult"></param>
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

    /// <summary>
    /// Force the sensor to make a reading and update the relevant properties.
    /// </summary>
    protected override async Task<(Units.Temperature? Temperature, RelativeHumidity? Humidity)> ReadSensor()
    {
        (Units.Temperature Temperature, RelativeHumidity Humidity) conditions;

        // send a start signal on the I2C bus to notify the sensor to read.
        BusComms?.Write(0);
        // Sensor takes 35ms to make a valid reading.
        await Task.Delay(40);

        // read data from the sensor
        BusComms?.Read(base.ReadBuffer.Span);
        //
        //  Data format:
        //
        //  Byte 0: S1  S0  H13 H12 H11 H10 H9 H8
        //  Byte 1: H7  H6  H5  H4  H3  H2  H1 H0
        //  Byte 2: T13 T12 T11 T10 T9  T8  T7 T6
        //  Byte 4: T5  T4  T3  T2  T1  T0  XX XX
        //
        if ((ReadBuffer.Span[0] & 0xc0) != 0)
        {
            throw new Exception("Status indicates readings are invalid.");
        }

        var reading = ((ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1]) & 0x3fff;
        conditions.Humidity = new RelativeHumidity(((float)reading / 16383) * 100);
        reading = ((ReadBuffer.Span[2] << 8) | ReadBuffer.Span[3]) >> 2;
        conditions.Temperature = new Units.Temperature((((float)reading / 16383) * 165) - 40, Units.Temperature.UnitType.Celsius);

        return conditions;
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
        => (await Read()).Temperature!.Value;

    async Task<RelativeHumidity> ISensor<RelativeHumidity>.Read()
        => (await Read()).Humidity!.Value;
}