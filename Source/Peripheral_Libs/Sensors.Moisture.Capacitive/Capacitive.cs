using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// Capacitive Soil Moisture Sensor
    /// 
    /// Note: This class is not yet implemented.
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
        public Capacitive(IIODevice device, IPin analogPin) : 
            this(device.CreateAnalogInputPort(analogPin)) { }

        /// <summary>
        /// Creates a Capacitive soil moisture sensor object with the especified AnalogInputPort.
        /// </summary>
        /// <param name="analogPort"></param>
        public Capacitive(IAnalogInputPort analogPort)
        {
            AnalogPort = analogPort;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the soil moisture current value.
        /// </summary>
        /// <returns>Value ranges from 0 - 100</returns>
        public async Task<float> Read()
        {
            Moisture = AnalogPort.Read();
            return 100 - Map(Moisture, 0, 1023, 0, 100);
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