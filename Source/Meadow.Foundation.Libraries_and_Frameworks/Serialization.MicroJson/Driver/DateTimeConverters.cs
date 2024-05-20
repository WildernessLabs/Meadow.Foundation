using System;
using System.Globalization;

namespace Meadow.Foundation.Serialization;

internal static class DateTimeConverters
{
    /// <summary>
    /// Converts an ISO 8601 time/input formatted string into a DateTime object.
    /// </summary>
    /// <param name="input">Date time string in ISO 8601 format.</param>
    /// <returns>A new DateTime object representing the input date and time.</returns>
    /// <exception cref="ArgumentException">Thrown when the input string is not in the expected ISO 8601 format.</exception>
    public static DateTime FromIso8601(string input)
    {
        // Check if format contains the timezone ID, UTC reference
        bool isUtc = input.EndsWith("Z");

        if (DateTime.TryParseExact(input, "yyyy-MM-ddTHH:mm:ss.FFFK", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTime))
        {
            if (!isUtc && input.Length > 19)
            {
                string offset = input[19..];
                TimeSpan timeOffset = TimeSpan.Parse(offset);
                dateTime = dateTime.Add(timeOffset);
            }

            return dateTime;
        }

        throw new ArgumentException("Invalid ISO 8601 format", nameof(input));
    }

    /// <summary>
    /// Converts a DateTime object into an ISO 8601 string in UTC format.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>DateTime as an ISO 8601 string in UTC format.</returns>
    public static string ToIso8601(DateTime dateTime)
    {
        //return $"{dateTime:yyyy-MM-ddTHH:mm:ss.FFFZ}";
        return dateTime.ToString("o", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a DateTime object into an ISO 8601 string in UTC format.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>DateTime as an ISO 8601 string in UTC format.</returns>
    public static string ToIso8601(DateTimeOffset dateTime)
    {
        return dateTime.ToString("o", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts a DateTime to the ASP.NET Ajax JSON format.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>A string representation of the DateTime in ASP.NET Ajax JSON format.</returns>
    public static string ToASPNetAjax(DateTime dateTime)
    {
        return $@"\/Date({dateTime.Ticks})\/";
    }

    /// <summary>
    /// Converts a DateTime to the ASP.NET Ajax JSON format.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert.</param>
    /// <returns>A string representation of the DateTime in ASP.NET Ajax JSON format.</returns>
    public static string ToASPNetAjax(DateTimeOffset dateTime)
    {
        return $@"\/Date({dateTime.Ticks})\/";
    }

    /// <summary>
    /// Converts an ASP.NET Ajax JSON string to DateTime
    /// </summary>
    /// <param name="ajax">The ajax formatted date time string</param>
    /// <returns>A new DateTime object representing the input date and time.</returns>
    public static DateTime FromASPNetAjax(string ajax)
    {
        string[] parts = ajax.Split(new char[] { '(', ')' });

        long ticks = Convert.ToInt64(parts[1]);

        return new DateTime(ticks, DateTimeKind.Utc);
    }
}