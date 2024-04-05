using Meadow.Hardware;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Meadow.Foundation.Sensors.Power;

/// <summary>
/// Represents a INA228 Precision Digital Current and Power Monitor
/// </summary>
public class Ina228 : Ina2xx
{
    private readonly Units.Temperature _temperatureScale;
    private Units.Voltage _shuntVoltageScale;
    private Units.Energy _energyScale;
    private double _chargeScale;
    private ushort _calibration = 0x1000; // default on startup

    /// <summary>
    /// Create a new INA228 object
    /// </summary>
    /// <param name="i2cBus">The I2C bus</param>
    /// <param name="address">The I2C address</param>
    /// <param name="shuntResistance">Resistor value (in Ohms) used for measuring current. Default matches Adafruit's Breakout board. </param>
    public Ina228(II2cBus i2cBus, byte address = (byte)Addresses.Default, double shuntResistance = 0.015)
        : base(i2cBus, address)
    {
        _shuntResistor = new Units.Resistance(shuntResistance);
        _voltageScale = new Units.Voltage(195.3125, Units.Voltage.UnitType.Microvolts);
        _shuntVoltageScale = new Units.Voltage(312.5, Units.Voltage.UnitType.Nanovolts);
        _temperatureScale = new Units.Temperature(7.8125e-6);
        // TODO: set scaling/calibration dynamically
        _currentScale = new Units.Current(_calibration / (13107.2 * 1e6 * _shuntResistor.Ohms));
        _powerScale = new Units.Power(3.2 * _currentScale.Amps);
        _energyScale = new Units.Energy(_powerScale.Watts * 16);
        _chargeScale = _currentScale.Amps;

        ReadDeviceInfo();
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Configure(false, false, 0);
        ConfigureConversion(Mode.ContinuousAll, Averaging.Average_1,
            voltageTime: ConversionTime.ConversionTime_1052us,
            currentTime: ConversionTime.ConversionTime_1052us,
            temperatureTime: ConversionTime.ConversionTime_1052us
            );
    }

    /// <summary>
    /// Sets Configuration
    /// </summary>
    public void Configure(bool shuntGain4x = false, bool temperatureCompensation = false, ushort initialConversionDelayMs = 0)
    {
        if (initialConversionDelayMs > 510)
            throw new ArgumentOutOfRangeException(nameof(initialConversionDelayMs), initialConversionDelayMs, null);
        ushort config = (ushort)((shuntGain4x ? 0x10 : 0x00) | (temperatureCompensation ? 0x20 : 0x00) | ((initialConversionDelayMs / 2) << 6));
        WriteRegister(Registers.Config, config);
        IsConfigured = true;
    }

    /// <summary>
    /// Sets Configuration of ADC conversions
    /// </summary>
    /// <param name="mode">selection of values and trigger mode</param>
    /// <param name="averaging">On-chip value averaging</param>
    /// <param name="voltageTime">Conversion time for Voltage measurements</param>
    /// <param name="currentTime">Conversion time for Current measurements</param>
    /// <param name="temperatureTime">Conversion time for Die Temperature measurements</param>
    public void ConfigureConversion(Mode mode = Mode.ContinuousAll,
        Averaging averaging = Averaging.Average_1,
        ConversionTime voltageTime = ConversionTime.ConversionTime_1052us,
        ConversionTime currentTime = ConversionTime.ConversionTime_1052us,
        ConversionTime temperatureTime = ConversionTime.ConversionTime_1052us
        )
    {
        ushort adcConfig = (ushort)((ushort)mode << 12 | ((ushort)voltageTime << 9) | ((ushort)currentTime << 6) | ((ushort)temperatureTime << 3) | (ushort)averaging);
        WriteRegister(Registers.ADCConfig, adcConfig);
    }

