namespace Netduino.Foundation.Sensors
{
    /// <summary>
    ///     Humidity sensor interface requirements.
    /// </summary>
    public interface IHumiditySensor : ISensor
    {
        /// <summary>
        ///     The Humidity changed event will be raised when the difference (absolute value)
        ///     between the current humidity reading and the last notified reading is greater
        ///     than the HumidityChangeNotificationThreshold.
        /// </summary>
        event SensorFloatEventHandler HumidityChanged;

        /// <summary>
        ///     Last value read from the humidity sensor.
        /// </summary>
        float Humidity { get; }

        /// <summary>
        ///     Threshold value for the changed notification event.
        /// </summary>
        float HumidityChangeNotificationThreshold { get; set; }
    }
}
