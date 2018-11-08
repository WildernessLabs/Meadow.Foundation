using Meadow;
using Meadow.Hardware;

namespace Netduino.Fountation.Sensors.Light
{
    public class ALSPT19315C
    {
        #region Properties

        /// <summary>
        ///     Voltage being output by the sensor.
        /// </summary>
        public double Voltage
        {
            get
            {
                if (_referenceVoltagePort != null)
                {
                    _referenceVoltage = _referenceVoltagePort.Value * 3.3;
                }
                return _sensor.Value * _referenceVoltage;
            }
        }
        #endregion Properties

        #region Member variables / fields

        /// <summary>
        ///     Analog port connected to the sensor.
        /// </summary>
        private readonly AnalogInputPort _sensor;

        /// <summary>
        ///     Analog port connected to the reference voltage.
        /// </summary>
        private readonly AnalogInputPort _referenceVoltagePort;

        /// <summary>
        ///     Reference voltage.
        /// </summary>
        private double _referenceVoltage;

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
        /// <param name="sensorChannel">AnalogChannel connected to the sensor.</param>
        /// <param name="referenceVoltage">Reference voltage.</param>
        public ALSPT19315C(Cpu.AnalogChannel sensorChannel, double referenceVoltage)
        {
            _sensor = new AnalogInputPort(sensorChannel);
            _referenceVoltagePort = null;
            _referenceVoltage = referenceVoltage;
        }

        /// <summary>
        ///     Create a new light sensor object using a dynaic reference voltage.
        /// </summary>
        /// <param name="sensorChannel">Analog channel connected to the sensor.</param>
        /// <param name="referenceVoltageChannel">Analog channel connected to the reference voltage souce.</param>
        public ALSPT19315C(Cpu.AnalogChannel sensorChannel, Cpu.AnalogChannel referenceVoltageChannel)
        {
            _sensor = new AnalogInputPort(sensorChannel);
            _referenceVoltagePort = new AnalogInputPort(referenceVoltageChannel);
        }

        #endregion Constructors
    }
}