using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Meadow.Foundation.Sensors.Power;

/// <summary>
/// Represents a INA260 Precision Digital Current and Power Monitor
/// </summary>
public class Ina260 : Ina2xx
{
    /// <summary>
    /// Create a new INA260 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">The I2C address</param>
    /// <param name="alertPort"><see cref="IDigitalInterruptPort"/> that can be used to monitor for <b>Alert</b> outputs, triggering <see cref="AlertChanged"/></param>
    public Ina260(II2cBus i2cBus, byte address = (byte)Addresses.Default, IDigitalInterruptPort? alertPort = null)
        : base(i2cBus, address)
    {
        _voltageScale = new Voltage(1.25, Units.Voltage.UnitType.Millivolts);
        _currentScale = new Current(1.25, Units.Current.UnitType.Milliamps);
        _powerScale = new Units.Power(10, Units.Power.UnitType.Milliwatts);
        _shuntResistor = new Resistance(2, Resistance.UnitType.Milliohms);      // Integrated in IC

        _alertPort = alertPort;

        if (_alertPort != null)
        {
            // TODO: configure _alertPort for open-drain active low signaling.
            //_alertPort.Resistor = ; 
            _alertPort.Changed += AlertPortOnChanged;
        }

        ReadDeviceInfo();
    }

    /// <summary>
    /// Create a new INA2xx object, using the address pin connections to calculate the correct I2C address.
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="a0"><see cref="Ina2xx.AddressConnection"/> specifying what A0 is connected to.</param>
    /// <param name="a1"><see cref="Ina2xx.AddressConnection"/> specifying what A1 is connected to.</param>
    public Ina260(II2cBus i2cBus, AddressConnection a0, AddressConnection a1)
        : this(i2cBus, GetAddress(a0, a1))
    { }

    /// <inheritdoc/>
    public override void Configure()
    {
        Configure(); // Call the implementation's version.
    }

    /// <summary>
    /// Sets Configuration of sensor
    /// </summary>
    /// <param name="mode">selection of values and trigger mode</param>
    /// <param name="currentConversionTime">Conversion time for Current measurements</param>
    /// <param name="voltageConversionTime">Conversion time for Voltage measurements</param>
    /// <param name="averaging">On-chip value averaging</param>
    public void Configure(ConversionTime currentConversionTime = ConversionTime.ConversionTime_1100us, 
        ConversionTime voltageConversionTime = ConversionTime.ConversionTime_1100us, 
        Averaging averaging = Averaging.Average_1,
        Mode mode = Mode.ContinuousAll)
    {
        ushort config = (ushort)((ushort)averaging << 9 | ((ushort)voltageConversionTime << 6) | ((ushort)currentConversionTime << 3) | (ushort)mode);
        BusComms.WriteRegister(ConfigRegister, config, ByteOrder.BigEndian);
    }

    /// <inheritdoc/>
    public override Units.Current ReadCurrent() => new Current((short)ReadRegisterAsUShort(Registers.Current) * _currentScale.Amps, Units.Current.UnitType.Amps);
    /// <inheritdoc/>
    public override Units.Voltage ReadBusVoltage() => new Voltage(ReadRegisterAsUShort(Registers.BusVoltage) * _voltageScale.Volts, Units.Voltage.UnitType.Volts);
    /// <inheritdoc/>
    /// <remarks> Ina260 doesn't directly have a register for this, so we compute from known values. </remarks>
    public override Units.Voltage ReadShuntVoltage() => new((ReadCurrent().Amps * _shuntResistor.Ohms), Units.Voltage.UnitType.Volts);
    /// <inheritdoc/>
    public override Units.Power ReadPower() => new Units.Power(ReadRegisterAsUShort(Registers.Power) * _powerScale.Watts, Units.Power.UnitType.Watts);

    // TODO: Helpers for triggered measurement?

    /// <inheritdoc/>
    internal override void ReadDeviceInfo()
    {
        Span<byte> buffer = stackalloc byte[2];
        ReadRegister(Registers.ManufacturerID, buffer);
        ManufacturerID = Encoding.ASCII.GetString(buffer);
        var deviceInfo = ReadRegisterAsUShort(Registers.DeviceID);
        DeviceID = (ushort)(deviceInfo >> 4);
        DeviceRevision = (byte)(deviceInfo & 0xF);
    }
    private readonly IDigitalInterruptPort? _alertPort;

    #region Alerts
    /// <summary>
    /// Raised when the Alert signal changes.
    /// </summary>
    public event EventHandler AlertChanged = default!;

    private void AlertPortOnChanged(object sender, DigitalPortResult e)
    {
        OnAlert(sender, EventArgs.Empty);
    }

    private void OnAlert(object sender, EventArgs e)
    {
        AlertChanged?.Invoke(sender, e);
    }

    private const double minVoltage = 0;
    private const double maxVoltage = 36;
    private const double minCurrent = -15;
    private const double maxCurrent = 15;
    private const double minPower = 0;
    private const double maxPower = 419.43;
    
