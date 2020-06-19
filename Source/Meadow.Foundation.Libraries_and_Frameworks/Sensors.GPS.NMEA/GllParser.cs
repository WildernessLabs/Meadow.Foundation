﻿using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing
{
    /// <summary>
    /// Process GLL (Geographic position Latitude / Longitude) messages from a
    /// GPS receiver.
    /// </summary>
    public class GllParser : INmeaParser
    {
        /// <summary>
        ///     Delegate for the GLL data received event.
        /// </summary>
        /// <param name="location">Location data to pass to the application.</param>
        /// <param name="sender">Reference to the object generating the event.</param>
        public delegate void GeographicLatitudeLongitudeReceived(object sender, GnssPositionInfo location);

        /// <summary>
        ///     Event raised when valid GLL data is received.
        /// </summary>
        public event GeographicLatitudeLongitudeReceived OnGeographicLatitudeLongitudeReceived;

        /// <summary>
        ///     Prefix for the GLL (Geographic position Latitude / Longitude) decoder.
        /// </summary>
        public string Prefix
        {
            get { return "$GPGLL"; }
        }

        /// <summary>
        ///     Friendly name for the GLL messages.
        /// </summary>
        public string Name
        {
            get { return"GLL - Global Postioning System Fix Data"; }
        }

        /// <summary>
        ///     Process the data from a GLL message.
        /// </summary>
        /// <param name="data">String array of the message components for a GLL message.</param>
        public void Process(NmeaSentence sentence)
        {
            if (OnGeographicLatitudeLongitudeReceived != null)
            {
                //
                //  Status is stored in element 7 (position 6), A = valid, V = not valid.
                //
                if (sentence.DataElements[5].ToLower() == "a")
                {
                    var location = new GnssPositionInfo();
                    location.Position.Latitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[0], sentence.DataElements[1]);
                    location.Position.Longitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[2], sentence.DataElements[3]);
                    location.TimeOfReading = NmeaUtilities.TimeOfReading(null, sentence.DataElements[4]);
                    OnGeographicLatitudeLongitudeReceived(this, location);
                }
            }
        }
    }
}