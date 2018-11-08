namespace Netduino.Foundation.Sensors
{
    /// <summary>
    ///     Light sensor interface requirements.
    /// </summary>
    public interface ILightSensor : ISensor
    {
        /// <summary>
        ///     The LightChanged event will be raised when the difference (absolute value)
        ///     between the current Light reading and the last notified reading is greater
        ///     than the LightChangeNotificationThreshold.
        /// </summary>
        event SensorFloatEventHandler LightLevelChanged;

        /// <summary>
        ///     Last value read from the Light sensor.
        /// </summary>
        float Luminosity { get; }

        /// <summary>
        ///     Threshold value for the changed notification event.
        /// </summary>
        float LightLevelChangeNotificationThreshold { get; set; }
    }
}