    /// <summary>
    /// _currentScale and _powerScale are set along with _calibration using logic outlined in the datasheet for the INA228
    /// </summary>
    /// <param name="maxExpectedCurrent">Maximum expected Current in the specific application.</param>
    /// <param name="shuntGain4x"><c>true</c> applies 4x gain to shunt measurements</param>
    public void SetCalibration(Units.Current maxExpectedCurrent, bool shuntGain4x = false)
    {
        // apply shuntGain4x to the config register (Read, set/clear bit, write)
        ushort config = ReadRegisterAsUShort(Registers.Config);
        config = (ushort)((config & ~0x10) | (shuntGain4x ? 0x10 : 0x00));
        WriteRegister(Registers.Config, config);

        // shuntGain4x also affects the calibration and other current derived values
        _shuntVoltageScale = new Units.Voltage(shuntGain4x ? 78.125 : 312.5, Units.Voltage.UnitType.Nanovolts);
        var maxShuntVoltage = new Units.Voltage(shuntGain4x ? 40.96 : 163.84, Units.Voltage.UnitType.Millivolts);
        var maxCurrent = maxShuntVoltage.Volts / _shuntResistor.Ohms;
        var currentLSB = Math.Min(maxExpectedCurrent.Amps, maxCurrent) / (1 << 19);
        _calibration = (ushort)(13107.2 * 1e6 * currentLSB * _shuntResistor.Ohms * (shuntGain4x ? 4 : 1));
        WriteRegister(Registers.ShuntCal, _calibration);

        _currentScale = new Units.Current(currentLSB);
        _powerScale = new Units.Power(_currentScale.Amps * 3.2);
        _energyScale = new Units.Energy(_powerScale.Watts * 16);
        _chargeScale = _currentScale.Amps;
    }

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

    #region Measurement
    /// <inheritdoc/>
    public override Units.Current ReadCurrent()
    {
        Span<byte> buffer = stackalloc byte[3];
        ReadRegister(Registers.Current, buffer);
        var sign = buffer[0] >= 0x80;
        int register = (sign ? 0x0FFF : 0x0000) << 20 | buffer[0] << 12 | buffer[1] << 4 | buffer[2] >> 4;
        return new Units.Current(register * _currentScale.Amps);
    }

    /// <inheritdoc/>
    public override Units.Voltage ReadBusVoltage()
    {
        Span<byte> buffer = stackalloc byte[3];
        ReadRegister(Registers.BusVoltage, buffer);
        int register = buffer[0] << 12 | buffer[1] << 4 | buffer[2] >> 4;
        return new Units.Voltage(register * _voltageScale.Volts);
    }

    /// <inheritdoc/>
    public override Units.Voltage ReadShuntVoltage()
    {
        Span<byte> buffer = stackalloc byte[3];
        ReadRegister(Registers.ShuntVoltage, buffer);
        var sign = buffer[0] >= 0x80;
        int register = (sign ? 0x0FFF : 0x0000) << 20 | buffer[0] << 12 | buffer[1] << 4 | buffer[2] >> 4;
        return new Units.Voltage(register * _shuntVoltageScale.Volts);
    }

    /// <inheritdoc/>
    public override Units.Power ReadPower()
    {
        Span<byte> buffer = stackalloc byte[3];
        ReadRegister(Registers.Power, buffer);
        uint register = (uint)(buffer[0] << 16 | buffer[1] << 8 | buffer[2]);
        return new Units.Power(register * _powerScale.Watts);
    }

    /// <summary>
    /// Read the Energy accumulator from the power monitor IC.
    /// </summary>
    /// <returns><see cref="Units.Energy"/> representing the energy accumulator value.</returns>
    public Units.Energy ReadEnergyAccumulator()
    {
        Span<byte> buffer = stackalloc byte[5];
        ReadRegister(Registers.Energy, buffer);
        long register = (long)buffer[0] << 32 | (long)(buffer[1] << 24 | buffer[2] << 16 | buffer[3] << 8 | buffer[4]);
        return new Units.Energy(register * _energyScale.Joules);
    }

    /// <summary>
    /// Read the Charge accumulator from the power monitor IC.
    /// </summary>
    /// <returns><see cref="Units.Charge"/> representing the charge accumulator value.</returns>
    public double ReadChargeAccumulator()
    {
        Span<byte> buffer = stackalloc byte[5];
        ReadRegister(Registers.Charge, buffer);
        long register = (long)buffer[0] << 32 | (long)(buffer[1] << 24 | buffer[2] << 16 | buffer[3] << 8 | buffer[4]);
        return register * _chargeScale;
    }

