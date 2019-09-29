using Meadow.Hardware;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Light
{
    public class ALSPT19315C
    {
        #region Member variables / fields

        /// <summary>
        ///     Analog port connected to the sensor.
        /// </summary>
        private readonly IAnalogInputPort _sensor;

        /// <summary>
        ///     Analog port connected to the reference voltage.
        /// </summary>
        private readonly IAnalogInputPort _referenceVoltagePort;

        /// <summary>
        ///     Reference voltage.
        /// </summary>
        private double _referenceVoltage;

        /// <summary>
        ///     Voltage being output by the sensor.
        /// </summary>
        public double GetVoltage()
        {
            return _referenceVoltagePort.Read().Result * 3.3;
        }

        #endregion Member variables / fields

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent it being used).
        /// </summary>
        private ALSPT19315C()
        {
        }

        /// <summary>
        ///     Create a new light sensor object using a static reference voltage.
        /// </summary>
        /// <param name="pin">AnalogChannel connected to the sensor.</param>
        /// <param name="referenceVoltage">Reference voltage.</param>
        public ALSPT19315C(IIODevice device, IPin pin, double referenceVoltage)
        {
            _sensor = device.CreateAnalogInputPort(pin);
            _referenceVoltagePort = null;
            _referenceVoltage = referenceVoltage;
        }

        /// <summary>
        ///     Create a new light sensor object using a dynaic reference voltage.
        /// </summary>
        /// <param name="pin">Analog channel connected to the sensor.</param>
        /// <param name="referenceVoltagePin">Analog channel connected to the reference voltage souce.</param>
        public ALSPT19315C(IIODevice device, IPin pin, IPin referenceVoltagePin)
        {
            _sensor = device.CreateAnalogInputPort(pin);
            _referenceVoltagePort = device.CreateAnalogInputPort(referenceVoltagePin);
        }

        #endregion Constructors
    }
}