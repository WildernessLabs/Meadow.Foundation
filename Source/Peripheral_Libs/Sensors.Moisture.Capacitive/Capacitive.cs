using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// Capacitive Soil Moisture Sensor
    /// </summary>
    public class Capacitive : IMoistureSensor
    {
        #region Properties

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        public IAnalogInputPort AnalogPort { get; protected set; }

        /// <summary>
        /// Last value read from the moisture sensor.
        /// </summary>
        public float Moisture { get; private set; }

        /// <summary>
        /// Boundary value of most moist soil
        /// </summary>
        public float MaximumMoisture { get; set; }

        /// <summary>
        /// Boundary value of most dry soil 
        /// </summary>
        public float MinimumMoisture { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private Capacitive() { }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the especified analog pin and a IO device.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="analogPin"></param>
        public Capacitive(IIODevice device, IPin analogPin, float minMoistureValue = 0f, float maxMoistureValue = 5f) : 
            this(device.CreateAnalogInputPort(analogPin), minMoistureValue, maxMoistureValue) { }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the especified AnalogInputPort.
        /// </summary>
        /// <param name="analogPort"></param>
        public Capacitive(IAnalogInputPort analogPort, float minimumMoisture = 0f, float maximumMoisture = 5f)
        {
            AnalogPort = analogPort;
            MinimumMoisture = minimumMoisture;
            MaximumMoisture = maximumMoisture;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the raw soil moisture current value
        /// </summary>
        /// <returns>Value ranges from 0.0f - 5.0f</returns>
        public float ReadRaw() 
        { 
            return AnalogPort.Read().Result;
        }

        /// <summary>
        /// Returns the soil moisture current value.
        /// </summary>
        /// <returns>Value ranges from 0 - 100</returns>
        public float Read()
        {
            Moisture = AnalogPort.Read().Result;

            if (MinimumMoisture > MaximumMoisture)
                return 100 - Map(Moisture, MaximumMoisture, MinimumMoisture, 0, 100);
            else
                return 100 - Map(Moisture, MinimumMoisture, MaximumMoisture, 0, 100);
        }

        /// <summary>
        /// Re-maps a value from one range (fromLow - fromHigh) to another (toLow - toHigh).
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fromLow"></param>
        /// <param name="fromHigh"></param>
        /// <param name="toLow"></param>
        /// <param name="toHigh"></param>
        /// <returns></returns>
        protected float Map(float value, float fromLow, float fromHigh, float toLow, float toHigh)
        {
            return (((toHigh - toLow) * (value - fromLow)) / (fromHigh - fromLow)) - toLow;
        }

        #endregion
    }
}