    /// <summary>
    /// Resets the Energy and Charge accumulators on the power monitor IC.
    /// </summary>
    public void ResetAccumulators()
    {
        // Read config, set bit, write config
        ushort config = ReadRegisterAsUShort(Registers.Config);
        config |= 0x4000;
        WriteRegister(Registers.Config, config);
    }

    #endregion

    #region Enumerations
    /// <summary>
    /// Enumeration of supported operation modes.
    /// </summary>
    public enum Mode
    {
        /// <summary> Power down (no conversions). </summary>
        PowerDown = 0x0,
        /// <summary> Measure Voltage once. </summary>
        TriggeredVoltage = 0x1,
        /// <summary> Measure Current once. </summary>
        TriggeredCurrent = 0x2,
        /// <summary> Measure Voltage and Current once. </summary>
        TriggeredVoltageCurrent = 0x3,
        /// <summary> Measure Die Temperature once. </summary>
        TriggeredTemperature = 0x4,
        /// <summary> Measure Voltage and Die Temperature once. </summary>
        TriggeredTemperatureVoltage = 0x5,
        /// <summary> Measure Current and Die Temperature once. </summary>
        TriggeredTemperatureCurrent = 0x6,
        /// <summary> Measure Voltage, Current, and Die Temperature once. </summary>
        TriggeredAll = 0x7,
        /// <summary> Power down (no conversions). </summary>
        PowerDown2 = 0x8,
        /// <summary> Measure Voltage continuously. </summary>
        ContinuousVoltage = 0x9,
        /// <summary> Measure Current continuously. </summary>
        ContinuousCurrent = 0xA,
        /// <summary> Measure Voltage and Current continuously. </summary>
        ContinuousVoltageCurrent = 0xB,
        /// <summary> Measure Die Temperature continuously. </summary>
        ContinuousTemperature = 0xC,
        /// <summary> Measure Voltage and Die Temperature continuously. </summary>
        ContinuousTemperatureVoltage = 0xD,
        /// <summary> Measure Current and Die Temperature continuously. </summary>
        ContinuousTemperatureCurrent = 0xE,
        /// <summary> Measure Voltage, Current, and Die Temperature continuously (default). </summary>
        ContinuousAll = 0xF, // default at POR
    }

    /// <summary>
    /// Enumeration of supported ADC conversion times.
    /// </summary>
    /// <remarks>Note: This is used for Voltage, Current, and Die Temperature with different amounts of bit shifting</remarks>
    public enum ConversionTime
    {
        /// <summary> 50 µs ADC conversion. </summary>
        ConversionTime_50us = 0x00,
        /// <summary> 84 µs ADC conversion. </summary>
        ConversionTime_84us = 0x01,
        /// <summary> 150 µs ADC conversion. </summary>
        ConversionTime_150us = 0x02,
        /// <summary> 280 µs ADC conversion. </summary>
        ConversionTime_280us = 0x03,
        /// <summary> 540 µs ADC conversion. </summary>
        ConversionTime_540us = 0x04,
        /// <summary> 1.052 ms ADC conversion. (default) </summary>
        ConversionTime_1052us = 0x05, // default at POR
        /// <summary> 2.074 ms ADC conversion. </summary>
        ConversionTime_2074us = 0x06,
        /// <summary> 4.12 ms ADC conversion. </summary>
        ConversionTime_4120us = 0x07,
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

    /// <summary>
    /// Enumeration of device memory registers.
    /// </summary>
    /// <remarks> Note: Some registers are wider than 16 bits. </remarks>>
    private enum Registers : byte
    {
        Config = 0x00,
        ADCConfig = 0x01,
        ShuntCal = 0x02,
        ShuntVoltage = 0x04, // 24 bits
        BusVoltage = 0x05, // 24 bits
        DieTemp = 0x06,
        Current = 0x07, // 24 bits
        Power = 0x08, // 24 bits
        Energy = 0x09, // 40 bits
        Charge = 0x0A, // 40 bits
        DiagnosticAlert = 0x0B,
        ManufacturerID = 0x3E,
        DeviceID = 0x3F
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