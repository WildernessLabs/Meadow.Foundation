namespace Meadow.Foundation.Sensors.GPS
{
    /// <summary>
    ///     Decoder for GGA messages.
    /// </summary>
    public class GGADecoder : NMEADecoder
    {
        #region Delegates and events.

        /// <summary>
        ///     Delegate for the position update received event.
        /// </summary>
        /// <param name="location">Location data received.</param>
        /// <param name="sender">Reference to the object generating the event.</param>
        public delegate void PositionReceived(object sender, GPSLocation location);

        /// <summary>
        ///     Position update received event.
        /// </summary>
        public event PositionReceived OnPositionReceived;

        #endregion Delegates and events.

        #region NMEADecoder methods & properties

        /// <summary>
        ///     Prefix for the GGA decoder.
        /// </summary>
        public override string Prefix
        {
            get { return "$GPGGA"; }
        }

        /// <summary>
        ///     Friendly name for the GGA messages.
        /// </summary>
        public override string Name
        {
            get { return "Global Postioning System Fix Data"; }
        }

        /// <summary>
        ///     Process the data from a GGA message.
        /// </summary>
        /// <param name="data">String array of the message components for a CGA message.</param>
        public override void Process(string[] data)
        {
            if (OnPositionReceived != null)
            {
                var invalidFieldCount = 0;
                for (var index = 1; index <= 9; index++)
                {
                    if ((data[index] == null) || (data[index] == ""))
                    {
                        invalidFieldCount++;
                    }
                }
                if (invalidFieldCount == 0)
                {
                    var location = new GPSLocation();
                    location.ReadingTime = NMEAHelpers.TimeOfReading(null, data[1]);
                    location.Latitude = NMEAHelpers.DegreesMinutesDecode(data[2], data[3]);
                    location.Longitude = NMEAHelpers.DegreesMinutesDecode(data[4], data[5]);
                    location.FixQuality = (FixType) Converters.Integer(data[6]);
                    location.NumberOfSatellites = Converters.Integer(data[7]);
                    location.HorizontalDilutionOfPrecision = Converters.Double(data[8]);
                    location.Altitude = Converters.Double(data[9]);
                    OnPositionReceived(this, location);
                }
            }
        }

        #endregion NMEADecoder methods & properties
    }
}