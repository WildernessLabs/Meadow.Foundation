using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Color;
using System;
using System.Buffers.Binary;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Color;

/// <summary>
/// Represents a Tcs3472x color sensor.
/// </summary>
public partial class Tcs3472x : ByteCommsSensorBase<Meadow.Color>, IColorSensor, II2cPeripheral
{
    /// <summary>
    /// Default I2C address for the TCS3472x family.
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    /// <summary>
    /// Gets or sets the integration time for the sensor.
    /// </summary>
    public double IntegrationTime
    {
        get => integrationTime;
        set
        {
            integrationTime = value;
            SetIntegrationTime(integrationTime);
        }
    }

    /// <summary>
    /// Gets or sets the gain.
    /// </summary>
    public Gain CurrentGain
    {
        get => gain;
        set
        {
            gain = value;
            WriteRegister(Registers.CONTROL, (byte)gain);
        }
    }

    /// <summary>
    /// Gets the last color read from the sensor.
    /// </summary>
    public Meadow.Color? Color => Conditions;

    private double integrationTime;
    private bool isLongIntegrationTime;
    private byte integrationTimeByte;
    private Gain gain;

    /// <summary>
    /// Creates a new Tcs3472x color sensor object.
    /// </summary>
    /// <param name="i2cBus">The I2C bus to use.</param>
    /// <param name="address">The I2C address of the sensor.</param>
    /// <param name="integrationTime">The integration time for color measurement.</param>
    /// <param name="gain">The gain for color measurement.</param>
    public Tcs3472x(II2cBus i2cBus,
        byte address = (byte)Addresses.Default,
        double integrationTime = 0.0024,
        Gain gain = Gain.Gain16x)
        : base(i2cBus, address)
    {
        IntegrationTime = integrationTime;
        CurrentGain = gain;
        Initialize();
    }

    private void Initialize()
    {
        BusComms.Write((byte)(Registers.COMMAND_BIT | Registers.ID));
        SetIntegrationTime(IntegrationTime);
        PowerOn();
    }

    private void SetIntegrationTime(double timeSeconds)
    {
        if (timeSeconds <= 0.7)
        {
            if (isLongIntegrationTime)
            {
                SetConfigLongTime(false);
            }

            isLongIntegrationTime = false;
            var timeByte = Math.Clamp((int)(0x100 - (timeSeconds / 0.0024)), 0, 255);
            WriteRegister(Registers.ATIME, (byte)timeByte);
            integrationTimeByte = (byte)timeByte;
        }
        else
        {
            if (!isLongIntegrationTime)
            {
                SetConfigLongTime(true);
            }

            isLongIntegrationTime = true;
            var timeByte = Math.Clamp((int)(0x100 - (timeSeconds / 0.029)), 0, 255);
            WriteRegister(Registers.WTIME, (byte)timeByte);
            integrationTimeByte = (byte)timeByte;
        }
    }

    private void SetConfigLongTime(bool setLong)
    {
        WriteRegister(Registers.CONFIG, setLong ? (byte)Registers.CONFIG_WLONG : (byte)0x00);
    }

    private void PowerOn()
    {
        WriteRegister(Registers.ENABLE, (byte)Registers.ENABLE_PON);
        Task.Delay(10).Wait();
        WriteRegister(Registers.ENABLE, (byte)(Registers.ENABLE_PON | Registers.ENABLE_AEN));
    }

    private void PowerOff()
    {
        var powerState = ReadRegister(Registers.ENABLE);
        powerState = (byte)(powerState & ~(byte)(Registers.ENABLE_PON | Registers.ENABLE_AEN));
        WriteRegister(Registers.ENABLE, powerState);
    }

    /// <summary>
    /// Sets or clears the colors and clear interrupts.
    /// </summary>
    /// <param name="state">True to set all interrupts, false to clear.</param>
    public void SetInterrupt(bool state)
    {
        SetInterrupt(InterruptState.All, state);
    }

    /// <summary>
    /// Sets or clears a specific interrupt persistence.
    /// </summary>
    /// <param name="interrupt">The persistence cycles.</param>
    /// <param name="state">True to set the interrupt, false to clear.</param>
    public void SetInterrupt(InterruptState interrupt, bool state)
    {
        WriteRegister(Registers.PERS, (byte)interrupt);
        var enable = ReadRegister(Registers.ENABLE);
        enable = state
                ? enable |= (byte)Registers.ENABLE_AIEN
                : enable = (byte)(enable & ~(byte)Registers.ENABLE_AIEN);
        WriteRegister(Registers.ENABLE, enable);
    }

    /// <summary>
    /// Gets the current color reading from the sensor.
    /// </summary>
    /// <param name="delay">Wait to read the data that the integration time has passed.</param>
    /// <returns>The current color reading.</returns>
    public async Task<Meadow.Color> GetColor(bool delay = true)
    {
        if (delay)
        {
            await Task.Delay((int)(IntegrationTime * 1000));
        }

        var divide = (256 - integrationTimeByte) * 1024 * 12;

        if (isLongIntegrationTime)
        {
            divide *= 12;
        }

        var r = Read16BitValue(Registers.RDATAL) * 255 / divide;
        var g = Read16BitValue(Registers.GDATAL) * 255 / divide;
        var b = Read16BitValue(Registers.BDATAL) * 255 / divide;
        var a = Read16BitValue(Registers.CDATAL) * 255 / divide;

        return new Meadow.Color(
            (byte)Math.Clamp(r, 0, 255),
            (byte)Math.Clamp(g, 0, 255),
            (byte)Math.Clamp(b, 0, 255),
            (byte)Math.Clamp(a, 0, 255));
    }

    /// <inheritdoc />
    protected override Task<Meadow.Color> ReadSensor()
    {
        return GetColor();
    }

    private ushort Read16BitValue(Registers register)
    {
        BusComms.Write((byte)(Registers.COMMAND_BIT | register));
        BusComms.Read(ReadBuffer.Span[0..2]);
        return BinaryPrimitives.ReadUInt16BigEndian(ReadBuffer.Span[0..2]);
    }

    private byte ReadRegister(Registers register)
    {
        return BusComms.ReadRegister((byte)(Registers.COMMAND_BIT | register));
    }

    private void WriteRegister(Registers register, byte data)
    {
        BusComms.Write(new byte[] { (byte)(Registers.COMMAND_BIT | register), data });
    }
}