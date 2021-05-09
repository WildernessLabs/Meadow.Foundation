using System;
using Meadow.Units;

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
                HorizontalCenter = voltage / 2;
                HorizontalMin = 0;
                HorizontalMax = voltage;

                VerticalCenter = voltage / 2;
                VerticalMin = 0;
                VerticalMax = voltage;

                DeadZone = 0.2f;
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
