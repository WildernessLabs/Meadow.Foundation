using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;

namespace Meadow.Foundation.Sensors.Battery;

/// <summary>
/// Base class for MAX1704x fuel gauge family, which monitors battery state of charge for lithium-ion batteries
/// </summary>
public abstract partial class Max1704x : II2cPeripheral, ISensor
{
    /// <summary>
    /// Event that fires when battery charge falls below alert threshold
    /// </summary>
    /// <remarks>
    /// The event provides the current state of charge as a percentage (0-100)
    /// </remarks>
    public event EventHandler<double>? LowChargeAlert;

    private readonly I2cCommunications i2c;

    /// <inheritdoc/>
    public byte DefaultI2cAddress => 0x36; // this sensor only supports this address

    /// <summary>
    /// Gets the version of the MAX1704x chip
    /// </summary>
    public byte Version { get; }

    private double vbattScale;
    private IDigitalInterruptPort? alertPort;
    private bool alertPortCreated;

    /// <summary>
    /// Initializes a new instance of the MAX1704x fuel gauge
    /// </summary>
    /// <param name="i2cBus">The I2C bus connected to the fuel gauge</param>
    /// <param name="vbattScale">Scaling factor for battery voltage calculation</param>
    protected Max1704x(
        II2cBus i2cBus,
        double vbattScale
        )
    {
        this.vbattScale = vbattScale;
        i2c = new I2cCommunications(i2cBus, DefaultI2cAddress, 2);
        var v = i2c.ReadRegisterAsUShort((byte)Registers.Version);
        Version = (byte)(v & 0xff);
        Resolver.Log.Info($"Version: {Version} v:0x{v:x}");
    }

    /// <summary>
    /// Initializes a new instance of the MAX1704x fuel gauge with alert functionality
    /// </summary>
    /// <param name="i2cBus">The I2C bus connected to the fuel gauge</param>
    /// <param name="vbattScale">Scaling factor for battery voltage calculation</param>
    /// <param name="alertInterruptPin">Pin connected to the ALRT output of the gauge</param>
    /// <param name="alertThresholdPercent">Battery percentage threshold to trigger alert (1-32%)</param>
    protected Max1704x(
        II2cBus i2cBus,
        double vbattScale,
        IPin alertInterruptPin,
        int alertThresholdPercent = 25
        )
        : this(i2cBus, vbattScale)
    {
        alertPort = alertInterruptPin.CreateDigitalInterruptPort(
            InterruptMode.EdgeFalling, ResistorMode.InternalPullUp);
        alertPortCreated = true;
        alertPort.Changed += OnAlertInterrupt;
    }

    /// <summary>
    /// Handles interrupt events from the ALRT pin
    /// </summary>
    /// <param name="sender">The source of the event</param>
    /// <param name="e">The interrupt state</param>
    private void OnAlertInterrupt(object sender, DigitalPortResult e)
    {
        LowChargeAlert?.Invoke(this, ReadStateOfCharge());
    }

    /// <summary>
    /// Reads the current battery voltage
    /// </summary>
    /// <returns>The battery voltage in volts</returns>
    public Voltage ReadVoltage()
    {
        var raw = i2c.ReadRegisterAsUShort((byte)Registers.VCell, ByteOrder.BigEndian);
        return new Voltage((raw >> 4) * vbattScale / 1000, Voltage.UnitType.Volts);
    }

    /// <summary>
    /// Reads the current state of charge (battery percentage)
    /// </summary>
    /// <returns>Battery percentage from 0.0 to 100.0</returns>
    public double ReadStateOfCharge()
    {
        var raw = i2c.ReadRegisterAsUShort((byte)Registers.SoC, ByteOrder.BigEndian);
        var soc = (raw >> 8) + (raw & 0xff) / 256d;
        return soc;
    }

    /// <summary>
    /// Sets the alert threshold for low battery
    /// </summary>
    /// <param name="percent">Alert threshold percentage (1-32)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when percent is outside the valid range of 1-32</exception>
    /// <remarks>
    /// The MAX17043 alert threshold is set in 1% increments from 1% to 32%.
    /// The threshold is stored in the lower 5 bits of the CONFIG register.
    /// Register values: 0 = 32%, 1 = 31%, ..., 31 = 1%
    /// </remarks>
    public void SetAlertThreshold(int percent)
    {
        // The MAX17043 alert threshold is set in 1% increments from 1% to 32%
        // The threshold is stored in the lower 5 bits of the CONFIG register
        // 0 = 32%, 1 = 31%, ..., 31 = 1%
        if (percent < 0 || percent > 32) throw new ArgumentOutOfRangeException();
        byte thresholdValue = (byte)(32 - percent);
        byte[] configData = new byte[2];
        var register = i2c.ReadRegisterAsUShort((byte)Registers.Config, ByteOrder.BigEndian);
        register &= 0xFFE0;
        register |= thresholdValue;
        i2c.WriteRegister((byte)Registers.Config, register, ByteOrder.BigEndian);
    }

    /// <summary>
    /// Checks if the alert flag is currently set
    /// </summary>
    /// <returns>True if the alert flag is set, otherwise false</returns>
    /// <remarks>
    /// The ALRT bit is bit 5 of the CONFIG register
    /// </remarks>
    public bool IsAlertSet()
    {
        ushort register = i2c.ReadRegisterAsUShort((byte)Registers.Config, ByteOrder.BigEndian);
        return (register & 0x20) == 0x20;
    }

    /// <summary>
    /// Clears the alert flag
    /// </summary>
    /// <remarks>
    /// Clears the ALRT bit (bit 5) in the CONFIG register
    /// </remarks>
    public void ClearAlert()
    {
        ushort register = i2c.ReadRegisterAsUShort((byte)Registers.Config, ByteOrder.BigEndian);
        register = (ushort)(register & ~0x20); // ~0x20 = 11011111
        i2c.WriteRegister((byte)Registers.Config, register, ByteOrder.BigEndian);
    }

    /// <summary>
    /// Sets or clears the sleep mode of the fuel gauge
    /// </summary>
    /// <param name="sleep">True to enter sleep mode, false to exit sleep mode</param>
    /// <remarks>
    /// In sleep mode, the IC halts all operations to reduce power consumption.
    /// The SLEEP bit is bit 7 of the CONFIG register.
    /// </remarks>
    public void SetSleepMode(bool sleep)
    {
        ushort register = i2c.ReadRegisterAsUShort((byte)Registers.Config, ByteOrder.BigEndian);
        if (sleep)
        {
            register |= 0b1000_0000;
        }
        else
        {
            register &= 0b0111_1111;
        }
        i2c.WriteRegister((byte)Registers.Config, register, ByteOrder.BigEndian);
    }
}