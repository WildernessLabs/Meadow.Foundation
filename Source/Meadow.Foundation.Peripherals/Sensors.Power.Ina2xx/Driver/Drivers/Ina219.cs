using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Runtime.CompilerServices;

namespace Meadow.Foundation.Sensors.Power;

/// <summary>
/// Represents a INA219 Precision Digital Current and Power Monitor
/// </summary>
public class Ina219 : Ina2xx
{
    private readonly Voltage _shuntVoltageScale;
    private Current _maxExpectedCurrent;
    private ushort _calibration;

    /// <summary>
    /// Create a new INA219 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">The I2C address</param>
    /// <param name="shuntResistance">Resistance used for measuring current. <c>null</c> uses default of 0.1 Ohms, to match Adafruit's INA219 breakout board.</param>
    protected Ina219(II2cBus i2cBus, byte address, Units.Resistance shuntResistance)
        : base(i2cBus, address)
    {
        _voltageScale = new Voltage(4, Units.Voltage.UnitType.Millivolts);
        _shuntVoltageScale = new Voltage(10, Units.Voltage.UnitType.Microvolts);
        _shuntResistor = shuntResistance;
    }

    /// <summary>
    /// Create a new INA219 object, with the default current and resistor values to match Adafruit's INA219 breakout board.
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">The I2C address</param>
    public Ina219(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        : this(i2cBus, address, new Resistance(0.1))
    { }

    /// <inheritdoc/>
    public override void Configure()
    {
        Configure(BusVoltageRange.Range_32V, ADCModes.ADCMode_12bit_532us,
            ShuntVoltageRange.Range_320mV, ADCModes.ADCMode_12bit_532us,
            Mode.ContinuousAll);
    }

    /// <summary>
    /// Sets Configuration of sensor
    /// </summary>
    /// <param name="busVoltageRange"><see cref="BusVoltageRange"/> to use for Bus Voltage measurement</param>
    /// <param name="busADCMode">ADC resolution/averaging for Bus Voltage</param>
    /// <param name="shuntVoltageRange"><see cref="ShuntVoltageRange"/> to use for Shunt Voltage measurement</param>
    /// <param name="shuntADCMode">ADC resolution/averaging for Shunt Voltage</param>
    /// <param name="mode">selection of values and trigger mode</param>
    public void Configure(
        BusVoltageRange busVoltageRange = BusVoltageRange.Range_32V,
        ADCModes busADCMode = ADCModes.ADCMode_12bit_532us,
        ShuntVoltageRange shuntVoltageRange = ShuntVoltageRange.Range_320mV,
        ADCModes shuntADCMode = ADCModes.ADCMode_12bit_532us,
        Mode mode = Mode.ContinuousAll
        )
    {
        ushort config = (ushort)((ushort)busVoltageRange | (ushort)shuntVoltageRange | (ushort)busADCMode << 7 | (ushort)shuntADCMode << 3 | (ushort)mode);
        WriteRegister(Registers.Config, config);
        if (_calibration > 0)
        {
            WriteRegister(Registers.Calibration, _calibration);
        }

        IsConfigured = true;
    }

    /// <summary>
    /// Sets Configuration of sensor, using best shunt voltage range possible based on the max expected current.
    /// </summary>
    /// <param name="busVoltageRange">voltage measurement range</param>
    /// <param name="maxExpectedCurrent">Maximum expected current for the application.</param>
    /// <param name="adcMode">ADC resolution/averaging setting common to both ADCs</param>
    /// <param name="mode">selection of values and trigger mode</param>
    public void Configure(
        BusVoltageRange busVoltageRange,
        Current maxExpectedCurrent,
        ADCModes adcMode = ADCModes.ADCMode_12bit_532us,
        Mode mode = Mode.ContinuousAll)
    {
        _maxExpectedCurrent = maxExpectedCurrent;
        var shuntVoltageRange = SetCalibration(_maxExpectedCurrent); // NOTE: Calibration register needs to be calculated and populated before Current or Power registers will work.
        Configure(busVoltageRange, adcMode, shuntVoltageRange, adcMode, mode);
    }

    /// <summary>
    /// _currentScale and _powerScale are set along with _calibration using logic outlined in the datasheet for the INA219
    /// </summary>
    /// <param name="maxExpectedCurrent">Maximum expected Current in the specific application.</param>
    /// <returns>suggested <see cref="ShuntVoltageRange"/> to use in configuration</returns>
    private ShuntVoltageRange SetCalibration(Current maxExpectedCurrent)
    {
        var maxShuntVoltage = 0.32;
        var minimumLSB = maxExpectedCurrent.Microamps / 0x7FFF;
        var maximumLSB = maxExpectedCurrent.Microamps / 0x1000;
        var selectedLSB = Math.Ceiling(minimumLSB);
        _currentScale = new Current(selectedLSB, Units.Current.UnitType.Microamps);
        _powerScale = new Units.Power(_currentScale.Amps * 20, Units.Power.UnitType.Watts);
        _calibration = (ushort)(0.04096 / (_currentScale.Amps * _shuntResistor.Ohms));

        var maxCurrentBeforeOverflow = Math.Min(maxShuntVoltage / _shuntResistor.Ohms, _currentScale.Amps * 0x7FFF);
        var maxShuntVoltageBeforeOverflow = Math.Min(maxExpectedCurrent.Amps * _shuntResistor.Ohms, maxShuntVoltage);

        return maxShuntVoltageBeforeOverflow switch
        {
            <= 0.04 => ShuntVoltageRange.Range_40mV,
            <= 0.08 => ShuntVoltageRange.Range_80mV,
            <= 0.16 => ShuntVoltageRange.Range_160mV,
            _ => ShuntVoltageRange.Range_320mV,
        };
    }

    /// <inheritdoc/>
    public override Units.Current ReadCurrent()
    {
        var rawRegister = ReadRegisterAsUShort(Registers.Current);
        return new Current((short)rawRegister * _currentScale.Amps);
    }

    /// <inheritdoc/>
    public override Units.Voltage ReadBusVoltage()
    {
        var rawRegister = ReadRegisterAsUShort(Registers.BusVoltage);
        return new Voltage((rawRegister >> 3) * _voltageScale.Volts);
    }

    /// <inheritdoc/>
    public override Units.Voltage ReadShuntVoltage()
    {
        var rawRegister = ReadRegisterAsUShort(Registers.ShuntVoltage);
        return new Voltage(rawRegister * _shuntVoltageScale.Volts);
    }

    /// <inheritdoc/>
    public override Units.Power ReadPower() => new Units.Power(ReadRegisterAsUShort(Registers.Power) * _powerScale.Watts);

    /// <inheritdoc/>
    internal override void ReadDeviceInfo()
    {
        ManufacturerID = string.Empty;
        DeviceID = DeviceRevision = 0;
    }

    #region Enumerations
    private enum Registers : byte
    {
        Config = 0x00,
        ShuntVoltage = 0x01,
        BusVoltage = 0x02,
        Power = 0x03,
        Current = 0x04,
        Calibration = 0x05,
    }

    /// <summary>
    /// Enumeration of supported voltage measurement ranges.
    /// </summary>
    /// <remarks>There is no actual reason to not use 32V, as it has the same resolution.</remarks>
    public enum BusVoltageRange : ushort
    {
        /// <summary> 16 Volt measurement range. </summary>
        Range_16V = 0x0000,
        /// <summary> 32 Volt measurement range. </summary>
        Range_32V = 0x2000,
    }

    /// <summary>
    /// Enumeration of supported shunt voltage measurement ranges.
    /// </summary>
    public enum ShuntVoltageRange : ushort
    {
        /// <summary> 40 millivolt measurement range. </summary>
        Range_40mV = 0x0000,
        /// <summary> 80 millivolt measurement range. </summary>
        Range_80mV = 0x0800,
        /// <summary> 160 millivolt measurement range. </summary>
        Range_160mV = 0x1000,
        /// <summary> 320 millivolt measurement range. </summary>
        Range_320mV = 0x1800,
    }

    /// <summary>
    /// Enumeration of supported ADC Averaging and Conversion time settings.
    /// </summary>
    public enum ADCModes : byte
    {
        /// <summary> 9-bit measurement resolution with 84 µs conversion time. </summary>
        ADCMode_9bit_84us = 0x00,
        /// <summary> 10-bit measurement resolution with 148 µs conversion time. </summary>
        ADCMode_10bit_148us = 0x01,
        /// <summary> 11-bit measurement resolution with 276 µs conversion time. </summary>
        ADCMode_11bit_276us = 0x02,
        /// <summary> 12-bit measurement resolution with 532 µs conversion time. </summary>
        ADCMode_12bit_532us = 0x03,
        /// <summary> 12-bit measurement resolution with 532 µs conversion time, averaging 1 sample. </summary>
        ADCMode_1xAvg_532us = 0x08,
        /// <summary> 12-bit measurement resolution with 1.064 ms conversion time, averaging 2 samples. </summary>
        ADCMode_2xAvg_1064us = 0x09,
        /// <summary> 12-bit measurement resolution with 2.128 ms conversion time, averaging 4 samples. </summary>
        ADCMode_4xAvg_2128us = 0x0A,
        /// <summary> 12-bit measurement resolution with 4.256 ms conversion time, averaging 8 samples. </summary>
        ADCMode_8xAvg_4256us = 0x0B,
        /// <summary> 12-bit measurement resolution with 8.512 ms conversion time, averaging 16 samples. </summary>
        ADCMode_16xAvg_8512us = 0x0C,
        /// <summary> 12-bit measurement resolution with 17.024 ms conversion time, averaging 32 samples. </summary>
        ADCMode_32xAvg_17024us = 0x0D,
        /// <summary> 12-bit measurement resolution with 34.048 ms conversion time, averaging 64 samples. </summary>
        ADCMode_64xAvg_34048us = 0x0E,
        /// <summary> 12-bit measurement resolution with 68.096 ms conversion time, averaging 128 samples. </summary>
        ADCMode_128xAvg_68096us = 0x0F,

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

}