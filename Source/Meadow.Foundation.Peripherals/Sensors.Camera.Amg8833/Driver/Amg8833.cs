using Meadow.Hardware;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Meadow.Foundation.Sensors.Camera;

/// <summary>
/// Represents a Panasonic Grid-EYE infrared array sensor
/// </summary>
public partial class Amg8833 : II2cPeripheral
{
    private II2cCommunications _i2cComms;

    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Creates an instance of an AMG8833 sensor
    /// </summary>
    /// <param name="i2cBus"></param>
    public Amg8833(II2cBus i2cBus)
    {
        _i2cComms = new I2cCommunications(i2cBus, (byte)Addresses.Default);

        Initialize();
    }

    private void Initialize()
    {
        SetPowerMode(Modes.Normal);
        WriteRegister(Registers.RST, Commands.RST_InitialReset);
        DisableInterrupts();
        WriteRegister(Registers.FPSC, Commands.FPS_Ten);

        Thread.Sleep(Constants.StartupDelayMs);
    }

    private void SetPowerMode(Modes mode)
    {
        WriteRegister(Registers.PCTL, Modes.Normal);
    }

    private void DisableInterrupts()
    {
        WriteRegister(Registers.INTC, (byte)0x00);
    }

    /// <summary>
    /// Reads the temperature of the on-board thermistor
    /// </summary>
    /// <returns></returns>
    public Units.Temperature ReadThermistor()
    {
        // read 2 bytes
        var raw = _i2cComms.ReadRegisterAsUShort((byte)Registers.TTHL);
        // see the data sheet on this calculation
        var converted = Convert12BitUShortToFloat(raw) * Constants.ThermistorConversion;
        return new Units.Temperature(converted, Units.Temperature.UnitType.Celsius);
    }

    /// <summary>
    /// Reads the raw sensor data into the provided buffer.
    /// </summary>
    /// <param name="buffer">A span of 64 16-but values</param>
    public void ReadRawPixels(Span<short> buffer)
    {
        if (buffer.Length != Constants.PixelArraySize)
        {
            throw new ArgumentOutOfRangeException($"Expected a buffer of {Constants.PixelArraySize} shorts");
        }

        Span<byte> tx = stackalloc byte[1];

        tx[0] = (byte)Registers.PIXEL_OFFSET;
        _i2cComms.Write(tx);
        var rx = MemoryMarshal.Cast<short, byte>(buffer);
        _i2cComms.Read(rx);
    }

    /// <summary>
    /// Reads the temperature of each pixel in the sensor
    /// </summary>
    /// <returns></returns>
    public Units.Temperature[] ReadPixels()
    {
        var temps = new Units.Temperature[Constants.PixelArraySize];
        Span<short> raw = stackalloc short[Constants.PixelArraySize];

        ReadRawPixels(raw);

        for (var i = 0; i < raw.Length; i++)
        {
            var r = Convert12BitUShortToFloat((ushort)raw[i]) * Constants.PixelTempConversion;
            temps[i] = new Units.Temperature(r, Units.Temperature.UnitType.Celsius);
        }

        return temps;
    }

    private float Convert12BitUShortToFloat(ushort s)
    {
        // take 11 bits of value
        var absolute = s & 0x7ff;
        // apply any negative

        if ((absolute & 0x800) != 0)
        {
            return 0 - (float)absolute;
        }

        return absolute;
    }

    private void EnableInterrupts(InterruptModes mode)
    {
        WriteRegister(Registers.INTC, (byte)(0x01 | (byte)mode));
    }

    private void WriteRegister(Registers register, byte value)
    {
        _i2cComms.WriteRegister((byte)register, value);
    }

    private void WriteRegister(Registers register, Modes mode)
    {
        WriteRegister(register, (byte)mode);
    }

    private void WriteRegister(Registers register, Commands command)
    {
        WriteRegister(register, (byte)command);
    }
}