    /// <summary>
    /// Configures the Voltage Limit Alert function.
    /// </summary>
    /// <param name="threshold"><see cref="Units.Voltage"/> threshold for triggering the Alert.</param>
    /// <param name="activeHigh">State of the alert pin to use for the Alert Signal</param>
    /// <param name="latching">Whether to latch the Alert until cleared.</param>
    /// <param name="overLimit">Set <c>true</c> to trigger an alert when the measured voltage is <b>above</b> the <paramref name="threshold"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="threshold"/> is outside the limits of the power monitor IC.</exception>
    public void AlertOnVoltageLimit(Units.Voltage threshold, bool activeHigh = false, bool latching = false, bool overLimit = false)
    {
        if (threshold.Volts is < minVoltage or > maxVoltage)
            throw new ArgumentOutOfRangeException(nameof(threshold), threshold.Volts, null);

        var maskValue = ((overLimit ? MaskEnable.AlertOverVoltageLimit : MaskEnable.AlertUnderVoltageLimit) |
                         (activeHigh ? MaskEnable.AlertPolarity : 0) |
                         (latching ? MaskEnable.LatchEnable : 0));
        ushort limitValue = (ushort)(threshold.Volts / _voltageScale.Volts);

        WriteRegister(Registers.MaskEnable, (ushort)maskValue);
        WriteRegister(Registers.AlertLimit, limitValue);
    }

    /// <summary>
    /// Configures the Current Limit Alert function.
    /// </summary>
    /// <param name="threshold"><see cref="Units.Current"/> threshold for triggering the Alert.</param>
    /// <param name="activeHigh">State of the alert pin to use for the Alert Signal</param>
    /// <param name="latching">Whether to latch the Alert until cleared.</param>
    /// <param name="overLimit">Set <c>true</c> to trigger an alert when the measured Current is <b>above</b> the <paramref name="threshold"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="threshold"/> is outside the limits of the power monitor IC.</exception>
    public void AlertOnCurrentLimit(Units.Current threshold, bool activeHigh = false, bool latching = false, bool overLimit = false)
    {
        if (threshold.Amps is < minCurrent or > maxCurrent)
            throw new ArgumentOutOfRangeException(nameof(threshold), threshold.Amps, null);
            
        var maskValue = ((overLimit ? MaskEnable.AlertOverCurrentLimit : MaskEnable.AlertUnderCurrentLimit) |
                         (activeHigh ? MaskEnable.AlertPolarity : 0) |
                         (latching ? MaskEnable.LatchEnable : 0));
        // TODO: negative current is allowed.
        ushort limitValue = (ushort)(threshold.Amps / _currentScale.Amps);

        WriteRegister(Registers.MaskEnable, (ushort)maskValue);
        WriteRegister(Registers.AlertLimit, limitValue);
    }

    /// <summary>
    /// Configures the Power Limit Alert function.
    /// </summary>
    /// <param name="threshold"><see cref="Units.Power"/> threshold for triggering the Alert.</param>
    /// <param name="activeHigh">State of the alert pin to use for the Alert Signal</param>
    /// <param name="latching">Whether to latch the Alert until cleared.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="threshold"/> is outside the limits of the power monitor IC.</exception>
    public void AlertOnPowerLimit(Units.Power threshold, bool activeHigh = false, bool latching = false)
    {
        if (threshold.Watts is < minPower or > maxPower)
            throw new ArgumentOutOfRangeException(nameof(threshold), threshold.Watts, null);
        var maskValue = ((MaskEnable.AlertOverPowerLimit) |
                         (activeHigh ? MaskEnable.AlertPolarity : 0) |
                         (latching ? MaskEnable.LatchEnable : 0));
        ushort limitValue = (ushort)(threshold.Watts / _powerScale.Watts);

        WriteRegister(Registers.MaskEnable, (ushort)maskValue);
        WriteRegister(Registers.AlertLimit, limitValue);
    }

    /// <summary>
    /// Configures the Alert function to activate when all ADC conversions are complete.
    /// </summary>
    /// <param name="activeHigh">State of the alert pin to use for the Alert Signal</param>
    /// <param name="latching">Whether to latch the Alert until cleared.</param>
    public void AlertOnConversionComplete(bool activeHigh = false, bool latching = false)
    {
        var maskValue = ((MaskEnable.AlertConversionReady) |
                         (activeHigh ? MaskEnable.AlertPolarity : 0) |
                         (latching ? MaskEnable.LatchEnable : 0));

        WriteRegister(Registers.MaskEnable, (ushort)maskValue);
    }
    #endregion

    /// <summary>
    /// Reads all status bits.
    /// </summary>
    /// <param name="alert"><c>true</c> if the Alert condition was the source of the Alert pin output.</param>
    /// <param name="conversionReady"><c>true</c> if all conversions and calculations are completed since last time this bit was read.</param>
    /// <param name="overflow"><c>true</c> indicates the internal arithmetic for Power may have exceeded the maximum of 419.43 W</param>
    public void GetStatus(out bool alert, out bool conversionReady, out bool overflow)
    {
        var maskValue = (MaskEnable)ReadRegisterAsUShort(Registers.MaskEnable);
        alert = (maskValue & MaskEnable.AlertFunctionFlag) != 0;
        conversionReady = (maskValue & MaskEnable.ConversionReady) != 0;
        overflow = (maskValue & MaskEnable.MathOverFlow) != 0;
    }

