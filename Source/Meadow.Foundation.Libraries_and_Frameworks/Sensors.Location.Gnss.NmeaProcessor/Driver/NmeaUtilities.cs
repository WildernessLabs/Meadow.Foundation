using Meadow.Peripherals.Sensors.Location;
using System;

namespace Meadow.Foundation.Sensors.Location.Gnss;

/// <summary>
/// Provide common functionality for the decode classes.
/// </summary>
public static class NmeaUtilities
{
    /// <summary>
    /// Extract the time of the reading.
    /// </summary>
    /// <param name="date">Date the reading was taken (this can be null)</param>
    /// <param name="time">String containing the time of the reading in the format hhmmss.sss</param>
    /// <returns>DateTime object containing the time.</returns>
    public static DateTime? TimeOfReading(string? date, string time)
    {
        var day = 1;
        var month = 1;
        var year = 2000;

        if (date != null)
        {
            if (double.TryParse(date, out double d))
            {
                day = (int)(d / 10000);
                month = (int)((d - (day * 10000)) / 100);
                year = 2000 + ((int)d - (day * 10000) - (month * 100));
            }
            else
            {
                return null;
            }
        }

        int hour;
        int minute;
        int second;
        int milliseconds;

        if (double.TryParse(time, out double t))
        {
            hour = (int)(t / 10000);
            minute = (int)((t - (hour * 10000)) / 100);
            second = (int)(t - (hour * 10000) - (minute * 100));
            milliseconds = (int)(t - (int)t) * 100;
        }
        else
        {
            return null;
        }
        return new DateTime(year, month, day, hour, minute, second, milliseconds);
    }

    /// <summary>
    /// Parses a string decimal latitude and N/S direction indicator to a signed double
    /// </summary>
    /// <param name="position">The position in ddmm.mmmm format</param>
    /// <param name="directionIndicator">N or S</param>
    public static double ParseLatitude(string position, string directionIndicator)
    {
        position = position.Trim();
        var degrees = double.Parse(position[..2]);
        var minutes = double.Parse(position[2..]);
        var latitude = degrees + (minutes / 60d);
        if (directionIndicator.ToUpper() == "S")
        {
            latitude *= -1;
        }

        return latitude;
    }

    /// <summary>
    /// Parses a string decimal longitude and E/W direction indicator to a signed double
    /// </summary>
    /// <param name="position">The position in dddmm.mmmm format</param>
    /// <param name="directionIndicator">E or W</param>
    public static double ParseLongitude(string position, string directionIndicator)
    {
        position = position.Trim();
        var degrees = double.Parse(position[..3]);
        var minutes = double.Parse(position[3..]);
        var longitude = degrees + (minutes / 60d);
        if (directionIndicator.ToUpper() == "W")
        {
            longitude *= -1;
        }

        return longitude;
    }

    /// <summary>
    /// Decode the degrees / minutes location and return a DMPosition.
    /// </summary>
    /// <param name="location">Location in the format dddmm.mmmm or ddmm.mmmm</param>
    /// <param name="direction">Direction of the reading, one of N, S, E, W.</param>
    /// <exception cref="ArgumentException">Throw if the location string cannot be decoded.</exception>
    /// <returns>DMPosition in degrees and minutes.</returns>
    public static DegreesMinutesSecondsPosition? DegreesMinutesDecode(string location, string direction)
    {
        var position = new DegreesMinutesSecondsPosition();

        if (decimal.TryParse(location, out decimal loc))
        {
            position.Degrees = (int)(loc / 100);
            position.Minutes = loc - (position.Degrees * 100);
            position.Direction = direction.ToLower() switch
            {
                "n" => CardinalDirection.North,
                "s" => CardinalDirection.South,
                "e" => CardinalDirection.East,
                "w" => CardinalDirection.West,
                _ => CardinalDirection.Unknown,
            };
        }
        else
        {
            return null;
        }
        return position;
    }
}