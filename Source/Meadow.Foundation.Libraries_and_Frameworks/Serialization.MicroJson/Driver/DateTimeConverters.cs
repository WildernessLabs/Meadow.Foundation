using System;

namespace Meadow.Foundation.Serialization;

internal static class DateTimeConverters
{
    /// <summary>
    /// Converts an ISO 8601 time/date format string into a DateTime object
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static DateTime FromIso8601(string date)
    {
        // Check if format contains the timezone ID, UTC reference
        // Neither means it's localtime
        bool utc = date.EndsWith("Z");

        string[] parts = date.Split(new char[] { 'T', 'Z', ':', '-', '.', '+', });

        string year = parts[0];
        string month = (parts.Length > 1) ? parts[1] : "1";
        string day = (parts.Length > 2) ? parts[2] : "1";
        string hour = (parts.Length > 3) ? parts[3] : "0";
        string minute = (parts.Length > 4) ? parts[4] : "0";
        string second = (parts.Length > 5) ? parts[5] : "0";
        string ms = (parts.Length > 6) ? parts[6] : "0";

        var dateTime = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day), Convert.ToInt32(hour), Convert.ToInt32(minute), Convert.ToInt32(second), Convert.ToInt32(ms));

        // If a time offset was specified instead of the UTC marker, add/subtract in the hours/minutes
        if ((utc == false) && (parts.Length >= 9))
        {
            string hourOffset = (parts.Length > 7) ? parts[7] : string.Empty;
            string minuteOffset = (parts.Length > 8) ? parts[8] : string.Empty;
            if (date.Contains("+"))
            {
                dateTime = dateTime.AddHours(Convert.ToDouble(hourOffset));
                dateTime = dateTime.AddMinutes(Convert.ToDouble(minuteOffset));
            }
            else
            {
                dateTime = dateTime.AddHours(-(Convert.ToDouble(hourOffset)));
                dateTime = dateTime.AddMinutes(-(Convert.ToDouble(minuteOffset)));
            }
        }

        return dateTime;
    }

    /// <summary>
    /// Converts a DateTime object into an ISO 8601 string in UTC format.
    /// </summary>
    /// <param name="dateTime">The DateTime to convert</param>
    /// <returns>DateTime as an ISO8601 string</returns>
    public static string ToIso8601(DateTime dateTime)
    {
        string result = dateTime.Year.ToString() + "-" +
                        FormatAsTwoDigits(dateTime.Month) + "-" +
                        FormatAsTwoDigits(dateTime.Day) + "T" +
                        FormatAsTwoDigits(dateTime.Hour) + ":" +
                        FormatAsTwoDigits(dateTime.Minute) + ":" +
                        FormatAsTwoDigits(dateTime.Second) + "." +
                        FormatAsThreeDigits(dateTime.Millisecond) + "Z";

        return result;
    }

    /// <summary>
    /// Ensures a two-digit number with leading zeros as necessary.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <returns>The two-digit string.</returns>
    private static string FormatAsTwoDigits(int value)
    {
        return value.ToString("D2");
    }

    /// <summary>
    /// Formats an integer as a three-digit string, adding leading zeros if necessary.
    /// </summary>
    /// <param name="value">The integer value to format.</param>
    /// <returns>The formatted three-digit string.</returns>
    private static string FormatAsThreeDigits(int value)
    {
        return value.ToString("D3");
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
    /// Converts an ASP.NET Ajax JSON string to DateTime
    /// </summary>
    /// <param name="ajax"></param>
    /// <returns></returns>
    public static DateTime FromASPNetAjax(string ajax)
    {
        string[] parts = ajax.Split(new char[] { '(', ')' });

        long ticks = Convert.ToInt64(parts[1]);

        // Create a Utc DateTime based on the tick count
        return new DateTime(ticks, DateTimeKind.Utc);
    }
}