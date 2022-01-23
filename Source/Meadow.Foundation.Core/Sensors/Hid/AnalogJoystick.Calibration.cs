using System;
using Meadow.Units;
using VU = Meadow.Units.Voltage.UnitType;

namespace Meadow.Foundation.Sensors.Hid
{
    public partial class AnalogJoystick
    {
        /// <summary>
        /// Calibration class for new sensor types.  This allows new sensors
        /// to be used with this class.
        /// </summary>
        /// <remarks>
        ///     
        /// </remarks>
        public class JoystickCalibration
        {
            /// <summary>
            /// Voltage at horizonal center
            /// </summary>
            public Voltage HorizontalCenter { get; protected set; }

            /// <summary>
            /// Voltage at minimum horizonal position
            /// </summary>
            public Voltage HorizontalMin { get; protected set; }
            /// <summary>
            /// Voltage at maximum horizonal postion
            /// </summary>
            public Voltage HorizontalMax { get; protected set; }

            /// <summary>
            /// Voltage at vertical center
            /// </summary>
            public Voltage VerticalCenter { get; protected set; }
            /// <summary>
            /// Voltage at vertical minumum position
            /// </summary>
            public Voltage VerticalMin { get; protected set; }
            /// <summary>
            /// Voltage at vertical maximum position
            /// </summary>
            public Voltage VerticalMax { get; protected set; }

            /// <summary>
            /// Voltage range of center deadzone
            /// </summary>
            public Voltage DeadZone { get; protected set; }

            /// <summary>
            /// Joystick callibation
            /// </summary>
            /// <param name="voltage">VCC or maximum voltage</param>
            public JoystickCalibration(Voltage voltage)
            {
                HorizontalCenter = new Voltage(voltage.Volts / 2, VU.Volts);
                HorizontalMin = new Voltage(0, VU.Volts);
                HorizontalMax = voltage;

                VerticalCenter = new Voltage(voltage.Volts / 2, VU.Volts);
                VerticalMin = new Voltage(0, VU.Volts);
                VerticalMax = voltage;

                DeadZone = new Voltage(0.2f, VU.Volts);
            }

            /// <summary>
            /// Joystick callibation
            /// </summary>
            /// <param name="horizontalCenter"></param>
            /// <param name="horizontalMin"></param>
            /// <param name="horizontalMax"></param>
            /// <param name="verticalCenter"></param>
            /// <param name="verticalMin"></param>
            /// <param name="verticalMax"></param>
            /// <param name="deadZone"></param>
            public JoystickCalibration(Voltage horizontalCenter, Voltage horizontalMin, Voltage horizontalMax,
                Voltage verticalCenter, Voltage verticalMin, Voltage verticalMax, Voltage deadZone)
            {
                HorizontalCenter = horizontalCenter;
                HorizontalMin = horizontalMin;
                HorizontalMax = horizontalMax;

                VerticalCenter = verticalCenter;
                VerticalMin = verticalMin;
                VerticalMax = verticalMax;

                DeadZone = deadZone;
            }
        }
    }
}
