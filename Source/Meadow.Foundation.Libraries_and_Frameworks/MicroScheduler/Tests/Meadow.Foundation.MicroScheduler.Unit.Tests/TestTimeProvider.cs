namespace Meadow.Foundation.Scheduling.Tests;

public class TestTimeProvider : ITimeProvider
{
    private (double latitude, double longitude)? _location;

    public DateTimeOffset Now { get; set; } = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero); // Saturday, June 15, 2024, noon UTC

    public ValueTask<DateTimeOffset> GetUtcNow()
    {
        return new ValueTask<DateTimeOffset>(Now);
    }

    public ValueTask<(DateTimeOffset sunrise, DateTimeOffset sunset)> GetUtcSunriseAndSunset()
    {
        var date = Now.Date;
        var sunrise = new DateTimeOffset(date.Year, date.Month, date.Day, 6, 30, 0, TimeSpan.Zero); // 6:30 AM UTC
        var sunset = new DateTimeOffset(date.Year, date.Month, date.Day, 18, 30, 0, TimeSpan.Zero); // 6:30 PM UTC
        return new ValueTask<(DateTimeOffset, DateTimeOffset)>((sunrise, sunset));
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        _location = (latitude, longitude);
    }
}
