using System;
using Meadow.Peripherals.Sensors.Location;
using Meadow.Peripherals.Sensors.Location.Gnss;

namespace Meadow.Foundation.Sensors.Location.Gnss.NmeaParsing
{
    /// <summary>
    /// Decode RMC - Recommended Minimum Specific GPS messages.
    /// </summary>
    public class RmcDecoder : INmeaDecoder
    {
        /// <summary>
        /// Position update received event.
        /// </summary>
        public event EventHandler<GnssPositionInfo> PositionCourseAndTimeReceived = delegate { };

        /// <summary>
        /// Prefix for the RMBC decoder.
        /// </summary>
        public string Prefix {
            get => "RMC";
        }

        /// <summary>
        /// Friendly name for the RMC messages.
        /// </summary>
        public string Name {
            get => "Recommended Minimum";
        }

        /// <summary>
        /// Process the data from a RMC
        /// </summary>
        /// <param name="data">String array of the message components for a RMC message.</param>
        public void Process(NmeaSentence sentence)
        {
            var position = new GnssPositionInfo();

            position.TalkerID = sentence.TalkerID;

            position.TimeOfReading = NmeaUtilities.TimeOfReading(sentence.DataElements[8], sentence.DataElements[0]);
            //Console.WriteLine($"Time of Reading:{position.TimeOfReading}UTC");

            if (sentence.DataElements[1].ToLower() == "a") {
                position.Valid = true;
            } else {
                position.Valid = false;
            }
            //Console.WriteLine($"valid:{position.Valid}");

            //if (position.Valid) {
                //Console.WriteLine($"will attempt to parse latitude; element[2]:{sentence.DataElements[2]}, element[3]:{sentence.DataElements[3]}");
                position.Position.Latitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[2], sentence.DataElements[3]);
                //Console.WriteLine($"will attempt to parse longitude; element[4]:{sentence.DataElements[4]}, element[5]:{sentence.DataElements[5]}");
                position.Position.Longitude = NmeaUtilities.DegreesMinutesDecode(sentence.DataElements[4], sentence.DataElements[5]);
                //Console.WriteLine("40");

                decimal speedInKnots;
                if(decimal.TryParse(sentence.DataElements[6], out speedInKnots)) {
                    position.SpeedInKnots = speedInKnots;
                }
                decimal courseHeading;
                if (decimal.TryParse(sentence.DataElements[7], out courseHeading)) {
                    position.CourseHeading = courseHeading;
                }

                if (sentence.DataElements[10].ToLower() == "e") {
                    position.MagneticVariation = CardinalDirection.East;
                } else if (sentence.DataElements[10].ToLower() == "w") {
                    position.MagneticVariation = CardinalDirection.West;
                } else {
                    position.MagneticVariation = CardinalDirection.Unknown;
                }
            //}
            //Console.WriteLine($"RMC Message Parsed, raising event");
            PositionCourseAndTimeReceived(this, position);
        }

    }
}