    #region Enumerations
    private enum Registers : byte
    {
        Config = 0x00,
        Current = 0x01,
        BusVoltage = 0x02,
        Power = 0x03,
        MaskEnable = 0x06,
        AlertLimit = 0x07,
        ManufacturerID = 0xFE,
        DeviceID = 0xFF
    }

    /// <summary>
    /// Enumeration of supported operation modes.
    /// </summary>
    public enum Mode
    {
        /// <summary> Power down (no conversions). </summary>
        PowerDown = 0x0,
        /// <summary> Measure Current once. </summary>
        TriggeredCurrent = 0x1,
        /// <summary> Measure Voltage once. </summary>
        TriggeredVoltage = 0x2,
        /// <summary> Measure Voltage and Current once. </summary>
        TriggeredAll = 0x3,
        /// <summary> Power down (no conversions). </summary>
        PowerDown2 = 0x4,
        /// <summary> Measure Current continuously. </summary>
        ContinuousCurrent = 0x5,
        /// <summary> Measure Voltage continuously. </summary>
        ContinuousVoltage = 0x6,
        /// <summary> Measure Voltage and Current continuously (default). </summary>
        ContinuousAll = 0x7, // default at POR
    }

    /// <summary>
    /// Enumeration of supported ADC conversion times.
    /// </summary>
    /// <remarks>Note: This is used for Voltage and Current with different amounts of shifting</remarks>
    public enum ConversionTime
    {
        /// <summary> 140 µs ADC conversion. </summary>
        ConversionTime_140us = 0x00,
        /// <summary> 204 µs ADC conversion. </summary>
        ConversionTime_204us = 0x01,
        /// <summary> 332 µs ADC conversion. </summary>
        ConversionTime_332us = 0x02,
        /// <summary> 588 µs ADC conversion. </summary>
        ConversionTime_588us = 0x03,
        /// <summary> 1.1 ms ADC conversion. (default) </summary>
        ConversionTime_1100us = 0x04, // default at POR
        /// <summary> 2.116 ms ADC conversion. </summary>
        ConversionTime_2116us = 0x05,
        /// <summary> 4.156 ms ADC conversion. </summary>
        ConversionTime_4156us = 0x06,
        /// <summary> 8.244 ms ADC conversion. </summary>
        ConversionTime_8244us = 0x07,
    }

    /// <summary>
    /// Enumeration of supported ADC Sample Averaging values.
    /// </summary>
    public enum Averaging
    {
        /// <summary> No Averaging. (default) </summary>
        Average_1 = 0x00, // default at POR
        /// <summary> 4x Sample Averaging. </summary>
        Average_4 = 0x01,
        /// <summary> 16x Sample Averaging. </summary>
        Average_16 = 0x02,
        /// <summary> 64x Sample Averaging. </summary>
        Average_64 = 0x03,
        /// <summary> 128x Sample Averaging. </summary>
        Average_128 = 0x04,
        /// <summary> 256x Sample Averaging. </summary>
        Average_256 = 0x05,
        /// <summary> 512x Sample Averaging. </summary>
        Average_512 = 0x06,
        /// <summary> 1024x Sample Averaging. </summary>
        Average_1024 = 0x07,
    }

    [Flags]
    private enum MaskEnable : ushort
    {
        LatchEnable = 0x01,
        AlertPolarity = 0x02,
        MathOverFlow = 0x04,
        ConversionReady = 0x08,
        AlertFunctionFlag = 0x10,
        AlertConversionReady = 0x400,
        AlertOverPowerLimit = 0x800,
        AlertUnderVoltageLimit = 0x1000,
        AlertOverVoltageLimit = 0x2000,
        AlertUnderCurrentLimit = 0x4000,
        AlertOverCurrentLimit = 0x8000,
    }
    #endregion

    #region Shorthand
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteRegister(Registers register, byte value) => BusComms.WriteRegister((byte)register, value, ByteOrder.BigEndian);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteRegister(Registers register, ushort value) => BusComms.WriteRegister((byte)register, value, ByteOrder.BigEndian);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteRegister(Registers register, uint value) => BusComms.WriteRegister((byte)register, value, ByteOrder.BigEndian);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadRegister(Registers register, Span<byte> buffer) => BusComms.ReadRegister((byte)register, buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte ReadRegister(Registers register) => BusComms.ReadRegister((byte)register);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort ReadRegisterAsUShort(Registers register) => BusComms.ReadRegisterAsUShort((byte)register, ByteOrder.BigEndian);
    #endregion

    #region IDisposable extras
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_alertPort != null) 
                _alertPort.Changed -= AlertPortOnChanged;
            base.Dispose(disposing);
        }
    }
    #endregion
}