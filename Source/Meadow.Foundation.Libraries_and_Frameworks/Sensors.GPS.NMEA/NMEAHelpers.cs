using System;

namespace Meadow.Foundation.Sensors.GPS
{
    /// <summary>
    ///     Provide common functionality for the decode classes.
    /// </summary>
    public class NMEAHelpers
    {
        #region Methods

        /// <summary>
        ///     Extract the time of the reading.
        /// </summary>
        /// <param name="date">Date the reading was taken (this can be null)</param>
        /// <param name="time">String containing the time of the reading in the format hhmmss.sss</param>
        /// <returns>DateTime object containing the time.</returns>
        public static DateTime TimeOfReading(string date, string time)
        {
            var day = 1;
            var month = 1;
            var year = 2000;
            double d = 0;
            if (date != null)
            {
                if (double.TryParse(date, out d))
                {
                    day = (int) (d / 10000);
                    month = (int) ((d - (day * 10000)) / 100);
                    year = 2000 + ((int) d - (day * 10000) - (month * 100));
                }
                else
                {
                    throw new ArgumentException("Unable to decode the date");
                }
            }
            //
            int hour;
            int minute;
            int second;
            int milliseconds;
            double t = 0;
            if (double.TryParse(time, out t))
            {
                hour = (int) (t / 10000);
                minute = (int) ((t - (hour * 10000)) / 100);
                second = (int) (t - (hour * 10000) - (minute * 100));
                milliseconds = (int) (t - (int) t) * 100;
            }
            else
            {
                throw new ArgumentException("Unable to decode the time");
            }
            return new DateTime(year, month, day, hour, minute, second, milliseconds);
        }

        /// <summary>
        ///     Decode the degrees / minutes location and return a DMPosition.
        /// </summary>
        /// <param name="location">Location in the format dddmm.mmmm or ddmm.mmmm</param>
        /// <param name="direction">Direction of the reading, one of N, S, E, W.</param>
        /// <exception cref="ArgumentException">Throw if the location string cannot be decoded.</exception>
        /// <returns>DMPosition in degrees and minutes.</returns>
        public static DegreeMinutePosition DegreesMinutesDecode(string location, string direction)
        {
            double loc = 0;
            var position = new DegreeMinutePosition();

            if (double.TryParse(location, out loc))
            {
                position.Degrees = (int) (loc / 100);
                position.Minutes = loc - (position.Degrees * 100);
                switch (direction.ToLower())
                {
                    case "n":
                        position.Direction = DirectionIndicator.North;
                        break;
                    case "s":
                        position.Direction = DirectionIndicator.South;
                        break;
                    case "e":
                        position.Direction = DirectionIndicator.East;
                        break;
                    case "w":
                        position.Direction = DirectionIndicator.West;
                        break;
                    default:
                        position.Direction = DirectionIndicator.Unknown;
                        break;
                }
            }
            else
            {
                throw new ArgumentException("Invalid location");
            }
            return position;
        }

        #endregion Methods
    }
}