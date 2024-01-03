using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature;

/// <summary>
/// Represents a Mcp960x Thermocouple sensor object
/// </summary>    
public abstract partial class Mcp960x :
    ByteCommsSensorBase<(Units.Temperature? TemperatureHot, Units.Temperature? TemperatureCold)>,
    ITemperatureSensor, II2cPeripheral
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers;

    event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
    {
        add => _temperatureHandlers += value;
        remove => _temperatureHandlers -= value;
    }

    /// <summary>
    /// Raised when the Hot temperature value changes
    /// </summary>
    public event EventHandler<IChangeResult<Units.Temperature>> TemperatureHotUpdated = default!;

    /// <summary>
    /// Raised when the Cold / ambient temperature value changes
    /// </summary>
    public event EventHandler<IChangeResult<Units.Temperature>> TemperatureColdUpdated = default!;

    /// <summary>
    /// The Hot Temperature value from the last reading
    /// </summary>
    public Units.Temperature? Temperature => TemperatureHot;

    /// <summary>
    /// The Hot Temperature value from the last reading
    /// </summary>
    public Units.Temperature? TemperatureHot { get; protected set; }

    /// <summary>
    /// The Cold Temperature value from the last reading
    /// </summary>
    public Units.Temperature? TemperatureCold { get; protected set; }

    /// <summary>
    /// Create a new Mcp960x object using the default configuration for the sensor
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">I2C address of the sensor</param>
    public Mcp960x(II2cBus i2cBus, byte address)
        : base(i2cBus, address)
    {
        if (BusComms == null)
        {
            throw new NullReferenceException("Mcp960x peripheral did not initialize");
        }

        BusComms.WriteRegister(DEVICECONFIG, 0x80);
    }

    /// <summary>
    /// Update the Temperature property
    /// </summary>
    protected override Task<(Units.Temperature? TemperatureHot, Units.Temperature? TemperatureCold)> ReadSensor()
    {
        (Units.Temperature? TemperatureHot, Units.Temperature? TemperatureCold) conditions;

        conditions.TemperatureHot = new Units.Temperature(ReadTemperatureHot(), Units.Temperature.UnitType.Celsius);
        conditions.TemperatureCold = new Units.Temperature(ReadTemperatureCold(), Units.Temperature.UnitType.Celsius);

        return Task.FromResult(conditions);
    }

    /// <summary>
    /// Reads the hot junction temperature from the MCP960x
    /// </summary>
    /// <returns>The hot junction temperature in degrees Celsius</returns>
    private double ReadTemperatureHot()
    {
        byte[] readBuffer = new byte[2];

        BusComms?.ReadRegister(HOTJUNCTION, readBuffer);

        int rawTemp = (readBuffer[0] << 8) | readBuffer[1];
        double temperature = rawTemp * 0.0625;

        return temperature;
    }

    /// <summary>
    /// Reads the cold/ambient temperature from the MCP960x
    /// </summary>
    /// <returns>The cold/ambient temperature in degrees Celsius</returns>
    private double ReadTemperatureCold()
    {
        byte[] readBuffer = new byte[2];

        BusComms?.ReadRegister(COLDJUNCTION, readBuffer);

        int rawTemp = (readBuffer[0] << 8) | readBuffer[1];
        double temperature = rawTemp * 0.0625;

        return temperature;
    }

    /// <summary>
    /// Sets the thermocouple type for the MCP960x
    /// </summary>
    /// <param name="type">The thermocouple type to set</param>
    public void SetThermocoupleType(ThermocoupleType type)
    {
        byte config = BusComms.ReadRegister(SENSORCONFIG);

        config = (byte)((config & 0xF8) | (byte)type);

        BusComms.WriteRegister(SENSORCONFIG, config);
    }

    /// <summary>
    /// Gets the thermocouple type currently configured for the MCP960x
    /// </summary>
    /// <returns>The currently configured thermocouple type</returns>
    public ThermocoupleType GetThermocoupleType()
    {
        byte config = BusComms.ReadRegister(SENSORCONFIG);

        return (ThermocoupleType)(config & 0x07);
    }

    /// <summary>
    /// Sets the ADC resolution for the MCP960x
    /// </summary>
    /// <param name="resolution">The ADC resolution to set</param>
    public void SetAdcResolution(AdcResolution resolution)
    {
        byte config = BusComms.ReadRegister(SENSORCONFIG);

        config = (byte)((config & 0x9F) | ((byte)resolution << 5));

        BusComms.WriteRegister(SENSORCONFIG, config);
    }

    /// <summary>
    /// Gets the ADC resolution currently configured for the MCP960x
    /// </summary>
    /// <returns>The currently configured ADC resolution</returns>
    public AdcResolution GetAdcResolution()
    {
        byte config = BusComms.ReadRegister(SENSORCONFIG);

        return (AdcResolution)((config & 0x60) >> 5);
    }

    /// <summary>
    /// Sets the filter coefficient for the MCP960x
    /// </summary>
    /// <param name="coefficient">The filter coefficient to set</param>
    public void SetFilterCoefficient(FilterCoefficient coefficient)
    {
        byte config = BusComms.ReadRegister(SENSORCONFIG);

        config = (byte)((config & 0xF8) | ((byte)coefficient & 0x07));

        BusComms.WriteRegister(SENSORCONFIG, config);
    }

    /// <summary>
    /// Gets the filter coefficient for the MCP960x
    /// </summary>
    /// <returns>The currently configured filter coefficient</returns>
    public FilterCoefficient GetFilterCoefficient()
    {
        byte config = BusComms.ReadRegister(SENSORCONFIG);

        return (FilterCoefficient)(config & 0x07);
    }

    /// <summary>
    /// Sets the alert temperature for the specified alert number of the MCP960x
    /// </summary>
    /// <param name="alertNumber">The alert number (1-4) to set the temperature for</param>
    /// <param name="temperature">The temperature value</param>
    public void SetAlertTemperature(AlertNumber alertNumber, Units.Temperature temperature)
    {
        if (alertNumber < AlertNumber.Alert1 || alertNumber > AlertNumber.Alert4)
        {
            throw new ArgumentOutOfRangeException("Alert number must be between Alert1 and Alert4.");
        }

        short tempData = (short)(temperature.Celsius / 0.0625);

        BusComms.WriteRegister((byte)(ALERTLIMIT_1 + ((int)alertNumber - 1) * 2), new byte[] { (byte)(tempData >> 8), (byte)(tempData & 0xFF) });
    }

    /// <summary>
    /// Gets the alert temperature for the specified alert number of the MCP960x
    /// </summary>
    /// <param name="alertNumber">The alert number (1-4) to get the temperature for</param>
    /// <returns>The alert temperature value</returns>
    public Units.Temperature GetAlertTemperature(AlertNumber alertNumber)
    {
        if (alertNumber < AlertNumber.Alert1 || alertNumber > AlertNumber.Alert4)
        {
            throw new ArgumentOutOfRangeException("Alert number must be between Alert1 and Alert4.");
        }

        byte[] readBuffer = new byte[2];

        BusComms.ReadRegister((byte)(ALERTLIMIT_1 + ((int)alertNumber - 1) * 2), readBuffer);

        ushort tempData = (ushort)((readBuffer[0] << 8) | readBuffer[1]);
        double temperature = tempData / 16.0;

        return new Units.Temperature(temperature, Units.Temperature.UnitType.Celsius);
    }

    /// <summary>
    /// Configures the alert settings for the MCP960x
    /// </summary>
    /// <param name="alertNumber">The alert number (Alert1-Alert4) to configure</param>
    /// <param name="enabled">Whether the alert is enabled</param>
    /// <param name="rising">Whether the alert triggers on a rising temperature. Set to false for falling temperature</param>
    /// <param name="alertColdJunction">Whether the alert triggers on cold junction temperature. Set to false for thermocouple temperature</param>
    /// <param name="activeHigh">Whether the alert pin is active high. Set to false for active low</param>
    /// <param name="interruptMode">Whether the alert pin is in interrupt mode. Set to false for comparator mode</param>
    public void ConfigureAlert(AlertNumber alertNumber, bool enabled, bool rising, bool alertColdJunction, bool activeHigh, bool interruptMode)
    {
        if (alertNumber < AlertNumber.Alert1 || alertNumber > AlertNumber.Alert4)
        {
            throw new ArgumentOutOfRangeException("Alert number must be between Alert1 and Alert4.");
        }

        byte configValue = 0;

        if (enabled) configValue |= 0x01;
        if (interruptMode) configValue |= 0x02;
        if (activeHigh) configValue |= 0x04;
        if (rising) configValue |= 0x08;
        if (alertColdJunction) configValue |= 0x10;

        BusComms.WriteRegister((byte)(ALERTCONFIG_1 + ((int)alertNumber - 1)), configValue);
    }

    /// <summary>
    /// Enables or disables the MCP960x sensor
    /// </summary>
    /// <param name="enable">True to enable the sensor, false to enter sleep mode</param>
    public void Enable(bool enable)
    {
        byte config = BusComms.ReadRegister(DEVICECONFIG);

        config &= 0b1111_1100; // Clear bits 0 and 1

        if (!enable) // Sleep mode
        {
            config |= 0x01;
        }

        BusComms.WriteRegister(DEVICECONFIG, config);
    }

    /// <summary>
    /// Checks whether the MCP960x sensor is enabled and working or in sleep mode
    /// </summary>
    /// <returns>True if in awake mode, false if in sleep mode</returns>
    public bool IsEnabled()
    {
        byte config = BusComms.ReadRegister(DEVICECONFIG);
        int statusBits = (config & 0b11);

        return statusBits == 0;
    }

    /// <summary>
    /// Raise events for subscribers and notify of value changes
    /// </summary>
    /// <param name="changeResult">The updated sensor data</param>
    protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? TemperatureHot, Units.Temperature? TemperatureCold)> changeResult)
    {
        if (changeResult.New.TemperatureHot is { } hot)
        {
            _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(hot, changeResult.Old?.TemperatureHot));
            TemperatureHotUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(hot, changeResult.Old?.TemperatureHot));
        }
        if (changeResult.New.TemperatureCold is { } cold)
        {
            TemperatureHotUpdated?.Invoke(this, new ChangeResult<Units.Temperature>(cold, changeResult.Old?.TemperatureCold));
        }

        base.RaiseEventsAndNotify(changeResult);
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
        => (await Read()).TemperatureHot ?? throw new Exception("Temperature not available");
}