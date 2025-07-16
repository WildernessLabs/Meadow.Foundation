using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.CurrentLoop;

/// <summary>
/// Emulates a current sensor that outputs a 4-20mA or 0-20mA current loop signal
/// proportional to the measured current within a specified range
/// </summary>
public class CurrentTransmitter : CurrentLoopSensor
{
    /// <summary>
    /// Gets the minimum current that this sensor can measure
    /// </summary>
    public Current MinimumSenseCurrent { get; }

    /// <summary>
    /// Gets the maximum current that this sensor can measure
    /// </summary>
    public Current MaximumSenseCurrent { get; }

    /// <summary>
    /// Initializes a new instance of the CurrentTransmitter class
    /// </summary>
    /// <param name="generator">The current loop generator to use for output</param>
    /// <param name="range">The current loop range (0-20mA or 4-20mA)</param>
    /// <param name="minimumSenseCurrent">The minimum current the sensor can measure</param>
    /// <param name="maximumSenseCurrent">The maximum current the sensor can measure</param>
    /// <exception cref="ArgumentException">Thrown when minimum current is greater than or equal to maximum current</exception>
    public CurrentTransmitter(
        ICurrentLoopGenerator generator,
        CurrentLoopRange range,
        Current minimumSenseCurrent,
        Current maximumSenseCurrent)
        : base(generator, range)
    {
        if (minimumSenseCurrent >= maximumSenseCurrent)
        {
            throw new ArgumentException("Minimum current must be less than maximum current",
                nameof(minimumSenseCurrent));
        }

        MinimumSenseCurrent = minimumSenseCurrent;
        MaximumSenseCurrent = maximumSenseCurrent;
    }

    /// <summary>
    /// Sets the sensor current and outputs the corresponding current loop signal.
    /// The output current is linearly interpolated between the sensor's current range
    /// based on where the measured current falls within the sensor's measurement range.
    /// </summary>
    /// <param name="measuredCurrent">The current to simulate measuring</param>
    /// <returns>A task representing the asynchronous operation</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when measured current is outside the sensor's minimum and maximum range
    /// </exception>
    /// <example>
    /// For a 4-20mA sensor with range 0A to 100A:
    /// - 0A outputs 4mA
    /// - 50A outputs 12mA
    /// - 100A outputs 20mA
    /// </example>
    public async Task SetCurrent(Current measuredCurrent)
    {
        // Validate current is within sensor range
        if (measuredCurrent < MinimumSenseCurrent || measuredCurrent > MaximumSenseCurrent)
        {
            throw new ArgumentOutOfRangeException(nameof(measuredCurrent),
                $"Current {measuredCurrent} is outside sensor range {MinimumSenseCurrent} to {MaximumSenseCurrent}");
        }

        // Determine current range based on loop type
        double minOutputCurrent = Range == CurrentLoopRange.Current_4_20 ? 4.0 : 0.0;
        double maxOutputCurrent = 20.0;

        // Calculate current span and position within range
        double currentSpan = MaximumSenseCurrent.Amps - MinimumSenseCurrent.Amps;
        double currentPosition = measuredCurrent.Amps - MinimumSenseCurrent.Amps;
        double currentRatio = currentPosition / currentSpan;

        // Linear interpolation to map measured current to output current
        double outputCurrentSpan = maxOutputCurrent - minOutputCurrent;
        double outputMilliamps = minOutputCurrent + (currentRatio * outputCurrentSpan);

        // Set the output current
        await SetOutputCurrent(new Current(outputMilliamps, Current.UnitType.Milliamps));
    }
}
