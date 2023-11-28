using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// Bosch BMP085 digital pressure and temperature sensor
/// </summary>
public partial class Bmp085 :
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
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    private readonly byte oversamplingSetting;

    /// <summary>
    /// These wait times correspond to the oversampling settings
    /// </summary>
    private readonly byte[] pressureWaitTime = { 5, 8, 14, 26 };

    // Calibration data backing stores
    private short _ac1;
    private short _ac2;
    private short _ac3;
    private ushort _ac4;
    private ushort _ac5;
    private ushort _ac6;
    private short _b1;
    private short _b2;
    private short _mb;
    private short _mc;
    private short _md;

    /// <summary>
    /// Last value read from the Pressure sensor
    /// </summary>
    public Units.Temperature? Temperature => Conditions.Temperature;

    /// <summary>
    /// Last value read from the Pressure sensor
    /// </summary>
    public Pressure? Pressure => Conditions.Pressure;

    /// <summary>
    /// Create a new BMP085 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">The I2C address</param>
    /// <param name="deviceMode">The device mode</param>
    public Bmp085(II2cBus i2cBus, byte address = (byte)Addresses.Default,
        DeviceMode deviceMode = DeviceMode.Standard)
            : base(i2cBus, address)
    {
        oversamplingSetting = (byte)deviceMode;

        GetCalibrationData();
    }

    /// <summary>
    /// Raise events for subscribers and notify of value changes
    /// </summary>
    /// <param name="changeResult">The updated sensor data</param>
    protected override void RaiseEventsAndNotify(IChangeResult<(Units.Temperature? Temperature, Pressure? Pressure)> changeResult)
    {
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
    /// Calculates the compensated pressure and temperature
    /// </summary>
    protected override Task<(Units.Temperature? Temperature, Pressure? Pressure)> ReadSensor()
    {
        (Units.Temperature? Temperature, Pressure? Pressure) conditions;

        long x1, x2, x3, b4, b5, b6, b7, p;

        long ut = ReadUncompensatedTemperature();

        long up = ReadUncompensatedPressure();

        // calculate the compensated temperature
        x1 = (ut - _ac6) * _ac5 >> 15;
        x2 = (_mc << 11) / (x1 + _md);
        b5 = x1 + x2;

        conditions.Temperature = new Units.Temperature((float)((b5 + 8) >> 4) / 10, Units.Temperature.UnitType.Celsius);

        // calculate the compensated pressure
        b6 = b5 - 4000;
        x1 = (_b2 * (b6 * b6 >> 12)) >> 11;
        x2 = _ac2 * b6 >> 11;
        x3 = x1 + x2;
        var b3 = oversamplingSetting switch
        {
            0 => (_ac1 * 4 + x3 + 2) >> 2,
            1 => (_ac1 * 4 + x3 + 2) >> 1,
            2 => (_ac1 * 4 + x3 + 2),
            3 => (_ac1 * 4 + x3 + 2) << 1,
            _ => throw new Exception("Oversampling setting must be 0-3"),
        };

        x1 = _ac3 * b6 >> 13;
        x2 = (_b1 * (b6 * b6 >> 12)) >> 16;
        x3 = ((x1 + x2) + 2) >> 2;
        b4 = (_ac4 * (x3 + 32768)) >> 15;
        b7 = (up - b3) * (50000 >> oversamplingSetting);
        p = (b7 < 0x80000000 ? (b7 * 2) / b4 : (b7 / b4) * 2);
        x1 = (p >> 8) * (p >> 8);
        x1 = (x1 * 3038) >> 16;
        x2 = (-7357 * p) >> 16;

        int value = (int)(p + ((x1 + x2 + 3791) >> 4));

        conditions.Pressure = new Pressure(value, Units.Pressure.UnitType.Pascal);

        return Task.FromResult(conditions);
    }

    private long ReadUncompensatedTemperature()
    {
        WriteBuffer.Span[0] = 0xf4;
        WriteBuffer.Span[1] = 0x2e;
        BusComms?.Write(WriteBuffer.Span[0..2]);

        Thread.Sleep(5);

        WriteBuffer.Span[0] = 0xf6;
        BusComms?.Write(WriteBuffer.Span[0]);

        BusComms?.Read(ReadBuffer.Span[0..2]);

        return (ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1];
    }

    private long ReadUncompensatedPressure()
    {
        WriteBuffer.Span[0] = 0xf4;
        WriteBuffer.Span[1] = (byte)(0x34 + (oversamplingSetting << 6));

        Thread.Sleep(pressureWaitTime[oversamplingSetting]);

        BusComms?.ReadRegister(0xf6, ReadBuffer.Span[0..3]);

        return ((ReadBuffer.Span[0] << 16) | (ReadBuffer.Span[1] << 8) | (ReadBuffer.Span[2])) >> (8 - oversamplingSetting);
    }

    /// <summary>
    /// Retrieves the factory calibration data stored in the sensor
    /// </summary>
    private void GetCalibrationData()
    {
        _ac1 = ReadShort(0xAA);
        _ac2 = ReadShort(0xAC);
        _ac3 = ReadShort(0xAE);
        _ac4 = (ushort)ReadShort(0xB0);
        _ac5 = (ushort)ReadShort(0xB2);
        _ac6 = (ushort)ReadShort(0xB4);
        _b1 = ReadShort(0xB6);
        _b2 = ReadShort(0xB8);
        _mb = ReadShort(0xBA);
        _mc = ReadShort(0xBC);
        _md = ReadShort(0xBE);
    }

    private short ReadShort(byte address)
    {
        BusComms?.ReadRegister(address, ReadBuffer.Span[0..2]);

        return (short)((ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1]);
    }

    async Task<Units.Temperature> ISensor<Units.Temperature>.Read()
        => (await Read()).Temperature!.Value;

    async Task<Pressure> ISensor<Pressure>.Read()
        => (await Read()).Pressure!.Value;
}