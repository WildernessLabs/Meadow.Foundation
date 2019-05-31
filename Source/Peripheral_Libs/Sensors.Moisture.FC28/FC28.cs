using Meadow.Hardware;
using Meadow.Peripherals.Sensors.Moisture;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Moisture
{
    /// <summary>
    /// FC-28-D Soil Hygrometer Detection Module + Soil Moisture Sensor
    /// 
    /// Note: This class is not yet implemented.
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
        public FC28(IIODevice device, IPin analogPin, IPin digitalPin) : 
            this (device.CreateAnalogInputPort(analogPin), device.CreateDigitalOutputPort(digitalPin)) { }

        /// <summary>
        /// Creates a FC28 soil moisture sensor object with the especified analog pin and digital pin.
        /// </summary>
        /// <param name="analogPort"></param>
        /// <param name="digitalPort"></param>
        public FC28(IAnalogInputPort analogPort, IDigitalOutputPort digitalPort)
        {
            AnalogPort = analogPort;
            DigitalPort = digitalPort;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the soil moisture current value.
        /// </summary>
        /// <returns>Value ranges from 0 - 100</returns>
        public async Task<float> Read()
        {
            DigitalPort.State = true;
            Thread.Sleep(5);
            var sample = AnalogPort.Read();
            DigitalPort.State = false;

            Moisture = 100 - Map(sample, 0, 1023, 0, 100);
            return Moisture;
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