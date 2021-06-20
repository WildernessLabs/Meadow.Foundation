using System;
using Meadow.Units;
using VU = Meadow.Units.Voltage.UnitType;

namespace Meadow.Foundation.Sensors.Environmental
{
    public partial class AnalogWaterLevel
    {
        /// <summary>
        ///     Calibration class for new sensor types.  This allows new sensors
        ///     to be used with this class.
        /// </summary>
        public class Calibration
        {

            public Voltage VoltsAtZero { get; protected set; } = new Voltage(1, VU.Volts);

            /// <summary>
            ///     Linear change in the sensor output (in millivolts) per 1 mm
            ///     change in temperature.
            /// </summary>
            public Voltage VoltsPerCentimeter { get; protected set; } = new Voltage(0.25, VU.Volts);

            /// <summary>
            ///     Default constructor. Create a new Calibration object with default values
            ///     for the properties.
            /// </summary>
            public Calibration()
            {
            }

            /// <summary>
            ///     Create a new Calibration object using the specified values.
            /// </summary>
            /// <param name="millivoltsPerMillimeter">Millivolt change per degree centigrade (from the data sheet).</param>
            public Calibration(Voltage voltsPerCentimeter, Voltage voltsAtZeo)
            {
                VoltsPerCentimeter = voltsPerCentimeter;
                VoltsAtZero = voltsAtZeo;
            }
        }
    }
}
