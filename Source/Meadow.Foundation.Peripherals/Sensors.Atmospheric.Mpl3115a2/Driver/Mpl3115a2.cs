using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// Driver for the MPL3115A2 pressure and humidity sensor
/// </summary>
public partial class Mpl3115a2 :
    ByteCommsSensorBase<(Units.Temperature? Temperature, Pressure? Pressure)>,
    ITemperatureSensor, IBarometricPressureSensor, II2cPeripheral
{
    private event EventHandler<IChangeResult<Units.Temperature>> _temperatureHandlers;
    private event EventHandler<IChangeResult<Pressure>> _pressureHandlers;

    event EventHandler<IChangeResult<Units.Temperature>> ISamplingSensor<Units.Temperature>.Updated
    {
        add => _temperatureHandlers += value;
        remove => _temperatureHandlers -= value;
    }

    event EventHandler<IChangeResult<Pressure>> ISamplingSensor<Pressure>.Updated
    {
        add => _pressureHandlers += value;
        remove => _pressureHandlers -= value;
    }

    /// <summary>
    /// The temperature, from the last reading.
    /// </summary>
    public Units.Temperature? Temperature => Conditions.Temperature;

    /// <summary>
    /// The pressure, from the last reading.
    /// </summary>
    public Pressure? Pressure => Conditions.Pressure;

    /// <summary>
    /// Check if the part is in standby mode or change the standby mode.
    /// </summary>
    /// <remarks>
    /// Changes the SBYB bit in Control register 1 to put the device to sleep
    /// or to allow measurements to be made.
    /// </remarks>
    public bool Standby
    {
        get => (BusComms?.ReadRegister(Registers.Control1) & 0x01) > 0;
        set
        {
            var status = BusComms?.ReadRegister(Registers.Control1) ?? 0;
            if (value)
            {
                status &= (byte)~ControlRegisterBits.Active;
            }
            else
            {
                status |= ControlRegisterBits.Active;
            }
            BusComms?.WriteRegister(Registers.Control1, status);
        }
    }

    /// <summary>
    /// Get the status register from the sensor
    /// </summary>
    public byte Status => BusComms?.ReadRegister(Registers.Status) ?? 0;

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Create a new MPL3115A2 object with the default address and speed settings
    /// </summary>
    /// <param name="address">Address of the sensor (default = 0x60)</param>
    /// <param name="i2cBus">I2cBus (Maximum is 400 kHz)</param>
    public Mpl3115a2(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        : base(i2cBus, address)
    {
        if (BusComms?.ReadRegister(Registers.WhoAmI) != 0xc4)
        {
            throw new Exception("Unexpected device ID, expected 0xc4");
        }
        BusComms?.WriteRegister(Registers.Control1,
                                 (byte)(ControlRegisterBits.Active |
                                        ControlRegisterBits.OverSample128));

        BusComms?.WriteRegister(Registers.DataConfiguration,
                                 (byte)(ConfigurationRegisterBits.DataReadyEvent |
                                        ConfigurationRegisterBits.EnablePressureEvent |
                                        ConfigurationRegisterBits.EnableTemperatureEvent));
    }

    /// <summary>
    /// Update the temperature and pressure from the sensor and set the Pressure property.
    /// </summary>
    protected override async Task<(Units.Temperature? Temperature, Pressure? Pressure)> ReadSensor()
    {
        (Units.Temperature? Temperature, Pressure? Pressure) conditions;
        //  Force the sensor to make a reading by setting the OST bit in Control
        //  register 1 (see 7.17.1 of the datasheet).
        Standby = false;
        //  Pause until both temperature and pressure readings are available
        while ((Status & 0x06) != 0x06)
        {
            await Task.Delay(5);
        }

        await Task.Delay(100);
        BusComms?.ReadRegister(Registers.PressureMSB, ReadBuffer.Span);
        conditions.Pressure = new Pressure(DecodePressure(ReadBuffer.Span[0], ReadBuffer.Span[1], ReadBuffer.Span[2]), Units.Pressure.UnitType.Pascal);
        conditions.Temperature = new Units.Temperature(DecodeTemperature(ReadBuffer.Span[3], ReadBuffer.Span[4]), Units.Temperature.UnitType.Celsius);

        return conditions;
    }

    /// <summary>
    /// Inheritance-safe way to raise events and notify observers.
    /// </summary>
    /// <param name="changeResult"></param>
    protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, Pressure? Pressure)> changeResult)
    {
        //Updated?.Invoke(this, changeResult);
        if (changeResult.New.Temperature is { } temp)
        {
            _temperatureHandlers?.Invoke(this, new ChangeResult<Units.Temperature>(temp, changeResult.Old?.Temperature));
        }
        if (changeResult.New.Pressure is { } pressure)
        {
            _pressureHandlers?.Invoke(this, new ChangeResult<Units.Pressure>(pressure, changeResult.Old?.Pressure));
        }
        base.RaiseEventsAndNotify(changeResult);
    }

    /// <summary>
    /// Decode the three data bytes representing the pressure into a doubling
    /// point pressure value.
    /// </summary>
    /// <param name="msb">MSB for the pressure sensor reading.</param>
    /// <param name="csb">CSB for the pressure sensor reading.</param>
    /// <param name="lsb">LSB of the pressure sensor reading.</param>
    /// <returns>Pressure in Pascals.</returns>
    private float DecodePressure(byte msb, byte csb, byte lsb)
    {
        uint pressure = msb;
        pressure <<= 8;
        pressure |= csb;
        pressure <<= 8;
        pressure |= lsb;
        return (float)(pressure / 64.0);
    }

    /// <summary>
    /// Decode the two bytes representing the temperature into degrees C.
    /// </summary>
    /// <param name="msb">MSB of the temperature sensor reading.</param>
    /// <param name="lsb">LSB of the temperature sensor reading.</param>
    /// <returns>Temperature in degrees C.</returns>
    private float DecodeTemperature(byte msb, byte lsb)
    {
        ushort temperature = msb;
        temperature <<= 8;
        temperature |= lsb;
        return (float)(temperature / 256.0);
    }

    /// <summary>
    /// Reset the sensor
    /// </summary>
    public void Reset()
    {
        var data = BusComms?.ReadRegister(Registers.Control1) ?? 0;
        data |= 0x04;
        BusComms?.WriteRegister(Registers.Control1, data);
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
        => (await Read()).Temperature!.Value;

    async Task<Pressure> ISensor<Pressure>.Read()
        => (await Read()).Pressure!.Value;
}