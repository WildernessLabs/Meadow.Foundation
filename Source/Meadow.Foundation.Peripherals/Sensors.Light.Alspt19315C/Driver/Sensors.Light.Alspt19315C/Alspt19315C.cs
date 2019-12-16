﻿using Meadow.Hardware;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Light
{
    public class Alspt19315C
    {
        #region Member variables / fields

        /// <summary>
        ///     Analog port connected to the sensor.
        /// </summary>
        private readonly IAnalogInputPort _sensor;

        /// <summary>
        ///     Voltage being output by the sensor.
        /// </summary>
        public Task<float> GetVoltage()
        {
            return _sensor.Read();
        }

        #endregion Member variables / fields

        #region Constructors

        /// <summary>
        ///     Default constructor (private to prevent it being used).
        /// </summary>
        private Alspt19315C()
        {
        }

        /// <summary>
        ///     Create a new light sensor object using a static reference voltage.
        /// </summary>
        /// <param name="pin">AnalogChannel connected to the sensor.</param>
        public Alspt19315C(IIODevice device, IPin pin)
        {
            _sensor = device.CreateAnalogInputPort(pin);
        }

        #endregion Constructors
    }
}