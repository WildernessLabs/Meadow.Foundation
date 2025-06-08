using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature;

/// <summary>
/// LM75 Temperature sensor object
/// </summary>    
public partial class Mlx90614 : ByteCommsSensorBase<Units.Temperature>,
        ITemperatureSensor, II2cPeripheral
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// The Temperature value from the last reading
    /// </summary>
    public Units.Temperature? Temperature => Conditions;

    /// <summary>
    /// Create a new Mlx90614 object using the default configuration for the sensor
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">I2C address of the sensor</param>
    public Mlx90614(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        : base(i2cBus, address)
    {
    }

    /// <summary>
    /// Update the Temperature property
    /// </summary>
    protected override Task<Units.Temperature> ReadSensor()
    {
        return Task.FromResult(ReadTempRegister(Registers.TOBJ1));
    }

    private Units.Temperature ReadTempRegister(Registers register)
    {
        var raw = BusComms.ReadRegisterAsUShort((byte)register);

        if (raw == 0) { throw new Exception("Invalid read"); }

        return new Units.Temperature((raw * 0.02) - 273.15, Units.Temperature.UnitType.Celsius);
    }

    /// <summary>
    /// Reads the sensor (object) temperature
    /// </summary>
    public Task<Units.Temperature> ReadTemperature()
    {
        return Task.FromResult(ReadTempRegister(Registers.TOBJ1));
    }

    /// <summary>
    /// Reads the sensor (object) temperature
    /// </summary>
    public Task<Units.Temperature> ReadAmbientTemperature()
    {
        return Task.FromResult(ReadTempRegister(Registers.TA));
    }
}