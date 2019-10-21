using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;
using System.Threading;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// FC-28-D Soil Hygrometer Detection Module + Soil Moisture Sensor    
    /// </summary>
    public class FC28 : IMoistureSensor
    {
        #region Properties

        /// <summary>
        /// Returns the analog input port
        /// </summary>
        public IAnalogInputPort AnalogPort { get; protected set; }

        /// <summary>
        /// Returns the digital output port
        /// </summary>
        public IDigitalOutputPort DigitalPort { get; protected set; }

        /// <summary>
        /// Last value read from the moisture sensor.
        /// </summary>
        public float Moisture { get; private set; }

        /// <summary>
        /// Boundary value of most dry soil 
        /// </summary>
        public float MinimumMoisture { get; set; }

        /// <summary>
        /// Boundary value of most moist soil
        /// </summary>
        public float MaximumMoisture { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor is private to prevent it being called.
        /// </summary>
        private FC28() { }

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the especified analog pin, digital pin and IO device.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public FC28(IIODevice device, IPin analogPin, IPin digitalPin, float minimumMoisture = 0f, float maximumMoisture = 5f) : 
            this (device.CreateAnalogInputPort(analogPin), device.CreateDigitalOutputPort(digitalPin), minimumMoisture, maximumMoisture) { }

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the especified analog pin and digital pin.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public FC28(IAnalogInputPort analogPort, IDigitalOutputPort digitalPort, float minimumMoisture = 0f, float maximumMoisture = 5f)
        {
            AnalogPort = analogPort;
            DigitalPort = digitalPort;
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
            DigitalPort.State = true;
            Thread.Sleep(5);
            float value = AnalogPort.Read().Result;
            DigitalPort.State = false;

            return value;
        }

        /// <summary>
        /// Returns the soil moisture current value.
        /// </summary>
        /// <returns>Value ranges from 0 - 100</returns>
        public float Read()
        {
            DigitalPort.State = true;
            Thread.Sleep(5);
            Moisture = AnalogPort.Read().Result;
            DigitalPort.State = false;

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