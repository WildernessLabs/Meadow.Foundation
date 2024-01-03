using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Peripherals.Sensors.Atmospheric;
using Meadow.Units;
using Meadow.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Atmospheric;

/// <summary>
/// Device driver for the Adafruit MPRLS Ported Pressure Sensor Breakout
/// </summary>
public partial class AdafruitMPRLS :
    ByteCommsSensorBase<(Pressure? Pressure, Pressure? RawPsiMeasurement)>,
    II2cPeripheral, IBarometricPressureSensor
{
    /// <summary>
    /// The default I2C address for the peripheral
    /// </summary>
    public byte DefaultI2cAddress => (byte)Addresses.Default;

    private event EventHandler<IChangeResult<Pressure>> _pressureHandlers;

    event EventHandler<IChangeResult<Pressure>> ISamplingSensor<Pressure>.Updated
    {
        add => _pressureHandlers += value;
        remove => _pressureHandlers -= value;
    }

    /// <summary>
    /// Set by the sensor, to tell us it has power.
    /// </summary>
    public bool IsDevicePowered { get; set; }

    /// <summary>
    /// Set by the sensor, to tell us it's busy.
    /// </summary>
    public bool IsDeviceBusy { get; set; }

    /// <summary>
    /// Set by the sensor, to tell us whether or not there's an issue with its own memory.
    /// </summary>
    public bool HasMemoryIntegrityFailed { get; set; }

    /// <summary>
    /// Returns the current raw pressure value in pounds per square inch (PSI)
    /// </summary>
    public Pressure? RawPsiMeasurement => Conditions.RawPsiMeasurement;

    /// <summary>
    /// Returns the current pressure reading
    /// </summary>
    public Pressure? Pressure => Conditions.Pressure;

    /// <summary>
    /// Indicates the sensor has reached its pressure limit.
    /// </summary>
    public bool InternalMathSaturated { get; set; }

    private readonly byte[] mprlsMeasurementCommand = { 0xAA, 0x00, 0x00 };

    private const int MINIMUM_PSI = 0;
    private const int MAXIMUM_PSI = 25;

    /// <summary>
    /// Represents an Adafruit MPRLS Ported Pressure Sensor
    /// </summary>
    /// <param name="i2cbus">I2Cbus connected to the sensor</param>
    public AdafruitMPRLS(II2cBus i2cbus)
        : base(i2cbus, (byte)Addresses.Default)
    { }

    /// <summary>
    /// Notify subscribers of PressureUpdated event handler
    /// </summary>
    /// <param name="changeResult"></param>
    protected override void RaiseEventsAndNotify(IChangeResult<(Pressure? Pressure, Pressure? RawPsiMeasurement)> changeResult)
    {
        if (changeResult.New.Pressure is { } pressure)
        {
            _pressureHandlers?.Invoke(this, new ChangeResult<Pressure>(pressure, changeResult.Old?.Pressure));
        }
        base.RaiseEventsAndNotify(changeResult);
    }

    /// <summary>
    /// Convenience method to get the current Pressure. For frequent reads, use
    /// StartSampling() and StopSampling() in conjunction with the SampleBuffer.
    /// </summary>
    protected override Task<(Pressure? Pressure, Pressure? RawPsiMeasurement)> ReadSensor()
    {
        return Task.Run(() =>
        {
            BusComms.Write(mprlsMeasurementCommand);

            Thread.Sleep(5);

            while (true)
            {
                BusComms.Read(ReadBuffer.Span[0..1]);

                IsDevicePowered = BitHelpers.GetBitValue(ReadBuffer.Span[0], 6);
                IsDeviceBusy = BitHelpers.GetBitValue(ReadBuffer.Span[0], 5);
                HasMemoryIntegrityFailed = BitHelpers.GetBitValue(ReadBuffer.Span[0], 2);
                InternalMathSaturated = BitHelpers.GetBitValue(ReadBuffer.Span[0], 0);

                if (InternalMathSaturated)
                {
                    throw new InvalidOperationException("Sensor pressure has exceeded max value!");
                }
                if (HasMemoryIntegrityFailed)
                {
                    throw new InvalidOperationException("Sensor internal memory integrity check failed!");
                }
                if (!IsDeviceBusy)
                {
                    break;
                }
            }

            BusComms.Read(ReadBuffer.Span[0..4]);

            var rawPSIMeasurement = (ReadBuffer.Span[1] << 16) | (ReadBuffer.Span[2] << 8) | ReadBuffer.Span[3];

            var calculatedPSIMeasurement = (rawPSIMeasurement - 1677722) * (MAXIMUM_PSI - MINIMUM_PSI);
            calculatedPSIMeasurement /= 15099494 - 1677722;
            calculatedPSIMeasurement += MINIMUM_PSI;

            (Pressure? Pressure, Pressure? RawPsiMeasurement) conditions;

            conditions.RawPsiMeasurement = new Pressure(rawPSIMeasurement, Units.Pressure.UnitType.Psi);
            conditions.Pressure = new Pressure(calculatedPSIMeasurement, Units.Pressure.UnitType.Psi);

            return conditions;
        });
    }

    async Task<Pressure> ISensor<Pressure>.Read()
        => (await ReadSensor()).Pressure!.Value;
}