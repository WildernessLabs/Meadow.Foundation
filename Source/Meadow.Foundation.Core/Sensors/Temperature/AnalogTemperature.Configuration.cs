﻿namespace Meadow.Foundation.Sensors.Temperature
{
    public partial class AnalogTemperature
    {
        /// <summary>
        /// Calibration class for AnalogTemperature.  
        /// </summary>
        /// <remarks>
        /// The default settings for this object are correct for the TMP35.
        /// </remarks>
        public class Calibration
        {
            /// <summary>
            /// Sample reading as specified in the product data sheet.
            /// Measured in degrees Centigrade.
            /// </summary>
            public double SampleReading { get; protected set; } = 25;

            /// <summary>
            /// Millivolt reading the sensor will generate when the sensor
            /// is at the SampleReading temperature.  This value can be
            /// obtained from the data sheet. 
            /// </summary>
            public double MillivoltsAtSampleReading { get; protected set; } = 250;

            /// <summary>
            /// Linear change in the sensor output (in millivolts) per 1 degree C
            /// change in temperature.
            /// </summary>
            public double MillivoltsPerDegreeCentigrade { get; protected set; } = 10;

            /// <summary>
            /// Default constructor.  Create a new Calibration object with default values
            /// for the properties.
            /// </summary>
            public Calibration()
            {
            }

            /// <summary>
            /// Create a new Calibration object using the specified values.
            /// </summary>
            /// <param name="degreesCelsiusSampleReading">Sample reading from the data sheet.</param>
            /// <param name="millivoltsAtSampleReading">Millivolts output at the sample reading (from the data sheet).</param>
            /// <param name="millivoltsPerDegreeCentigrade">Millivolt change per degree centigrade (from the data sheet).</param>
            public Calibration(double degreesCelsiusSampleReading,
                               double millivoltsAtSampleReading,
                               double millivoltsPerDegreeCentigrade)
            {
                SampleReading = degreesCelsiusSampleReading;
                MillivoltsAtSampleReading = millivoltsAtSampleReading;
                MillivoltsPerDegreeCentigrade = millivoltsPerDegreeCentigrade;
            }
        }
    }
}