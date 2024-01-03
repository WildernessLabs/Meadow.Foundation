using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// Represents a HC2 humidity probe by Rotronic
/// </summary>
public partial class HC2 :
    PollingSensorBase<(RelativeHumidity? Humidity, Units.Temperature? Temperature)>,
    ITemperatureSensor, IHumiditySensor
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
    /// The current relative humidity
    /// </summary>
    public Units.RelativeHumidity? Humidity { get; protected set; }

    /// <summary>
    /// The current temperature
    /// </summary>
    public Units.Temperature? Temperature => Conditions.Temperature;

    private readonly CommunicationType communicationType;

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

    /// <summary>
    /// Reads the humidity and temperature.
    /// </summary>
    protected override async Task<(Units.RelativeHumidity?, Units.Temperature?)> ReadSensor()
    {
        (Units.RelativeHumidity? Humidity, Units.Temperature? Temperature) conditions = Conditions;
        switch (communicationType)
        {
            case CommunicationType.Analog:
                conditions = await ReadSensorAnalog();
                break;
            case CommunicationType.Serial:
                conditions = ReadSensorSerial();
                break;
        }
        return conditions;
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
        => (await Read()).Temperature!.Value;

    async Task<Units.RelativeHumidity> ISensor<Units.RelativeHumidity>.Read()
        => (await Read()).Humidity!.Value;
}