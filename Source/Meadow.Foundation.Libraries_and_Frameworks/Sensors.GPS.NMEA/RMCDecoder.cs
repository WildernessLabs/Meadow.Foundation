using Meadow.Peripherals.Sensors.Location;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.GPS
{
    /// <summary>
    ///     Decode RMC - Recommended Minimum Specific GPS messages.
    /// </summary>
    public class RMCDecoder : INMEADecoder
    {
        #region NMEADecoder methods & properties

        /// <summary>
        ///     Prefix for the GGA decoder.
        /// </summary>
        public string Prefix
        {
            get { return "$GPRMC"; }
        }

        /// <summary>
        ///     Friendly name for the GGA messages.
        /// </summary>
        public string Name
        {
            get { return "Global Postioning System Fix Data"; }
        }

        /// <summary>
        ///     Process the data from a GGA message.
        /// </summary>
        /// <param name="data">String array of the message components for a CGA message.</param>
        public void Process(string[] data)
        {
            if (OnPositionCourseAndTimeReceived != null)
            {
                //var position = new PositionCourseAndTime();
                var position = new GnssPositionInfo();
                position.TimeOfReading = NMEAHelpers.TimeOfReading(data[9], data[1]);
                if (data[2].ToLower() == "a")
                {
                    position.Valid = true;
                }
                else
                {
                    position.Valid = false;
                }
                position.Position.Latitude = NMEAHelpers.DegreesMinutesDecode(data[3], data[4]);
                position.Position.Longitude = NMEAHelpers.DegreesMinutesDecode(data[5], data[6]);
                position.SpeedInKnots = decimal.Parse(data[7]);
                position.CourseHeading = decimal.Parse(data[8]);
                if (data[10].ToLower() == "e")
                {
                    position.MagneticVariation = CardinalDirection.East;
                }
                else
                {
                    position.MagneticVariation = CardinalDirection.West;
                }
                OnPositionCourseAndTimeReceived(this, position);
            }
        }

        #endregion NMEADecoder methods & properties

        #region Delegates and events.

        /// <summary>
        ///     Delegate for the position update received event.
        /// </summary>
        /// <param name="positionCourseAndTime">Position, course and time information.</param>
        /// <param name="sender">Reference to the object generating the event.</param>
        public delegate void PositionCourseAndTimeReceived(object sender, GnssPositionInfo positionCourseAndTime);

        /// <summary>
        ///     Position update received event.
        /// </summary>
        public event PositionCourseAndTimeReceived OnPositionCourseAndTimeReceived;

        #endregion Delegates and events.
    }
}