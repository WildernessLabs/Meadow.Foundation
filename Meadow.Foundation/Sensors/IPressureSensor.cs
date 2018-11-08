namespace Netduino.Foundation.Sensors
{
    /// <summary>
    ///     Pressure sensor interface requirements.
    /// </summary>
    public interface IPressureSensor : ISensor
    {
        /// <summary>
        ///     The PressureChanged event will be raised when the difference (absolute value)
        ///     between the current Pressure reading and the last notified reading is greater
        ///     than the PressureChangeNotificationThreshold.
        /// </summary>
        event SensorFloatEventHandler PressureChanged;

        /// <summary>
        ///     Last value read from the Pressure sensor.
        /// </summary>
        float Pressure { get; }

        /// <summary>
        ///     Threshold value for the changed notification event.
        /// </summary>
        float PressureChangeNotificationThreshold { get; set; }
    }
}
