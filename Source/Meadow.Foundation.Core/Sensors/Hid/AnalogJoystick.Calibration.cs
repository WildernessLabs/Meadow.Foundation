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
            public Voltage HorizontalCenter { get; protected set; }
            public Voltage HorizontalMin { get; protected set; }
            public Voltage HorizontalMax { get; protected set; }

            public Voltage VerticalCenter { get; protected set; }
            public Voltage VerticalMin { get; protected set; }
            public Voltage VerticalMax { get; protected set; }

            public Voltage DeadZone { get; protected set; }

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
