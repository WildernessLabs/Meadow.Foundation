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

    /// <summary>
    /// Updates the current location of the object using the specified latitude and longitude.
    /// </summary>
    /// <param name="latitude">The latitude of the new location, in decimal degrees. Must be in the range -90 to 90.</param>
    /// <param name="longitude">The longitude of the new location, in decimal degrees. Must be in the range -180 to 180.</param>
    void UpdateLocation(double latitude, double longitude);
}