using System;

namespace Meadow.Foundation.Sensors.GPS
{
    /// <summary>
    ///     Position recorded in degrees and minutes along with an indiction of the direction
    ///     of the position..
    /// </summary>
    public struct DegreeMinutePosition
    {
        public int Degrees;
        public double Minutes;
        public DirectionIndicator Direction;
    }

    /// <summary>
    ///     Satellite information to use in the GSV (Satellites in View) decoder.
    /// </summary>
    public struct Satellite
    {
        /// <summary>
        ///     Satellite ID.
        /// </summary>
        public string ID;

        /// <summary>
        ///     Angle of elevation.
        /// </summary>
        public int Elevation;

        /// <summary>
        ///     Satellite azimuth.
        /// </summary>
        public int Azimuth;

        /// <summary>
        ///     Signal to noise ratio of the signal.
        /// </summary>
        public int SignalTolNoiseRatio;
    }

    /// <summary>
    ///     Decoded data for the VTG - Course over ground and ground speed messages.
    /// </summary>
    public struct CourseOverGround
    {
        /// <summary>
        ///     True heading in degrees.
        /// </summary>
        public double TrueHeading;

        /// <summary>
        ///     Magnetic heading.
        /// </summary>
        public double MagneticHeading;

        /// <summary>
        ///     Speed measured in knots.
        /// </summary>
        public double Knots;

        /// <summary>
        ///     Speed measured in kilometers per hour.
        /// </summary>
        public double KPH;
    }

    /// <summary>
    ///     Hold the location taken from a GPS reading.
    /// </summary>
    public struct GPSLocation
    {
        /// <summary>
        ///     Time that the reading was taken.  The date component is fixed for each reading.
        /// </summary>
        public DateTime ReadingTime;

        /// <summary>
        ///     Latitude of the reading.
        /// </summary>
        public DegreeMinutePosition Latitude;

        /// <summary>
        ///     Longitude of the reading.
        /// </summary>
        public DegreeMinutePosition Longitude;

        /// <summary>
        ///     Quality of the fix.
        /// </summary>
        public FixType FixQuality;

        /// <summary>
        ///     Number of satellites used to generate the positional information.
        /// </summary>
        public int NumberOfSatellites;

        /// <summary>
        ///     Horizontal dilution of position (HDOP).
        /// </summary>
        public double HorizontalDilutionOfPrecision;

        /// <summary>
        ///     Altitude above mean sea level (m).
        public double Altitude;
    }

    /// <summary>
    ///     Active satellite information (GSA message information).
    /// </summary>
    public struct ActiveSatellites
    {
        /// <summary>
        ///     Dimensional fix type (No fix, 2D or 3D?)
        /// </summary>
        public DimensionalFixType Demensions;

        /// <summary>
        ///     Satellite selection type (Automatic or manual).
        /// </summary>
        public ActiveSatelliteSelection SatelliteSelection;

        /// <summary>
        ///     PRNs of the satellites used in the fix.
        /// </summary>
        public string[] SatellitesUsedForFix;

        /// <summary>
        ///     Dilution of precision for the reading.
        /// </summary>
        public double DilutionOfPrecision;

        /// <summary>
        ///     Horizontal dilution of precision for the reading.
        /// </summary>
        public double HorizontalDilutionOfPrecision;

        /// <summary>
        ///     Vertical dilution of precision for the reading.
        /// </summary>
        public double VerticalDilutionOfPrecision;
    }

    /// <summary>
    ///     Recommended Minimum message - this is the equivalent of GPS position and
    ///     course data.
    /// </summary>
    public struct PositionCourseAndTime
    {
        /// <summary>
        ///     Time the reading was generated.
        /// </summary>
        public DateTime TimeOfReading;

        /// <summary>
        ///     Indicate if the data is valid or not.
        /// </summary>
        public bool Valid;

        /// <summary>
        ///     Latitude reading.
        /// </summary>
        public DegreeMinutePosition Latitude;

        /// <summary>
        ///     Longitude reading.
        /// </summary>
        public DegreeMinutePosition Longitude;

        /// <summary>
        ///     Current speed in Knots.
        /// </summary>
        public double Speed;

        /// <summary>
        ///     Course in degrees (true heading).
        /// </summary>
        public double Course;

        /// <summary>
        ///     Magnetic variation.
        /// </summary>
        public DirectionIndicator MagneticVariation;
    }
}