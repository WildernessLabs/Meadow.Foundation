using System;
namespace Meadow.Foundation.Sensors.Motion
{
    public partial class Bno055
    {
        /// <summary>
        ///     Possible operating modes for the sensor.
        /// </summary>
        public static class OperatingModes
        {
            /// <summary>
            ///     Put the sensor into configuration mode.
            /// </summary>
            public const byte CONFIGURATION_MODE = 0x00;

            /// <summary>
            ///     Turn on the accelerometer only.
            /// </summary>
            public const byte ACCELEROMETER = 0x01;

            /// <summary>
            ///     Turn on the magnetometer only.
            /// </summary>
            public const byte MAGNETOMETER = 0x02;

            /// <summary>
            /// Turn on the gyroscope only.
            /// </summary>
            public const byte GYROSCOPE = 0x03;

            /// <summary>
            ///     Turn on the accelerometer and the magnetometer.
            /// </summary>
            public const byte ACCELEROMETER_MAGNETOMETER = 0x04;

            /// <summary>
            ///     Turn on the accelerometer and the gyroscope.
            /// </summary>
            public const byte ACCELEROMETER_GYROSCOPE = 0x05;

            /// <summary>
            ///     Turn on the magnetometer and the gyroscope.
            /// </summary>
            public const byte MAGNETOMETER_GYROSCOPE = 0x06;

            /// <summary>
            ///     Turn on the accelerometer, magnetometer and the gyroscope.
            /// </summary>
            public const byte ACCELEROMETER_MAGNETOMETER_GYROSCOPE = 0x07;

            /// <summary>
            ///     Put the sensor into fusion mode intertial measurement unit mode.
            /// </summary>
            public const byte INERTIAL_MEASUREMENT_UNIT = 0x08;

            /// <summary>
            ///     Operate as a compass (fusion mode).
            /// </summary>
            public const byte COMPASS = 0x09;

            /// <summary>
            ///     Similar to IMU mode but uses the magnetometer instead of the gyroscope (fusion mode).
            /// </summary>
            public const byte MAGNET_FOR_GYROSCOPE = 0x0a;

            /// <summary>
            ///     Same as the NineDegreesOfFreedom but with the Fast Magnetometer Calibration turned off (fusion mode).
            /// </summary>
            public const byte NINE_DEGREES_OF_FREEDOM_MAGNETOMETER_CALIBRATION_OFF = 0x0b;

            /// <summary>
            ///     Fusion of the accelerometer, gyroscope and megnetometer readings (fusion mode).
            /// </summary>
            public const byte NINE_DEGREES_OF_FREEDOM = 0x0c;

            /// <summary>
            ///     Maximum value for the operating modes.
            /// </summary>
            public const byte MAXIMUM_VALUE = 0x0c;
        }
    }
}
