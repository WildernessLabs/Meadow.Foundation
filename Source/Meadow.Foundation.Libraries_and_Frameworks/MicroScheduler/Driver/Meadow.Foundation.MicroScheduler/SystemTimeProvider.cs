using Meadow.Foundation.Serialization;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Meadow.Foundation.Scheduling;

/// <summary>
/// Provides system time and sunrise/sunset calculations using online APIs and mathematical approximations.
/// </summary>
public class SystemTimeProvider : ITimeProvider, IDisposable
{
    private HttpClient? _httpClient;
    private DateTimeOffset? _lastSunTimeCheck = null;
    private (DateTimeOffset Sunrise, DateTimeOffset Sunset)? _sunTimes;
    private bool _disposed = false;

    private (float Latitude, float Longitude)? _location;

    /// <summary>
    /// Gets the time zone offset in hours from UTC.
    /// </summary>
    public float TimeZoneOffsetHours { get; }

    /// <summary>
    /// Gets or sets the geographic location used for sunrise/sunset calculations.
    /// Setting this clears any cached sunrise/sunset data.
    /// </summary>
    public (float Latitude, float Longitude)? GeographicLocation
    {
        get => _location;
        set
        {
            _location = value;
            _lastSunTimeCheck = null; // Clear cache when location changes
            _sunTimes = null;         // Clear cached sun times too
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTimeProvider"/> class with UTC time zone.
    /// </summary>
    public SystemTimeProvider()
        : this(0f)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTimeProvider"/> class with a specific time zone offset.
    /// </summary>
    /// <param name="timeZoneOffset">The time zone offset in hours from UTC.</param>
    public SystemTimeProvider(float timeZoneOffset)
    {
        TimeZoneOffsetHours = timeZoneOffset;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemTimeProvider"/> class with a specific time zone offset and HTTP client for testing.
    /// </summary>
    /// <param name="timeZoneOffset">The time zone offset in hours from UTC.</param>
    /// <param name="httpClient">The HTTP client to use for API calls.</param>
    public SystemTimeProvider(float timeZoneOffset, HttpClient httpClient)
        : this(timeZoneOffset)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    /// <returns>The current UTC date and time.</returns>
    public ValueTask<DateTimeOffset> GetUtcNow()
    {
        return new ValueTask<DateTimeOffset>(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Gets the UTC sunrise and sunset times for the current date.
    /// Uses online APIs when possible, falls back to mathematical approximations.
    /// Results are cached for the current day to avoid unnecessary API calls.
    /// </summary>
    /// <returns>A tuple containing the sunrise and sunset times in UTC.</returns>
    public async ValueTask<(DateTimeOffset sunrise, DateTimeOffset sunset)> GetUtcSunriseAndSunset()
    {
        var now = DateTime.UtcNow;

        // Check cache first
        if (_lastSunTimeCheck.HasValue
            && _lastSunTimeCheck.Value.Date == now.Date
            && _sunTimes != null)
        {
            return _sunTimes.Value;
        }

        if (_location != null)
        {
            // Try to get real sunrise/sunset times from API
            try
            {
                var apiResult = await GetSunriseAndSunsetFromApi(now);

                // Cache the successful result
                _sunTimes = apiResult;
                _lastSunTimeCheck = now;

                return apiResult;
            }
            catch (Exception)
            {
                // Fall back to estimated times if API fails
                return await CalculateEstimatedUtcSunriseAndSunset(now);
            }
        }

        // No location set, use estimated times
        return await CalculateEstimatedUtcSunriseAndSunset(now);
    }

    /// <summary>
    /// Internal class for deserializing sunrise-sunset.org API responses.
    /// </summary>
    internal class SunriseSunsetApiResponse
    {
        /// <summary>
        /// Gets or sets the results from the API.
        /// </summary>
        public SunriseSunsetResults results { get; set; } = new SunriseSunsetResults();

        /// <summary>
        /// Gets or sets the status of the API response.
        /// </summary>
        public string status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Internal class for deserializing the results portion of sunrise-sunset.org API responses.
    /// </summary>
    internal class SunriseSunsetResults
    {
        /// <summary>
        /// Gets or sets the sunrise time.
        /// </summary>
        public string sunrise { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sunset time.
        /// </summary>
        public string sunset { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the solar noon time.
        /// </summary>
        public string solar_noon { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the day length.
        /// </summary>
        public string day_length { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the civil twilight begin time.
        /// </summary>
        public string civil_twilight_begin { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the civil twilight end time.
        /// </summary>
        public string civil_twilight_end { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the nautical twilight begin time.
        /// </summary>
        public string nautical_twilight_begin { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the nautical twilight end time.
        /// </summary>
        public string nautical_twilight_end { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the astronomical twilight begin time.
        /// </summary>
        public string astronomical_twilight_begin { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the astronomical twilight end time.
        /// </summary>
        public string astronomical_twilight_end { get; set; } = string.Empty;
    }

    /// <summary>
    /// Gets sunrise and sunset times from the sunrise-sunset.org API.
    /// </summary>
    /// <param name="date">The date to get sunrise/sunset times for.</param>
    /// <returns>A tuple containing the sunrise and sunset times in UTC.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the API returns an error status.</exception>
    private async ValueTask<(DateTimeOffset sunrise, DateTimeOffset sunset)> GetSunriseAndSunsetFromApi(DateTimeOffset date)
    {
        if (_httpClient == null)
        {
            _httpClient = new HttpClient();
        }

        // Call sunrise-sunset.org API (free, no API key required)
        var url = $"https://api.sunrise-sunset.org/json" +
                 $"?lat={_location!.Value.Latitude:F6}" +
                 $"&lng={_location!.Value.Longitude:F6}" +
                 $"&date={date:yyyy-MM-dd}" +
                 $"&formatted=0"; // Get UTC times

        var response = await _httpClient.GetStringAsync(url);
        var apiResult = MicroJson.Deserialize<SunriseSunsetApiResponse>(response);

        if (apiResult?.status != "OK")
        {
            throw new InvalidOperationException($"API returned status: {apiResult?.status}");
        }

        // Parse the UTC times
        var sunrise = DateTimeOffset.Parse(apiResult.results.sunrise);
        var sunset = DateTimeOffset.Parse(apiResult.results.sunset);

        return (sunrise, sunset);
    }

    /// <summary>
    /// Calculates estimated sunrise and sunset times when API is not available or location is not set.
    /// </summary>
    /// <param name="date">The date to calculate sunrise/sunset times for.</param>
    /// <returns>A tuple containing the estimated sunrise and sunset times in UTC.</returns>
    private ValueTask<(DateTimeOffset sunrise, DateTimeOffset sunset)> CalculateEstimatedUtcSunriseAndSunset(DateTimeOffset date)
    {
        // If we have location information, use it for better estimates
        if (_location.HasValue)
        {
            return CalculateApproximateSunTimes(date, _location.Value);
        }

        // Basic fallback - fixed times
        var sunrise = new DateTimeOffset(date.Year, date.Month, date.Day, 6, 30, 0, TimeSpan.Zero);
        var sunset = new DateTimeOffset(date.Year, date.Month, date.Day, 18, 30, 0, TimeSpan.Zero);

        return new ValueTask<(DateTimeOffset, DateTimeOffset)>((sunrise, sunset));
    }

    /// <summary>
    /// Calculates approximate sunrise and sunset times using mathematical formulas based on latitude and day of year.
    /// </summary>
    /// <param name="date">The date to calculate sunrise/sunset times for.</param>
    /// <param name="location">The geographic location (latitude and longitude).</param>
    /// <returns>A tuple containing the calculated sunrise and sunset times in UTC.</returns>
    private ValueTask<(DateTimeOffset sunrise, DateTimeOffset sunset)> CalculateApproximateSunTimes(
        DateTimeOffset date,
        (float Latitude, float Longitude) location)
    {
        // Simple approximation based on latitude and day of year
        var dayOfYear = date.DayOfYear;
        var latitudeRad = location.Latitude * Math.PI / 180.0;

        // Simplified solar declination
        var declination = 23.45 * Math.Sin(2 * Math.PI * (284 + dayOfYear) / 365.0);
        var declinationRad = declination * Math.PI / 180.0;

        // Hour angle at sunrise/sunset
        var cosHourAngle = -Math.Tan(latitudeRad) * Math.Tan(declinationRad);

        // Handle polar day/night cases
        if (cosHourAngle > 1.0)
        {
            // Polar night - sun doesn't rise
            var midnight = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
            return new ValueTask<(DateTimeOffset, DateTimeOffset)>((midnight, midnight));
        }
        else if (cosHourAngle < -1.0)
        {
            // Polar day - sun doesn't set
            var noon = new DateTimeOffset(date.Year, date.Month, date.Day, 12, 0, 0, TimeSpan.Zero);
            return new ValueTask<(DateTimeOffset, DateTimeOffset)>((noon, noon));
        }

        var hourAngle = Math.Acos(cosHourAngle);
        var hourAngleDegrees = hourAngle * 180.0 / Math.PI;

        // Convert to hours
        var sunriseHour = 12 - hourAngleDegrees / 15.0;
        var sunsetHour = 12 + hourAngleDegrees / 15.0;

        // Apply longitude correction (4 minutes per degree)
        var longitudeCorrection = location.Longitude / 15.0;
        sunriseHour -= longitudeCorrection;
        sunsetHour -= longitudeCorrection;

        // Ensure times are within 24-hour range
        sunriseHour = ((sunriseHour % 24) + 24) % 24;
        sunsetHour = ((sunsetHour % 24) + 24) % 24;

        // Create DateTimeOffset objects
        var sunrise = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero)
            .AddHours(sunriseHour);
        var sunset = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero)
            .AddHours(sunsetHour);

        return new ValueTask<(DateTimeOffset, DateTimeOffset)>((sunrise, sunset));
    }

    /// <summary>
    /// Releases all resources used by the SystemTimeProvider.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the SystemTimeProvider and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
    /// <inheritdoc/>
    public void UpdateLocation(double latitude, double longitude)
    {
        _location = ((float)latitude, (float)longitude);
    }
}