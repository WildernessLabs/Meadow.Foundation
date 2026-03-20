using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.CurrentLoop;

/// <summary>
/// Emulates a pressure sensor that outputs a 4-20mA or 0-20mA current loop signal
/// proportional to the measured pressure within a specified range
/// </summary>
public class PressureTransmitter : CurrentLoopSensor
{
    /// <summary>
    /// Gets the minimum pressure that this sensor can measure
    /// </summary>
    public Pressure MinimumSensePressure { get; }

    /// <summary>
    /// Gets the maximum pressure that this sensor can measure
    /// </summary>
    public Pressure MaximumSensePressure { get; }

    /// <summary>
    /// Initializes a new instance of the PressureTransmitter class
    /// </summary>
    /// <param name="generator">The current loop generator to use for output</param>
    /// <param name="range">The current loop range (0-20mA or 4-20mA)</param>
    /// <param name="minimumSensePressure">The minimum pressure the sensor can measure</param>
    /// <param name="maximumSensePressure">The maximum pressure the sensor can measure</param>
    /// <exception cref="ArgumentException">Thrown when minimum pressure is greater than or equal to maximum pressure</exception>
    public PressureTransmitter(
        ICurrentLoopGenerator generator,
        CurrentLoopRange range,
        Pressure minimumSensePressure,
        Pressure maximumSensePressure)
        : base(generator, range)
    {
        if (minimumSensePressure >= maximumSensePressure)
        {
            throw new ArgumentException("Minimum pressure must be less than maximum pressure",
                nameof(minimumSensePressure));
        }

        MinimumSensePressure = minimumSensePressure;
        MaximumSensePressure = maximumSensePressure;
    }

    /// <summary>
    /// Sets the sensor pressure and outputs the corresponding current loop signal.
    /// The output current is linearly interpolated between the sensor's current range
    /// based on where the pressure falls within the sensor's measurement range.
    /// </summary>
    /// <param name="pressure">The pressure to simulate</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when pressure is outside the sensor's minimum and maximum range
    /// </exception>
    /// <example>
    /// For a 4-20mA sensor with range 0 PSI to 1000 PSI:
    /// - 0 PSI outputs 4mA
    /// - 500 PSI outputs 12mA
    /// - 1000 PSI outputs 20mA
    /// </example>
    public async Task SetPressure(Pressure pressure)
    {
        // Validate pressure is within sensor range
        if (pressure < MinimumSensePressure || pressure > MaximumSensePressure)
        {
            throw new ArgumentOutOfRangeException(nameof(pressure),
                $"Pressure {pressure} is outside sensor range {MinimumSensePressure} to {MaximumSensePressure}");
        }

        // Determine current range based on loop type
        double minCurrent = Range == CurrentLoopRange.Current_4_20 ? 4.0 : 0.0;
        double maxCurrent = 20.0;

        // Calculate pressure span and position within range
        double pressureSpan = MaximumSensePressure.StandardAtmosphere - MinimumSensePressure.StandardAtmosphere;
        double pressurePosition = pressure.StandardAtmosphere - MinimumSensePressure.StandardAtmosphere;
        double pressureRatio = pressurePosition / pressureSpan;

        // Linear interpolation to map pressure to current
        double currentSpan = maxCurrent - minCurrent;
        double outputMilliamps = minCurrent + (pressureRatio * currentSpan);

        // Set the output current
        await SetOutputCurrent(new Current(outputMilliamps, Current.UnitType.Milliamps));
    }
}
