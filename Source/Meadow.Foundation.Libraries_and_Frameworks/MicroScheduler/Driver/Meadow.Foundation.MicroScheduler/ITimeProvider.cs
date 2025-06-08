using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Defines methods for providing time information and sunrise/sunset calculations.
/// </summary>
public interface ITimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current UTC date and time.</returns>
    ValueTask<DateTimeOffset> GetUtcNow();

    /// <summary>
    /// Gets the UTC sunrise and sunset times for the current date.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with the sunrise and sunset times in UTC.</returns>
    ValueTask<(DateTimeOffset sunrise, DateTimeOffset sunset)> GetUtcSunriseAndSunset();
}