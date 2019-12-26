using System;

namespace Meadow.Foundation.DataLoggers
{
    /// <summary>
    ///     Sensor reading and the date and time the reading was taken.
    /// </summary>
    public class SensorReading
    {
        #region Properties

        /// <summary>
        ///     Name of the sensor reading.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     Value read from the sensor.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Date and time the reading was taken.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        #endregion Properties

        /// <summary>
        ///     Create a new SensorReading object.
        /// </summary>
        /// <param name="key">Name of the reading (e.g. temperature, humidity etc.)</param>
        /// <param name="value">Value read from the sensor.</param>
        /// <param name="createdAt">DateTime the sensor reading was taken.</param>
        public SensorReading(string key, string value, DateTime createdAt)
        {
            Key = key;
            Value = value;
            CreatedAt = createdAt;
        }
    }
}